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
	private bool doneMoving; //player has reached its destination
	[SyncVar]
	private bool isMyTurn = false; //is currently player's turn, allowing actions

	[HideInInspector]
	[SyncVar(hook = "OnChangeAnimationState")]
	public AnimationStates animationState=AnimationStates.HumanoidIdle;

	void Start () {
		isMoving = false;
		doneMoving = false;
		if(isLocalPlayer) GameObject.Find("Main Camera").GetComponent<CameraController>().SetPlayer(gameObject);
	}
	
	private int nextTile(){
		currentTile++;
		if(currentTile==tiles.Count)currentTile=0;
		return currentTile;
	}

	void Update () {
		if(!isLocalPlayer)return; //doesn't seem to change anything

		if(Input.GetKeyDown(KeyCode.Space) && !isMoving && isMyTurn){
			Debug.Log("Moving to tile "+currentTile);
			isMoving=true;
			doneMoving=false;
			nextTile();
		}

		Vector3 target;
		if(isMoving){
			target = tiles[currentTile].transform.GetChild(player_num).position;

			if(!isServer){ //prevents host from moving everything at double speed
				gameObject.transform.LookAt(target);
				gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
					target,speed*Time.deltaTime);
			}
			CmdFacePlayer(target);
			CmdMovePlayer(target);

			//set animation state to motion
			animationState = AnimationStates.HumanoidWalk;
			
			//display distance to target on gui text
			GameObject text = GameObject.Find("DebugText");
			String t = String.Format("Distance to target = {0}",Vector3.Distance(gameObject.transform.position,target));
			text.GetComponent<Text>().text = t;

			//when player has reached destination
			if(gameObject.transform.position == target){
				TurnOver();
			}
		}
	}



	[TargetRpc]
	public void TargetRpcBeginTurn(NetworkConnection target,int p_num){
		isMyTurn = true;

		if(player_num!=p_num)player_num=p_num;

		GameObject text = GameObject.Find("DebugText");
		text.GetComponent<Text>().text = "Your turn :)";
		Debug.Log("\tTurn began");
	}

	private void TurnOver(){
		isMoving = false;
		doneMoving = true;
		isMyTurn = false;

		animationState = AnimationStates.HumanoidIdle;

		//message the server that the turn is over
			//should refactor this into a custom MsgType
		NetworkManager.singleton.client.Send(MsgType.Highest+1,new StringMessage("turn_over"));

		GameObject text = GameObject.Find("DebugText");
		text.GetComponent<Text>().text = "Not your turn :(";
		Debug.Log("\tTurn ended");
	}

	[Command]
	void CmdMovePlayer(Vector3 target){
		gameObject.transform.LookAt(target);
		gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
			target,speed*Time.deltaTime);
	}

	[Command]
	void CmdFacePlayer(Vector3 target){
		gameObject.transform.LookAt(target);
	}

	//SyncVar hook
	void OnChangeAnimationState(AnimationStates state){
		GetComponent<AnimationController>().PlayAnimation(state);
	}

}