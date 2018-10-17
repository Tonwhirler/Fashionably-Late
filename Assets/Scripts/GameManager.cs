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
	private int currentPlayer=0; //current player's index in players

	[HideInInspector]
	public bool turnOver; //true when the player has finished its turn
	
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
			//reset players' position to start space and bind player_num for each player object
			players[i].transform.position = start_spaces[i].position;
			players[i].GetComponent<Player_Behavior>().RpcSetPlayerNum(i);
		}
		yield return new WaitForSeconds(0.1f); //3 second delay before beginning turn loop
	}

	//each player takes a turn until there is a winner or there is a timeout
	private IEnumerator TurnLoop(){
		for(turn_number=1; turn_number<=max_turns; turn_number++){
			Debug.Log("Turn  "+turn_number+"start");
			for(int i=0; i<players.Count; i++){
				turnOver=false;
				currentPlayer=i;

				Debug.Log("Player "+players[i].GetComponent<Player_Behavior>().player_num+" turn");
				
				//Rpc client to take turn, then wait until turn is over
				players[i].GetComponent<Player_Behavior>().TargetRpcBeginTurn(NetworkServer.connections[i]);
				yield return StartCoroutine(WaitForTurnOver());
				Debug.Log("Server recognized turn ended, waiting 1 second");

				yield return new WaitForSeconds(1f);
			}
			//maybe have a minigame here
			//maybe display score after all players have taken their turn
			Debug.Log("Turn  "+turn_number+"end\n");
		}
		yield return new WaitForSeconds(0.1f);
	}

	//calculates the results of the game, displays statistics
	private IEnumerator GameOver(){
		Debug.Log("GameOver");
		yield return new WaitForSeconds(0.1f);
	}

	private IEnumerator WaitForTurnOver(){
		while(!turnOver){
			yield return null;
		}
	}

	public void MoveCurrentPlayer(){
		Debug.Log("Moving player"+currentPlayer);
		players[currentPlayer].GetComponent<Player_Behavior>().RpcMove();
	}

	public void StopCurrentPlayer(){
		Debug.Log("Stopping player"+currentPlayer);
		players[currentPlayer].GetComponent<Player_Behavior>().RpcStop();
	}

	public void ChangePlayerTarget(){
		Debug.Log("Changing target tile of player"+currentPlayer);
		players[currentPlayer].GetComponent<Player_Behavior>().RpcChangeTarget();
	}
}
