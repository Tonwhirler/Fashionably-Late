using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour {

	public float speed;
	public List<GameObject> tiles;

	private int currentTile = 0;
	private bool isMoving;

	void Start () {
		isMoving = false;

		if(isLocalPlayer) GetComponent<Renderer>().material.color = Color.red;
	}
	
	private int nextTile(){
		currentTile++;
		if(currentTile==tiles.Count)currentTile=0;
		return currentTile;
	}

	void Update () {
		if(!isLocalPlayer)return;

		if(Input.GetKeyDown(KeyCode.Space) && !isMoving){
			isMoving = true;
			nextTile();
		}

		if(isMoving){
			gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
			tiles[currentTile].transform.position,speed*Time.deltaTime);
		
			if(gameObject.transform.position == tiles[currentTile].transform.position){
				isMoving = false;
			}
		}
	}
}
