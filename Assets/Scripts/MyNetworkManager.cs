using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//The network manager will take control until the players are locked in, then it will hand over control to the GameManager
public class MyNetworkManager : NetworkManager {

	public GameObject gameManager_prefab;
	public GameObject gameManager_instance;

	public int max_connections;//auto start the game when this is reached

	private int num_players;

	public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("Player "+num_players+" connected");
		num_players++;
		 
		if(num_players==max_connections){
			startGame();
		}
    }

	

	void startGame(){
		GameObject _player;

		gameManager_instance = Instantiate(gameManager_prefab);
		Debug.Log("Starting Game with "+NetworkServer.connections.Count+" connections");
		
		

		 for(int i=0; i<num_players; i++){
			 //instantiate
			 _player = Instantiate(playerPrefab,
				gameManager_instance.GetComponent<GameManager>().start_spaces[i].transform.position,Quaternion.identity);
			_player.GetComponent<Player_Behavior>().player_num=i;

			//bind to localplayer
			//NetworkServer.SetClientReady(NetworkServer.connections[i]);	
			//NetworkServer.SpawnWithClientAuthority(_player,NetworkServer.connections[i]);
			NetworkServer.AddPlayerForConnection(NetworkServer.connections[i],_player,0);

			//add reference to GameManaer
			gameManager_instance.GetComponent<GameManager>().players.Add(_player);

			Debug.Log("Player "+i+" spawned.");
		}

		gameManager_instance.GetComponent<GameManager>().StartGame();
	}
}
