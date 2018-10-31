using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;

public class Player_Behavior : NetworkBehaviour {
	
	private float speed = 10f; //2.5f is normal speed
	//private float boostSpeed = 5f;

	public GameObject currentTile;
	public GameObject targetTile_forward;
	public GameObject targetTile_backward;
	
	[HideInInspector]
	public int player_num;

	[HideInInspector]
	[SyncVar(hook = "OnChangeAnimationState")]
	public AnimationStates animationState=AnimationStates.HumanoidIdle;

	[HideInInspector]
	public int numSpacesToMove = 0; //how many tiles the player can move, as determined by dice roll
	public int max_tiles_per_turn; //upper bound on dice roll

	private bool isChangingTarget = false; //disable checking if the player has reached the target while the target is being changed

	private bool isMoving = false; //player is currently moving to destination tile
	private bool isMovingForward = true;
	private bool isMyTurn = false; //is currently player's turn, allowing actions
	private bool doneMoving = false; //true when player has used all available moves
	private bool atFork = false;

	private GameObject text_debug;//debug textbox on screen
	private GameObject text_turn;//turn textbox on screen
	void Start () {
		if(isLocalPlayer) GameObject.Find("Main Camera").GetComponent<CameraController>().SetPlayer(gameObject);
		text_debug = GameObject.Find("DebugText");
		text_turn = GameObject.Find("TurnText");
	}
	
	//returns false if target has been chosen, true if the target is still being chosen (fork)
	private bool targetNextTile(bool forwards){
		if(forwards){
			currentTile = targetTile_forward;
			List<GameObject> targets = currentTile.GetComponent<Tile>().nextTiles;
			if(targets.Count > 1){ //currently only supports forks with 2 choices
					Debug.Log("Fork in the road! Waiting for player input...");
				atFork = true;
				return true;
			}else{
					Debug.Log("linear path");
				targetTile_forward = targets[0];
				return false;
			}
		}else{
			currentTile = targetTile_backward;

			List<GameObject> targets = currentTile.GetComponent<Tile>().previousTiles;
			if(targets.Count > 1){ //currently only supports forks with 2 choices
					Debug.Log("Fork in the road! Waiting for player input...");
				atFork = true;
				return true;
			}else{
					Debug.Log("linear path");
				targetTile_backward = targets[0];
				return false;
			}
		}
	}

	void Update () {
		//all clients can move this gameObject
		if(isMoving){
			//move to next tile
			Vector3 target;
			if(isMovingForward){
				target = targetTile_forward.transform.GetChild(player_num).position;
			}else{
				target = targetTile_backward.transform.GetChild(player_num).position;
			}
			 
			gameObject.transform.LookAt(target);
			gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,target,speed*Time.deltaTime);
		}

		//only local player can detect input
		if(!isLocalPlayer)return;

		if(isMyTurn){
			text_turn.GetComponent<Text>().text = "Your turn!";
			if(!isMoving) text_debug.GetComponent<Text>().text ="Press SPACE to continue!";
			
		}else{
			text_turn.GetComponent<Text>().text = "Not your turn.";
			text_debug.GetComponent<Text>().text = "";
		}

		if(isMoving){
			if(isMyTurn){
				//display detailed debugging info for localPlayer
				String t = numSpacesToMove+" spaces left to move\n";

				if(atFork){
					t+="Press the LEFT or RIGHT arrow key to continue!";
				}else{
					if(isMovingForward){
						t+="Distance to target = "+Vector3.Distance(gameObject.transform.position,targetTile_forward.transform.GetChild(player_num).position);
					}else{
						t+="Distance to target = "+Vector3.Distance(gameObject.transform.position,targetTile_backward.transform.GetChild(player_num).position);
					}
					
				} 
				text_debug.GetComponent<Text>().text = t;
			}

			//when player has reached destination; will be replaced when the player can move multiple tiles per turn
			if(!isChangingTarget && (
				(isMovingForward && gameObject.transform.position == targetTile_forward.transform.GetChild(player_num).position)
			|| (!isMovingForward && gameObject.transform.position == targetTile_backward.transform.GetChild(player_num).position)
			)){
				if(numSpacesToMove <= 1 && !doneMoving){
					numSpacesToMove=0; //for display purposes
					doneMoving=true;//prevents multiple messages being sent
					NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerStop));
				}else if(numSpacesToMove > 1){
					numSpacesToMove-=1;
						Debug.Log(numSpacesToMove+" spaces left to move");
					//message server to changetarget
					isChangingTarget=true;
					NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerTargetChange));
				}
			}
		}

		//space bar begins player movement only at beginning of turn
		if(Input.GetKeyDown(KeyCode.Space) && !isMoving && isMyTurn){
			System.Random rng = new System.Random();

			numSpacesToMove = rng.Next(1,max_tiles_per_turn+1);

			//force player to move n spaces for movement debugging
			numSpacesToMove = 1;

				Debug.Log("You rolled a "+numSpacesToMove);

			//tell server to tell each client to move player forwards
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerMove));
			
			//force player to move backwards for debugging
			//NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.ItemMoveBackwards));
		}

		if(Input.GetKeyDown(KeyCode.RightArrow) && atFork && isMyTurn){
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerForkChoice_Right));
		}else if(Input.GetKeyDown(KeyCode.LeftArrow) && atFork && isMyTurn){
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerForkChoice_Left));
		}

	}

	[ClientRpc]
	public void RpcForkChoice(int i){ //0 is left and 1 is right
			if(isMovingForward){
				List<GameObject> targets = currentTile.GetComponent<Tile>().nextTiles;
				targetTile_forward = targets[i];
				
				isChangingTarget = false; //continues movement
				atFork = false; //locks further left/right arrow key detection

				animationState = AnimationStates.HumanoidWalk;
			}else{
				List<GameObject> targets = currentTile.GetComponent<Tile>().previousTiles;
				
				targetTile_backward = targets[i];
				//only fork that has 3 choices is the backwards path of the ending tile, so there is no need to handle 3 choices

				isChangingTarget = false; //continues movement
				atFork = false; //locks further left/right arrow key detection

				animationState = AnimationStates.HumanoidWalk;
			}
			
	}

	[ClientRpc]
	public void RpcMove(){
		targetNextTile(true);

		Debug.Log("Moving to tile "+targetTile_forward);
		
		if(atFork){
			animationState = AnimationStates.HumanoidIdle;
		}else{
			animationState = AnimationStates.HumanoidWalk;
		}
		
		isMoving=true;
		doneMoving = false;
	}

	[ClientRpc]
	public void RpcMoveBackwards(){
		targetNextTile(false);

		Debug.Log("Moving to tile "+targetTile_backward);
		
		if(atFork){
			animationState = AnimationStates.HumanoidIdle;
		}else{
			animationState = AnimationStates.HumanoidWalk;
		}
		
		isMoving=true;
		doneMoving = false;
	}

	[ClientRpc]
	public void RpcStop(){

			Debug.Log("RpcStop");
		isMoving=false;
		animationState = AnimationStates.HumanoidIdle;

		//if the player used an item, restart their turn, else end their turn

		if(isLocalPlayer)TurnOver();
	}

	[ClientRpc]
	public void RpcResetPlayer(int p_num){ //cannot send objects though rpc commands
		player_num=p_num;
		transform.position = currentTile.transform.GetChild(player_num).position;
	}

	[ClientRpc]
	public void RpcChangeTarget(){
		isChangingTarget=targetNextTile(isMovingForward);
		if(atFork){
			animationState = AnimationStates.HumanoidIdle;
		}else{
			animationState = AnimationStates.HumanoidWalk;
		}
	}

	[TargetRpc]
	public void TargetRpcBeginTurn(NetworkConnection target){
		isMyTurn = true; //only local player's turn flag is set
			Debug.Log("TargetRpcBeginTurn Player"+player_num+"'s turn");
	}

	private void TurnOver(){
		isMyTurn = false;
		//message the server that the turn is over
		NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.TurnOver));
			Debug.Log("\tTurn ended");
	}

	//SyncVar hook to change animations
	void OnChangeAnimationState(AnimationStates state){
		GetComponent<AnimationController>().PlayAnimation(state);
	}

}