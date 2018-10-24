using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;

public class Player_Behavior : NetworkBehaviour {
	
	private float speed = 2.5f;
	//private float boostSpeed = 5f;

	public GameObject currentTile;
	public GameObject targetTile;
	
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
	private bool isMyTurn = false; //is currently player's turn, allowing actions
	private bool doneMoving = false; //true when player has used all available moves

	private GameObject text_debug;//debug textbox on screen
	private GameObject text_turn;//turn textbox on screen
	void Start () {
		if(isLocalPlayer) GameObject.Find("Main Camera").GetComponent<CameraController>().SetPlayer(gameObject);
		text_debug = GameObject.Find("DebugText");
		text_turn = GameObject.Find("TurnText");
	}
	
	private void targetNextTile(){
		currentTile = targetTile;

		List<GameObject> targets = currentTile.GetComponent<Tile>().nextTiles;
		if(targets.Count != 1){
				Debug.Log("Fork in the road!");

			//need to wait for player input in selecting path

			targetTile = targets[0]; //for debugging, always choose left
		}else{
				Debug.Log("linear path");
			targetTile = targets[0];
		}
	}

	void Update () {
		//all clients can move this gameObject
		if(isMoving){
			//move to next tile
			Vector3 target = targetTile.transform.GetChild(player_num).position;
			gameObject.transform.LookAt(target);
			gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,target,speed*Time.deltaTime);
		}

		//only local player can detect input
		if(!isLocalPlayer)return;

		if(isMyTurn){
			text_turn.GetComponent<Text>().text = "Your turn!";
		}else{
			text_turn.GetComponent<Text>().text = "Not your turn.";
			text_debug.GetComponent<Text>().text = "";
		}

		if(isMoving){
			if(isMyTurn){
				//display detailed debugging info for localPlayer
				String t = numSpacesToMove+" spaces left to move\n"
					+"Distance to target = "+Vector3.Distance(gameObject.transform.position,targetTile.transform.GetChild(player_num).position);
				text_debug.GetComponent<Text>().text = t;
			}

			//when player has reached destination; will be replaced when the player can move multiple tiles per turn
			if(!isChangingTarget && gameObject.transform.position == targetTile.transform.GetChild(player_num).position){
				if(numSpacesToMove <= 1 && !doneMoving){
				//tell server to stop player's movement on all clients, also ends localplayer's turn
					NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerStop));
					numSpacesToMove=0;
					doneMoving=true;//prevents multiple messages being sent
				}else if(numSpacesToMove > 1){
					numSpacesToMove-=1;
						Debug.Log(numSpacesToMove+" spaces left to move");
					//message server to changetarget
					isChangingTarget=true;
					NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerTargetChange));
				}
			}
		}

		//only check for input once per turn
		if(Input.GetKeyDown(KeyCode.Space) && !isMoving && isMyTurn){
			System.Random rng = new System.Random();
			numSpacesToMove = rng.Next(1,max_tiles_per_turn+1);
				Debug.Log("You rolled a "+numSpacesToMove);
			//tell server to tell each client to move player
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerMove));
		}
	}

	[ClientRpc]
	public void RpcMove(){
		targetNextTile(); 
			Debug.Log("Moving to tile "+targetTile);
		animationState = AnimationStates.HumanoidWalk;
		isMoving=true;
		doneMoving = false;
	}

	[ClientRpc]
	public void RpcStop(){
		//doesn't implement the ability to move multiple spaces in a turn, but works for single space movement
			Debug.Log("RpcStop");
		isMoving=false;
		animationState = AnimationStates.HumanoidIdle;

		if(isLocalPlayer)TurnOver();
	}

	[ClientRpc]
	public void RpcResetPlayer(int p_num){ //cannot send objects though rpc commands
		player_num=p_num;
		transform.position = currentTile.transform.GetChild(player_num).position;
	}

	[ClientRpc]
	public void RpcChangeTarget(){
		targetNextTile();
		isChangingTarget=false;
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