using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;

public class Player_Behavior : NetworkBehaviour {
	public bool debug_enable_DebugMode; //only use debug_force variables when true
	public bool debug_force_BackwardMovement; //set in prefab editor
	public int debug_force_DiceRoll; //fix dice roll
	public float debug_force_Speed; //fix player speed

	private float speed = 3f; //normal speed
	//private float boostSpeed = 5f; //make item-based movement faster with running animation

	[HideInInspector]
	public GameObject currentTile;
	[HideInInspector]
	public GameObject targetTile;
	
	[HideInInspector]
	public int player_num;

	[HideInInspector]
	[SyncVar(hook = "OnChangeAnimationState")]
	public AnimationStates animationState=AnimationStates.HumanoidIdle;

	[HideInInspector]
	public int numSpacesToMove = 0; //how many tiles the player can move, as determined by dice roll
	public int max_tiles_per_turn; //upper bound on dice roll
    [HideInInspector]
    public bool isMyTurn = false; //is currently player's turn, allowing actions
    [HideInInspector]
    public bool isMoving = false; //player is currently moving to destination tile
    [HideInInspector]
	[SyncVar(hook = "OnChangeIsFrozen")]
    public bool isFrozen = false; //player is currently frozen and cannot move for one turn
    private bool isMovingForward = true;
	private bool doneMoving = false; //true when player has used all available moves
	private bool isChangingTarget = false; //disable checking if the player has reached the target while the target is being changed
	private bool atFork = false;

	private bool movedAfterItem = false;

	//UI Elements, when HUD is done, make these public and set in inspector
	private GameObject text_debug;//debug textbox on screen
	private GameObject text_turn;//turn textbox on screen
	private GameObject text_finished;//textbox for when a player reached the last tile

	void Start () {
		if(isLocalPlayer){
			GameObject.Find("Main Camera").GetComponent<CameraController>().SetPlayer(gameObject);
       		GameObject.Find("Ability Controller").GetComponent<Ability_Controller>().SetPlayer(gameObject);
        	GameObject.Find("Ability Controller").GetComponent<Ability_Controller>().GiveStartingAbilities();
       		GameObject.Find("Button Controller").GetComponent<Button_Controller>().SetPlayer(gameObject);
		}
        text_debug = GameObject.Find("DebugText");
		text_turn = GameObject.Find("TurnText");
		text_finished = GameObject.Find("FinishText");
        targetTile = GameObject.Find("Tile_0_Start");
		currentTile = GameObject.Find("Tile_0_Start");

        if (debug_enable_DebugMode)GameObject.Find("DebugModeText").GetComponent<Text>().text = "<DEBUG MODE ENABLED>";
	}
	
	//returns false if target has been chosen, true if the target is still being chosen (fork)
	private bool targetNextTile(bool forwards){
		if(forwards){
			currentTile = targetTile;
			List<GameObject> targets = currentTile.GetComponent<Tile>().nextTiles;
			if(targets.Count > 1){ //currently only supports forks with 2 choices
					Debug.Log("Fork in the road! Waiting for player input...");
				atFork = true;
				if(isLocalPlayer){
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().left.GetComponent<CanvasGroup>().alpha = 1f;
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().left.GetComponent<Button>().interactable = true;
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().right.GetComponent<CanvasGroup>().alpha = 1f;
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().right.GetComponent<Button>().interactable = true;
				}
				return true;
			}else{
					Debug.Log("linear path");
				targetTile = targets[0];
				return false;
			}
		}else{
			currentTile = targetTile;

			List<GameObject> targets = currentTile.GetComponent<Tile>().previousTiles;
			if(targets.Count > 1){ //currently only supports forks with 2 choices
					Debug.Log("Fork in the road! Waiting for player input...");
				atFork = true;
				if(isLocalPlayer){
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().left.GetComponent<CanvasGroup>().alpha = 1f;
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().left.GetComponent<Button>().interactable = true;
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().right.GetComponent<CanvasGroup>().alpha = 1f;
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().right.GetComponent<Button>().interactable = true;
				}
				return true;
			}else{
					Debug.Log("linear path");
				targetTile = targets[0];

				if(currentTile.GetComponent<Tile>().isStart){
					Debug.Log("Player cannot move backwards from the start tile");
					targetTile = currentTile;
				}

				return false;
			}
			//TODO: need to make sure player stops at starting tile
		}
	}

	public float rotationSpeed; //set in inspector

	void Update () {
		//all clients can move this gameObject
		if(isMoving){
			//move to next tile
			Vector3 target;
			target = targetTile.transform.GetChild(player_num).position; //player moves to a set position within tile corresponding to player number

			//instantly rotate to face target
			//gameObject.transform.LookAt(target);
			
			//smoothly rotate to face target
			if(isMovingForward){
				gameObject.transform.rotation = Quaternion.Lerp(transform.rotation,
            		targetTile.transform.rotation,
            		Time.deltaTime*rotationSpeed);
			}

			//move towards target
			gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,target,speed*Time.deltaTime);
			
		}

		//only local player can detect input
		if(!isLocalPlayer)return;

		if(isMyTurn){
			text_turn.GetComponent<Text>().text = "Your turn!";
			if(!isMoving){
				if(isFrozen){
					text_debug.GetComponent<Text>().text ="FROZEN, skip your turn!";
				}else{
					if(!movedAfterItem){
						text_debug.GetComponent<Text>().text ="Use your ability\nOR roll dice!";
					}else{
						text_debug.GetComponent<Text>().text ="Roll dice!";
					}
				}
			} 
			
		}else{
			text_turn.GetComponent<Text>().text = "Not your turn.";
			text_debug.GetComponent<Text>().text = "";
		}

		if(isMoving){ //only local player detects when the movement is to end
			if(isMyTurn){
				//display detailed debugging info for localPlayer
				String t = numSpacesToMove+" spaces left to move\n";

				if(atFork){
					t+="Press the LEFT or RIGHT arrow key to continue!";
				}else{
					t+="Distance to target = "+Vector3.Distance(gameObject.transform.position,targetTile.transform.GetChild(player_num).position);
				} 
				text_debug.GetComponent<Text>().text = t;
			}

			//check if player has reached destination; skip if player is still changing target (includes forked path decision making)
			if(!isChangingTarget){
				bool hasReachedDestination = false; //used to prevent big code duplication
				//logic separated for debugging
				if(gameObject.transform.position == targetTile.transform.GetChild(player_num).position){
					Debug.Log("Destination reached!");
					hasReachedDestination = true;
				}

				if(hasReachedDestination){
					if((numSpacesToMove <= 1 && !doneMoving) || targetTile.GetComponent<Tile>().isFinal){ //also stop player when at the final tile
						numSpacesToMove=0; //for display purposes
						doneMoving=true;//prevents multiple messages being sent

						if(targetTile.GetComponent<Tile>().isFinal){
							NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerStop_Final));
						}else{
							NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerStop));
						}

						
					}else if(numSpacesToMove > 1){
						numSpacesToMove-=1;
							Debug.Log(numSpacesToMove+" spaces left to move");
						//message server to changetarget
						isChangingTarget=true;
						NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerTargetChange));
					}
				}
			}
		}
	}

	[ClientRpc]
	public void RpcForkChoice(int i){ //0 is left and 1 is right
			if(isMovingForward){
				List<GameObject> targets = currentTile.GetComponent<Tile>().nextTiles;
				targetTile = targets[i];
				
				isChangingTarget = false; //continues movement
				atFork = false; //locks further left/right arrow key detection

				animationState = AnimationStates.HumanoidWalk;
			}else{
				List<GameObject> targets = currentTile.GetComponent<Tile>().previousTiles;
				
				targetTile = targets[i];
				//only fork that has 3 choices is the backwards path of the ending tile, so there is no need to handle 3 choices

				isChangingTarget = false; //continues movement
				atFork = false; //locks further left/right arrow key detection

				animationState = AnimationStates.HumanoidWalk;
			}
			
	}

	[ClientRpc]
	public void RpcMove(bool forwards){
		isMovingForward = forwards;
		isChangingTarget=targetNextTile(forwards);

		Debug.Log("Moving to tile "+targetTile);
		
		if(atFork){
			animationState = AnimationStates.HumanoidIdle;
		}else{
			animationState = AnimationStates.HumanoidWalk;
		}
		
		isMoving=true;
		doneMoving = false;
	}

	[ClientRpc]
	public void RpcStop(){
			Debug.Log("RpcStop");
		isMoving=false;
		animationState = AnimationStates.HumanoidIdle;

		//if the player used an item that was not freeze, restart their turn, else end their turn
		if(GameObject.Find("Ability Controller").GetComponent<Ability_Controller>().abilityUsed){
				Debug.Log("Player\tAbility USED this turn");
			if(movedAfterItem){ //prevents player from taking infinite turns after using an item
				movedAfterItem = false;
					Debug.Log("\tAbility already used, ending turn");
				TurnOver();
			}else{
				//allow player to roll dice after using item
				movedAfterItem = true;
				if(isLocalPlayer){		
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().dice.GetComponent<CanvasGroup>().alpha = 1f;
					GameObject.Find("Button Controller").GetComponent<Button_Controller>().dice.GetComponent<Button>().interactable = true;
				}
			}
		}else{
				Debug.Log("Player\tAbility NOT used this turn");
			if(isLocalPlayer)TurnOver();
		}
	}

	[ClientRpc]
	public void RpcStop_ReachedFinish(){ //same as RpcStop, but does not allow player to restart diceroll when item is used
			Debug.Log("RpcStop_ReachedFinish");
		isMoving=false;
		animationState = AnimationStates.HumanoidIdle;
		if(isLocalPlayer){
			text_finished.GetComponent<Text>().text = "You have arrived at the party, waiting for party to start...";
			TurnOver();
		}
	}

	[ClientRpc]
	public void RpcResetPlayer(int p_num){ //cannot send objects though rpc commands
		player_num=p_num;
		transform.position = currentTile.transform.GetChild(player_num).position;
	}

	[ClientRpc]
	public void RpcChangeTarget(){
		isChangingTarget=targetNextTile(isMovingForward);
		if(atFork){
			animationState = AnimationStates.HumanoidIdle;
		}else{
			animationState = AnimationStates.HumanoidWalk;
		}
	}

	[TargetRpc]
	public void TargetRpcBeginTurn(NetworkConnection target){
		isMyTurn = true; //only local player's turn flag is set
        GameObject.Find("Ability Controller").GetComponent<Ability_Controller>().abilityUsed = false;
        Debug.Log("TargetRpcBeginTurn Player"+player_num+"'s turn");
		if(isLocalPlayer){
			Debug.Log("\tEnabling Buttons");
			GameObject.Find("Button Controller").GetComponent<Button_Controller>().dice.GetComponent<CanvasGroup>().alpha = 1f;
			GameObject.Find("Button Controller").GetComponent<Button_Controller>().dice.GetComponent<Button>().interactable = true;
			GameObject.Find("Button Controller").GetComponent<Button_Controller>().dice.GetComponent<CanvasGroup>().interactable = true;
       		GameObject.Find("Ability Controller").GetComponent<Ability_Controller>().abilityButton.GetComponent<CanvasGroup>().alpha = 1f;
       		GameObject.Find("Ability Controller").GetComponent<Ability_Controller>().abilityButton.GetComponent<Button>().interactable = true;
		}
	}

	private void TurnOver(){
        //end frozen ability at end of turn
        isFrozen = false;
		isMyTurn = false;
		//message the server that the turn is over
		NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.TurnOver));
			Debug.Log("\tTurn ended");
	}

	//SyncVar hook to change animations
	void OnChangeAnimationState(AnimationStates state){
		GetComponent<AnimationController>().PlayAnimation(state);
	}

	//SyncVar hook to change dice button text when frozen
	void OnChangeIsFrozen(bool is_frozen){
		if(!isLocalPlayer)return;
		if(is_frozen){
       		GameObject.Find("Button Controller").GetComponent<Button_Controller>().dice.GetComponentInChildren<Text>().text = "Skip Turn";
		}else{
       		GameObject.Find("Button Controller").GetComponent<Button_Controller>().dice.GetComponentInChildren<Text>().text = "Roll Dice";
		}
	}

	public void DiceClicked(GameObject dice, GameObject ability){
		if(!isMoving && isMyTurn){
			dice.GetComponent<CanvasGroup>().alpha = 0f;
			dice.GetComponent<Button>().interactable = false;
        	ability.GetComponent<CanvasGroup>().alpha = 0f;
			ability.GetComponent<Button>().interactable = false;

            //skip turn due to player being frozen
            if(isFrozen)
            {
                TurnOver();
            }
            else
            {
                //force player to move n spaces for movement debugging
                if (debug_enable_DebugMode)
                {
                    numSpacesToMove = debug_force_DiceRoll;
                    if (player_num != 0) numSpacesToMove = 6;//force second player to move less so first player can finish first
                }
                else
                {
                    System.Random rng = new System.Random(); //C# System.Random, not Unity.Random
                    numSpacesToMove = rng.Next(1, max_tiles_per_turn + 1);
                }

                Debug.Log("You rolled a " + numSpacesToMove);

                if (debug_enable_DebugMode && debug_force_BackwardMovement)
                {
                    //force player to move backwards for debugging
                    NetworkManager.singleton.client.Send(MsgType.Highest + 1, new IntegerMessage((int)MyMessageType.ItemMoveBackwards));
                }
                else
                {
                    //tell server to tell each client to move player forwards
                    NetworkManager.singleton.client.Send(MsgType.Highest + 1, new IntegerMessage((int)MyMessageType.PlayerMoveForwards));
                }
            }
		}
	}

	public void RightClicked(GameObject left, GameObject right){
		if(atFork && isMyTurn){
			left.GetComponent<CanvasGroup>().alpha = 0f;
			left.GetComponent<Button>().interactable = false;
        	right.GetComponent<CanvasGroup>().alpha = 0f;
			right.GetComponent<Button>().interactable = false;
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerForkChoice_Right));
		}
	}

	public void LeftClicked(GameObject left, GameObject right){
		if(atFork && isMyTurn){
			left.GetComponent<CanvasGroup>().alpha = 0f;
			left.GetComponent<Button>().interactable = false;
        	right.GetComponent<CanvasGroup>().alpha = 0f;
			right.GetComponent<Button>().interactable = false;
			NetworkManager.singleton.client.Send(MsgType.Highest+1,new IntegerMessage((int)MyMessageType.PlayerForkChoice_Left));
		}
	}

}