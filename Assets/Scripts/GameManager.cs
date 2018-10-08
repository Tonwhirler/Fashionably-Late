using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public List<GameObject> players; //references to each of the players
	public List<Color> player_colors; //set each player's color, will be replaced with unique character models
	public Transform start_space; //beginning tile on board

	public int max_turns = 20; //prevents game from going too long

	private int turn_number; //increments after each player acts

	//To be called by the NetworkManager
	public void StartGame(){
		StartCoroutine(GameLoop());
	}
	
	//Sequence of Gameplay elements on macro level
	private IEnumerator GameLoop(){
		yield return StartCoroutine(ResetGame());
		yield return StartCoroutine(TurnLoop());
		yield return StartCoroutine(GameOver());
	}

	//resets each player to start and clears the score
	private IEnumerator ResetGame(){
		Debug.Log("Resetting Game");
		foreach(GameObject player in players){
			player.GetComponent<Rigidbody>().transform.position = start_space.position;
			//player.setScore(0);
		}
		yield return new WaitForSeconds(3f); //3 second delay before beginning turn loop
	}

	//each player takes a turn until there is a winner or there is a timeout
	private IEnumerator TurnLoop(){
		for(turn_number=1; turn_number<=max_turns; turn_number++){
			Debug.Log("Turn  "+turn_number+"start");
			foreach(GameObject player in players){
				Debug.Log("Player turn");
				//player.TakeTurn();
			}
			//maybe have a minigame here
			Debug.Log("Turn  "+turn_number+"end\n");
		}
		yield return null;
	}

	//calculates the results of the game, displays statistics
	private IEnumerator GameOver(){
		Debug.Log("GameOver");
		yield return new WaitForSeconds(3f);
	}
}
