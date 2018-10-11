using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Behavior : NetworkBehaviour {

	public float speed;
	public List<GameObject> tiles;
	public GameObject networkManager;

	[HideInInspector]
	public int player_num;
	
	private int currentTile = 0;
	private bool isMoving;
	private bool doneMoving;


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
		
		Vector3 target;
		if(isMoving){
			Debug.Log("moving...");
			target = tiles[currentTile].transform.GetChild(player_num).position;

			if(!isServer){ //prevents host from moving everything at double speed
				gameObject.transform.LookAt(target);
				gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
					target,speed*Time.deltaTime);
			}
			CmdMovePlayer(target);
			
			//when player has reached destination
			if(gameObject.transform.position == target){
				isMoving = false;
				doneMoving = true;
			}
		}
	}

	public IEnumerator TakeTurn(){
		Debug.Log("Taking turn...");
		
		//wait for player to press space to start movement, will be replaced with GUI or control scheme
		yield return StartCoroutine(WaitForKeyDown(KeyCode.Space));
		
		nextTile();
			Debug.Log("Moving to tile "+currentTile);
		isMoving=true;
		doneMoving=false;
		

		yield return StartCoroutine(WaitForDoneMoving());

		Debug.Log("Turn over, done moving");

		yield return new WaitForSeconds(1f); //small delay to make sure movement is finished on all clients
	}

	private IEnumerator WaitForKeyDown(KeyCode keyCode)
	{
		while(!Input.GetKeyDown(keyCode)){
			yield return null;
		}
	}

	private IEnumerator WaitForDoneMoving(){
		while(!doneMoving){
			yield return null;
		}
	}

	[Command]
	void CmdMovePlayer(Vector3 target){
		gameObject.transform.LookAt(target);
		gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
			target,speed*Time.deltaTime);
	}
}