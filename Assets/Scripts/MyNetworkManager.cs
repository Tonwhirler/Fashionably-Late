using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

//represents the different messages a client can send to the server
public enum MyMessageType
{
    TurnOver = 1,
	PlayerMove = 2,
	PlayerStop = 3,
	PlayerTargetChange = 4,
	ItemUsed = 5
}

//The network manager is the server and has a subcomponent GameManager
public class MyNetworkManager : NetworkManager {

	[HideInInspector]
	public static GameManager gameManager = null;

	public int max_connections;//auto start the game when this is reached, to be replaced with lobby code

	private int num_players = 0;//number of players currently in the game

	private bool hasStartedGame=false; //locks clients from joining game after it has started

	//this is called when a player connects to the server as a host or client
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
		if(hasStartedGame)return;

       	//instantiate gameManager once, when host connects
		if(gameManager==null){
				gameManager = gameObject.GetComponent<GameManager>(); //put here to ensure GameManager has been instantiated
				NetworkServer.RegisterHandler(MsgType.Highest+1, OnEnumMessage);
		}
        Debug.Log("Player "+num_players+" connected");

		//Instantiate player GameObject then bind to connection
		GameObject _player = Instantiate(playerPrefab,gameManager.start_spaces[num_players].transform.position,Quaternion.identity);
		
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
		if(hasStartedGame)return;

		Debug.Log("OnClientConnect for player "+num_players);
		ClientScene.AddPlayer(client.connection, 0);
	}
	
	void startGame()
	{
		Debug.Log("Starting Game with "+NetworkServer.connections.Count+" connections");
		hasStartedGame=true; //locks out more clients from joining
		gameManager.StartGame();
	}

	public void OnEnumMessage(NetworkMessage netMsg)
	{
		IntegerMessage msg = netMsg.ReadMessage<IntegerMessage>();
		
		switch(msg.value){
			case (int) MyMessageType.TurnOver:
					Debug.Log("Server got message: TurnOver from connection "+netMsg.conn);
					gameManager.turnOver=true;
				break;
			
			case (int) MyMessageType.PlayerMove:
					Debug.Log("Server got message: PlayerMove from connection "+netMsg.conn);
					gameManager.MoveCurrentPlayer();
				break;

			
			case (int) MyMessageType.PlayerStop:
					Debug.Log("Server got message: PlayerStop from connection "+netMsg.conn);
					gameManager.StopCurrentPlayer();
				break;

				case (int) MyMessageType.PlayerTargetChange:
					Debug.Log("Server got message: PlayerTargetChange from connection "+netMsg.conn);
					gameManager.ChangePlayerTarget();
				break;

			default:
				Debug.Log("Server got unidentified message from connection "+netMsg.conn);
				break;
		}
	}
}
