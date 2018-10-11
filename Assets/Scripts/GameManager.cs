using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour {

	public List<GameObject> players; //references to each of the players
	public List<Color> player_colors; //set each player's color, will be replaced with unique character models
	public List<Transform> start_spaces; //beginning tile on board
	
	public int max_turns = 20; //prevents game from going too long for debugging, will be removed for full game
	
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
		for(int i=0; i<players.Count; i++){
			players[i].GetComponent<CharacterController>().transform.position = start_spaces[i].position;
			//player.setScore(0);
		}
		yield return new WaitForSeconds(0.1f); //3 second delay before beginning turn loop
	}

	//each player takes a turn until there is a winner or there is a timeout
	private IEnumerator TurnLoop(){
		for(turn_number=1; turn_number<=max_turns; turn_number++){
			Debug.Log("Turn  "+turn_number+"start");
			for(int i=0; i<players.Count; i++){
				Debug.Log("Player "+players[i].GetComponent<Player_Behavior>().player_num+" turn");
				
				//Rpc client to take turn, then wait until turn is over
				players[i].GetComponent<Player_Behavior>().TargetRpcBeginTurn(NetworkServer.connections[i]);
				yield return new WaitForSeconds(5f);
				players[i].GetComponent<Player_Behavior>().TargetRpcEndTurn(NetworkServer.connections[i]);
				yield return new WaitForSeconds(5f);
			}
			//maybe have a minigame here
			Debug.Log("Turn  "+turn_number+"end\n");
		}
		yield return new WaitForSeconds(0.1f);
	}

	//calculates the results of the game, displays statistics
	private IEnumerator GameOver(){
		Debug.Log("GameOver");
		yield return new WaitForSeconds(0.1f);
	}
}
