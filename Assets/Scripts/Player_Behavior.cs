using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;

public class Player_Behavior : NetworkBehaviour {

	public float speed;
	public List<GameObject> tiles;

	[HideInInspector]
	public int player_num;

	[HideInInspector]
	[SyncVar(hook = "OnChangeAnimationState")]
	public AnimationStates animationState=AnimationStates.HumanoidIdle;

	[HideInInspector]
	public int numSpacesToMove = 0; //how many tiles the player can move, as determined by dice roll
	public int tiles_per_turn;

	[SyncVar]
	private int currentTileIndex = 0; //index of tile player will move to

	private bool isChangingTarget = false; //disable checking if the player has reached the target while the target is being changed

	private bool isMoving = false; //player is currently moving to destination tile
	private bool isMyTurn = false; //is currently player's turn, allowing actions
	private bool doneMoving = false; //true when player has used all available moves

	private GameObject text;//debug textbox on screen

	void Start () {
		if(isLocalPlayer) GameObject.Find("Main Camera").GetComponent<CameraController>().SetPlayer(gameObject);
		text = GameObject.Find("DebugText");
	}
	
	private int nextTile(){
		int oldTile = currentTileIndex;
		//will be refactored to allow branching paths to be chosen
			//tiles will also be stored in a linked list instead of a linear list
		currentTileIndex++;
		if(currentTileIndex==tiles.Count)currentTileIndex=0;
		
		Debug.Log("moving from tile "+oldTile+" to "+currentTileIndex);
		return currentTileIndex;
	}

	void Update () {
		//all clients can move this gameObject
		if(isMoving){
			//move to next tile
			Vector3 target = tiles[currentTileIndex].transform.GetChild(player_num).position;
			gameObject.transform.LookAt(target);
			gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,target,speed*Time.deltaTime);
		}

		//only local player can detect input
		if(!isLocalPlayer)return;

		//display detailed debugging info for localPlayer
		String t = "isMyTurn = "+isMyTurn+"\n"
			+numSpacesToMove+" spaces left to move\n"
			+"Distance to target = "+Vector3.Distance(gameObject.transform.position,tiles[currentTileIndex].transform.GetChild(player_num).position);
		text.GetComponent<Text>().text = t;

		if(isMoving){
			//when player has reached destination; will be replaced when the player can move multiple tiles per turn
			if(!isChangingTarget && gameObject.transform.position == tiles[currentTileIndex].transform.GetChild(player_num).position){
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
			Debug.Log("Moving to tile "+currentTileIndex);

			numSpacesToMove=tiles_per_turn;

			//tell server to tell each client to move player
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerMove));
		}
	}

	[ClientRpc]
	public void RpcMove(){
		nextTile(); 
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
	public void RpcSetPlayerNum(int p_num){
		player_num=p_num;
	}

	[ClientRpc]
	public void RpcChangeTarget(){
		nextTile();
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