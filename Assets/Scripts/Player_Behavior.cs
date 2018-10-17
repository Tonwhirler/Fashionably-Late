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

	private int currentTileIndex = 0; //index of tile player will move to
	private int numSpacesToMove = 0; //how many tiles the player can move, as determined by dice roll

	private bool isMoving = false; //player is currently moving to destination tile
	private bool isMyTurn = false; //is currently player's turn, allowing actions

	[HideInInspector]
	[SyncVar(hook = "OnChangeAnimationState")]
	public AnimationStates animationState=AnimationStates.HumanoidIdle;

	void Start () {
		if(isLocalPlayer) GameObject.Find("Main Camera").GetComponent<CameraController>().SetPlayer(gameObject);
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

		if(isMoving){
			//when player has reached destination; will be replaced when the player can move multiple tiles per turn
			if(gameObject.transform.position == tiles[currentTileIndex].transform.GetChild(player_num).position){
				//tell server to stop player's movement on all clients, also ends localplayer's turn
				NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerStop));
			}
		}

		//only check for input once per turn
		if(Input.GetKeyDown(KeyCode.Space) && !isMoving && isMyTurn){
			Debug.Log("Moving to tile "+currentTileIndex);
			//tell server to tell each client to move player
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerMove));
		}
	}

	[ClientRpc]
	public void RpcMove(){
		if(numSpacesToMove <= 0)numSpacesToMove=2; //will be replaced with dice roll
		
		/*Debug.Log(numSpacesToMove+" spaces left to move");
		if(isLocalPlayer){
			GameObject text = GameObject.Find("DebugText");
			String t = numSpacesToMove+" spaces left to move";
			text.GetComponent<Text>().text = t;
		}*/

		nextTile(); 
		animationState = AnimationStates.HumanoidWalk;
		isMoving=true;
	}

	[ClientRpc]
	public void RpcStop(){
		//doesn't implement the ability to move multiple spaces in a turn, but works for single space movement
		Debug.Log("RpcStop");
		isMoving=false;
		animationState = AnimationStates.HumanoidIdle;

		if(isLocalPlayer)TurnOver(); //only local player will end turn


		/* bug with client losing track of numSpacesToMove
		numSpacesToMove--;
		Debug.Log("...moving again, "+numSpacesToMove+" left to move");
		if(isLocalPlayer){
			GameObject text = GameObject.Find("DebugText");
			String t = numSpacesToMove+" spaces left to move";
			text.GetComponent<Text>().text = t;
		}

		if(numSpacesToMove <= 0){
			isMoving=false;
			animationState = AnimationStates.HumanoidIdle;
			if(isLocalPlayer)TurnOver();
		}else{
			nextTile();
		}*/
	}

	[ClientRpc]
	public void RpcSetPlayerNum(int p_num){
		player_num=p_num;
	}

	[TargetRpc]
	public void TargetRpcBeginTurn(NetworkConnection target,int p_num){
		isMyTurn = true; //only local player's turn flag is set
		Debug.Log("TargetRpcBeginTurn Player"+player_num+"'s turn");

		GameObject text = GameObject.Find("DebugText");
		text.GetComponent<Text>().text = "Your turn :)";
		Debug.Log("\tTurn began");
	}

	private void TurnOver(){
		isMyTurn = false;

		//message the server that the turn is over
		NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.TurnOver));

		GameObject text = GameObject.Find("DebugText");
		text.GetComponent<Text>().text = "Not your turn :(";
		Debug.Log("\tTurn ended");
	}

	//SyncVar hook to change animations
	void OnChangeAnimationState(AnimationStates state){
		GetComponent<AnimationController>().PlayAnimation(state);
	}

}