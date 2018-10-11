using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player_Behavior : NetworkBehaviour {

	public float speed;
	public List<GameObject> tiles;
	public GameObject networkManager;

	[HideInInspector]
	public int player_num;
	public GameManager gameManager = null;

	private int currentTile = 0; //target Tile index, perhaps refactor name
	private bool isMoving; //player is currently movint to destination tile
	private bool doneMoving; //player has reached its destination
	private bool isMyTurn = false; //is currently player's turn, allowing actions

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
		//if(!isLocalPlayer)return;

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

		animationState = AnimationStates.HumanoidIdle;

		doneMoving = true;
		isMyTurn = false;

		if(!isServer){
			GameObject.Find("DebugText").GetComponent<Text>().text = "GameManager null, program frozen";
		}else{
			GameObject text = GameObject.Find("DebugText");
			text.GetComponent<Text>().text = "Not your turn :(";
			gameManager.turnOver=true;
		}

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

//=====================================================================================================

	[Obsolete("Replaced by TargetRpc functions")]
	public IEnumerator TakeTurn(){
		Debug.Log("Taking turn...");
		
		//wait for player to press space to start movement, will be replaced with GUI or control scheme
		yield return StartCoroutine(WaitForKeyDown(KeyCode.Space));
		
		//set next tile and movement flags to allow movement during Update()
		nextTile();
		Debug.Log("Moving to tile "+currentTile);
		isMoving=true;
		doneMoving=false;
		

		yield return StartCoroutine(WaitForDoneMoving());
		Debug.Log("Turn over, done moving");
		yield return new WaitForSeconds(1f); //small delay to make sure movement is finished on all clients
	}

	[Obsolete("Replaced by TargetRpc functions")]
	private IEnumerator WaitForKeyDown(KeyCode keyCode)
	{
		while(!Input.GetKeyDown(keyCode)){
			yield return null;
		}
	}

	[Obsolete("Replaced by TargetRpc functions")]
	private IEnumerator WaitForDoneMoving(){
		while(!doneMoving){
			yield return null;
		}
	}
}