using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//The network manager will take control until the players are locked in, then it will hand over control to the GameManager
public class MyNetworkManager : NetworkManager {

	[HideInInspector]
	public static GameManager gameManager = null;

	public int max_connections;//auto start the game when this is reached

	private int num_players = 0;

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
       	//instantiate gameManager
		if(gameManager==null){
				gameManager = gameObject.GetComponent<GameManager>();
		}
        Debug.Log("Player "+num_players+" connected");
	    
		Debug.Log("OnServerAddPlayer for player"+num_players);

		//Instantiate player GameObject then bind to connection
		GameObject _player = Instantiate(playerPrefab,gameManager.start_spaces[num_players].transform.position,Quaternion.identity);
		_player.GetComponent<Player_Behavior>().player_num=num_players;
		_player.GetComponent<Player_Behavior>().gameManager=gameManager;

		NetworkServer.SetClientReady(conn);	
		NetworkServer.AddPlayerForConnection(conn,_player,0);
		gameManager.players.Add(_player);

		num_players++;
		//start game when enough players have joined, this should be replaced with NetworkLobby code
		if(num_players==max_connections){
			startGame();
		}
    }

	public override void OnClientConnect(NetworkConnection conn)
    {
		Debug.Log("OnClientConnect for player "+num_players);
		ClientScene.AddPlayer(client.connection, 0);
	}
	
	void startGame(){
		Debug.Log("Starting Game with "+NetworkServer.connections.Count+" connections");
		gameManager.StartGame();
	}
}
