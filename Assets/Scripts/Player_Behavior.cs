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

	private int currentTile = 0; //target Tile index, perhaps refactor name
	[SyncVar]
	private bool isMoving; //player is currently movint to destination tile
	[SyncVar]
	private bool isMyTurn = false; //is currently player's turn, allowing actions

	[HideInInspector]
	[SyncVar(hook = "OnChangeAnimationState")]
	public AnimationStates animationState=AnimationStates.HumanoidIdle;

	void Start () {
		isMoving = false;
		if(isLocalPlayer) GameObject.Find("Main Camera").GetComponent<CameraController>().SetPlayer(gameObject);
	}
	
	private int nextTile(){
		currentTile++;
		if(currentTile==tiles.Count)currentTile=0;
		return currentTile;
	}

	void Update () {
		//all clients can move this gameObject

		if(isMoving){
			//move to next tile in list
			Vector3 target = tiles[currentTile].transform.GetChild(player_num).position;
			gameObject.transform.LookAt(target);
			gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,target,speed*Time.deltaTime);
		}

		if(!isLocalPlayer)return; //only local player can detect input

		if(isMoving){
			//when player has reached destination; will be replaced when the player can move multiple tiles per turn
			if(gameObject.transform.position == tiles[currentTile].transform.GetChild(player_num).position){
				//tell server to stop player's movement on all clients, also ends localplayer's turn
				NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerStop));
			}
		}

		//only check for input once per turn
		if(Input.GetKeyDown(KeyCode.Space) && !isMoving && isMyTurn){
			Debug.Log("Moving to tile "+currentTile);
			//tell server to move player
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerMove));
		}
	}

	[ClientRpc]
	public void RpcMove(){
		//wait for server to respond before starting to move; should move all client's versions of this object
		Debug.Log("RpcMove");
		
		nextTile();
		isMoving=true; 
		animationState = AnimationStates.HumanoidWalk;
	}

	[ClientRpc]
	public void RpcStop(){
		Debug.Log("RpcStop");
		isMoving=false;
		animationState = AnimationStates.HumanoidIdle;

		if(isLocalPlayer)TurnOver(); //only local player will end turn
	}

	[ClientRpc]
	public void RpcSetPlayerNum(int p_num){
		player_num=p_num;
	}

	[TargetRpc]
	public void TargetRpcBeginTurn(NetworkConnection target,int p_num){
		isMyTurn = true; //only local player's turn is set
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

	//SyncVar hook
	void OnChangeAnimationState(AnimationStates state){
		GetComponent<AnimationController>().PlayAnimation(state);
	}

}