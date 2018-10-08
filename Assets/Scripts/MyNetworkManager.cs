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
		 
		if(num_players==2){
			startGame();
		}
    }

	void startGame(){
		GameObject _player;

		Debug.Log("Starting Game");
		gameManager_instance = Instantiate(gameManager_prefab,Vector3.zero,Quaternion.identity);
		
		for(int i=0; i<num_players; i++){
			_player = Instantiate(playerPrefab,
				gameManager_instance.GetComponent<GameManager>().start_space.transform.position,Quaternion.identity);
			_player.GetComponent<Renderer>().material.color = gameManager_instance.GetComponent<GameManager>().player_colors[i];

			gameManager_instance.GetComponent<GameManager>().players.Add(_player);
		}

		gameManager_instance.GetComponent<GameManager>().StartGame();
	}
}
