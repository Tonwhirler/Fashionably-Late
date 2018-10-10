using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//The network manager will take control until the players are locked in, then it will hand over control to the GameManager
public class MyNetworkManager : NetworkManager {

	public GameObject gameManager_prefab;
	private GameObject gameManager_instance;

	private int num_players;

	public override void OnServerConnect(NetworkConnection conn)
    {
		num_players++;
        Debug.Log("Player "+num_players+" connected");
		 
		if(num_players==1){
			startGame();
		}
    }

	void startGame(){
		GameObject _player;

		Debug.Log("Starting Game");
		gameManager_instance = Instantiate(gameManager_prefab);
		
		for(int i=0; i<num_players; i++){
//TODO: send command to each client to run the same piece of code
			_player = Instantiate(playerPrefab,
				gameManager_instance.GetComponent<GameManager>().start_spaces[i].transform.position,Quaternion.identity);
			//_player.GetComponent<Renderer>().material.color = gameManager_instance.GetComponent<GameManager>().player_colors[i]; //only works with debug_player prefab

			gameManager_instance.GetComponent<GameManager>().players.Add(_player);

			Debug.Log("Player "+i+" spawned.");
		}

		gameManager_instance.GetComponent<GameManager>().StartGame();
	}
}
