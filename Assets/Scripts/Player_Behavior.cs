using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Behavior : NetworkBehaviour {

	public float speed;
	public List<GameObject> tiles;

	private int currentTile = 0;
	private bool isMoving;
	private bool doneMoving;
	void Start () {
		isMoving = false;
		doneMoving = false;
	}
	
	private int nextTile(){
		currentTile++;
		if(currentTile==tiles.Count)currentTile=0;
		return currentTile;
	}

	void Update () {
		if(!isLocalPlayer)return;

		if(isMoving){
			if(!isServer){
				gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
					tiles[currentTile].transform.position,speed*Time.deltaTime);
			}
			CmdMovePlayer(tiles[currentTile].transform.position);
			
		
			if(gameObject.transform.position == tiles[currentTile].transform.position){
				isMoving = false;
				doneMoving = true;
				nextTile(); //loops movement automatically for debugging
			}
		}
	}

	public IEnumerator TakeTurn(){
		//dont't end turn until player has moved to a new tile
		while(true){
			if(Input.GetKeyDown(KeyCode.Space)){
				isMoving = true;
				doneMoving = false;
				nextTile();
			}

			if(doneMoving)break; //player has reached the destination tile, end turn for now
		}
		return null;
	}

	[Command]
	void CmdMovePlayer(Vector3 target){
		gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
			target,speed*Time.deltaTime);
	}
}