using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	[HideInInspector]
	public List<GameObject> activePlayers; //references to each of the players in the game
	
	public int max_turns = 50; //prevents game from going too long
	
	private int turn_number; //increments after each player acts
	private int currentPlayer=0; //current player's index in players

	[HideInInspector]
	public bool turnOver; //true when the player has finished its turn
	
	private bool firstPlayerHasEnded = false; //true when first player has reached the end goal
	private bool secondPlayerHasEnded = false;	//true when second player has reached the end goal

	private GameObject readyScreen;
	private GameObject gameOverScreen;

	//To be called by the NetworkManager
	public void StartGame(){
		readyScreen = GameObject.Find("Ready Splashscreen");
		StartCoroutine(GameLoop());
	}
	
	//Sequence of Gameplay elements on macro level
	private IEnumerator GameLoop(){
		readyScreen.GetComponent<CanvasGroup>().alpha = 1f;
		yield return new WaitForSeconds(1f); //band-aid for game starting before scene is finished loading
		yield return StartCoroutine(ResetGame());
		readyScreen.GetComponent<CanvasGroup>().alpha = 0f;
		
		yield return StartCoroutine(TurnLoop());

		yield return StartCoroutine(GameOver());
	}

	//resets each player to start and clears the score
	private IEnumerator ResetGame(){
		Debug.Log("Resetting Game");
		for(int i=0; i<activePlayers.Count; i++){
			Debug.Log("\tPlayer"+i);
			//reset players' position to start space and bind player_num for each player object
			activePlayers[i].GetComponent<Player_Behavior>().RpcResetPlayer(i);
		}
		yield return new WaitForSeconds(0.1f); //delay before beginning turn loop
	}

	//each player takes a turn until there is a winner or there is a timeout
	private IEnumerator TurnLoop(){
		int playerToRemove = 0;
		bool hasSetFristPlayer = false;
		bool hasRemovedFirstPlayer = false;

		for(turn_number=1; turn_number<=max_turns; turn_number++){
			Debug.Log("Turn  "+turn_number+"start");
			for(int i=0; i<activePlayers.Count; i++){
				turnOver=false;
				currentPlayer=i;

				Debug.Log("Player "+activePlayers[i].GetComponent<Player_Behavior>().player_num+" turn");
				
				//Rpc client to take turn, assumes player_num is correctly tied to player's network connection index
				activePlayers[i].GetComponent<Player_Behavior>().TargetRpcBeginTurn(
					NetworkServer.connections[activePlayers[i].GetComponent<Player_Behavior>().player_num]);

				yield return StartCoroutine(WaitForTurnOver()); //wait here until player's turn is over

				if(secondPlayerHasEnded){ //go directly to game over
					break;
				}else if(firstPlayerHasEnded && !hasSetFristPlayer){ //remove current player after the turn round is over
					//NOTE: a flag will be needed in player script when using items out of turn is implemented so finished player cannot act out of turn
					playerToRemove = i;
					hasSetFristPlayer = true; //fixes bug where playerToRemove is overwritten by the last player
				}

				Debug.Log("Server recognized turn ended, waiting 1 second");
				yield return new WaitForSeconds(1f); //wait 1 second before starting next turn as a buffer for network
			}
			Debug.Log("Turn  "+turn_number+"end\n");

			if(secondPlayerHasEnded)break; //allows breaking out of nested loop from inner loop

			if(firstPlayerHasEnded && !hasRemovedFirstPlayer){
				Debug.Log("Removing Player"+playerToRemove+" from turn loop");
				activePlayers.RemoveAt(playerToRemove);
				hasRemovedFirstPlayer = true; //prevents this conditional from executing again
			}
		}
		yield return new WaitForSeconds(0.1f);
	}

	//calculates the results of the game, displays statistics
	private IEnumerator GameOver(){
		Debug.Log("GameOver");

		for(int i = 0; i<NetworkServer.connections.Count; i++){
			NetworkServer.connections[i].playerControllers[0].gameObject.GetComponent<Player_Behavior>().RpcShowGameOverScreen();
		}

		yield return new WaitForSeconds(0.1f);
	}

	private IEnumerator WaitForTurnOver(){
		while(!turnOver){ //this acts as a blocking routine done in parallel to rest of program
			yield return null;
		}
	}

	public bool AddPlayer(GameObject player){
		Debug.Log("GameManager.Adding player"+activePlayers.Count);
		activePlayers.Add(player);
		if(activePlayers.Count == NetworkServer.connections.Count)return true;
		return false;
	}

	public void MoveCurrentPlayer(){
		Debug.Log("Moving player"+currentPlayer);
		activePlayers[currentPlayer].GetComponent<Player_Behavior>().RpcMove(true);
	}

	public void StopCurrentPlayer(){
		Debug.Log("Stopping player"+currentPlayer);
		activePlayers[currentPlayer].GetComponent<Player_Behavior>().RpcStop();
	}

	public void ChangePlayerTarget(){
		Debug.Log("Changing target tile of player"+currentPlayer);
		activePlayers[currentPlayer].GetComponent<Player_Behavior>().RpcChangeTarget();
	}

	public void PlayerForkChoice(int i){
		if(i==0){
			Debug.Log("Player"+currentPlayer+" chose the left path");
		}else{
			Debug.Log("Player"+currentPlayer+" chose the right path");
		}
		activePlayers[currentPlayer].GetComponent<Player_Behavior>().RpcForkChoice(i);
	}

	public void ApplyItemBackwards(int spaces){
		Debug.Log("Moving Player Backwards "+spaces+" spaces");
		activePlayers[currentPlayer].GetComponent<Player_Behavior>().RpcMove(false);
	}

	public void EndCurrentPlayer(){ //when the current player has reached the goal, remove from turn cycle list
		if(firstPlayerHasEnded){
			Debug.Log("secondPlayerHasEnded");
			secondPlayerHasEnded = true;
		}else{
			Debug.Log("firstPlayerHasEnded");
			firstPlayerHasEnded = true;
		}
		activePlayers[currentPlayer].GetComponent<Player_Behavior>().RpcStop_ReachedFinish();
	}
}
