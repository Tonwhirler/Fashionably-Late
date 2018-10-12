using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

//The network manager is the server and has a subcomponent GameManager
public class MyNetworkManager : NetworkManager {

	[HideInInspector]
	public static GameManager gameManager = null;

	public int max_connections;//auto start the game when this is reached, to be replaced with lobby code

	private int num_players = 0;//number of players currently in the game

	//this is called when a player connects to the server as a host or client
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
       	//instantiate gameManager once, when host connects
		if(gameManager==null){
				gameManager = gameObject.GetComponent<GameManager>(); //put here to ensure GameManager has been instantiated
				NetworkServer.RegisterHandler(MsgType.Highest+1, OnStringMessage); //message listener binding
		}
        Debug.Log("Player "+num_players+" connected");

		//Instantiate player GameObject then bind to connection
		GameObject _player = Instantiate(playerPrefab,gameManager.start_spaces[num_players].transform.position,Quaternion.identity);
		_player.GetComponent<Player_Behavior>().player_num=num_players;

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

	public void OnStringMessage(NetworkMessage netMsg)
    {
		var msg = netMsg.ReadMessage<StringMessage>();

		Debug.Log("Server got message: '"+msg.value+"' from connection "+netMsg.conn);
		
		//replace string checking with enum
		if(msg.value.Equals("turn_over")){
			gameManager.turnOver=true;
		}  
	}
}
