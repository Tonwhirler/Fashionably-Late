using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;


public class Ability_Controller : MonoBehaviour {
    public GameObject playerOneButton;
    public GameObject playerTwoButton;
    public GameObject playerThreeButton;
    public GameObject playerFourButton;

    public GameObject abilityButton;
    public GameObject diceButton;

    [HideInInspector]
    public GameObject player;
    [HideInInspector]
    public bool abilityUsed = false;
    private int currentAbility = 0;

    private Text text_ability;

    public void GiveStartingAbilities()
    {
        text_ability = GameObject.Find("AbilityButton").GetComponentInChildren<Text>();
        //give player ability to start with
        RandomAbility();
    }

    public enum MyAbilityType
    {
        ForceBack = 1,
        ForceForward = 2,
        Freeze = 3
    }

    public void SetPlayer(GameObject target)
    {
        player = target;
    }

    public void UseAbility()
    {
        Debug.Log("Use ability called");
        if (currentAbility == 0)
        {
            GiveStartingAbilities();
        }
        switch (currentAbility)
        {
            case (int) MyAbilityType.ForceBack:
                
                if (abilityUsed == false && !player.GetComponent<Player_Behavior>().isMoving && player.GetComponent<Player_Behavior>().isMyTurn)
                {
                    Debug.Log("Use ability ForceBack");
                    //move player 1 space back, the 1 space is temporary
                    player.GetComponent<Player_Behavior>().numSpacesToMove = 1;
                    NetworkManager.singleton.client.Send(MsgType.Highest + 1, new IntegerMessage((int)MyMessageType.ItemMoveBackwards));
                    abilityUsed = true;
                    //give new ability when old one is used
                    RandomAbility();

                    playerOneButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerTwoButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerThreeButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerFourButton.GetComponent<CanvasGroup>().alpha = 1f;

                    playerOneButton.GetComponent<Button>().interactable = true;
                    playerTwoButton.GetComponent<Button>().interactable = true;
                    playerThreeButton.GetComponent<Button>().interactable = true;
                    playerFourButton.GetComponent<Button>().interactable = true;

                    abilityButton.GetComponent<CanvasGroup>().alpha = 0f;
                    abilityButton.GetComponent<Button>().interactable = false;
                    diceButton.GetComponent<CanvasGroup>().alpha = 0f;
                    diceButton.GetComponent<Button>().interactable = false;
                }            
                break;

            case (int) MyAbilityType.ForceForward:

                if (abilityUsed == false && !player.GetComponent<Player_Behavior>().isMoving && player.GetComponent<Player_Behavior>().isMyTurn)
                {
                    Debug.Log("Use ability ForceForward");
                    //move player 1 space forward, the 1 space is temporary
                    player.GetComponent<Player_Behavior>().numSpacesToMove = 1;
                    NetworkManager.singleton.client.Send(MsgType.Highest + 1, new IntegerMessage((int)MyMessageType.PlayerMoveForwards));
                    abilityUsed = true;
                    //give new ability when old one is used
                    RandomAbility();

                    playerOneButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerTwoButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerThreeButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerFourButton.GetComponent<CanvasGroup>().alpha = 1f;

                    playerOneButton.GetComponent<Button>().interactable = true;
                    playerTwoButton.GetComponent<Button>().interactable = true;
                    playerThreeButton.GetComponent<Button>().interactable = true;
                    playerFourButton.GetComponent<Button>().interactable = true;

                    abilityButton.GetComponent<CanvasGroup>().alpha = 0f;
                    abilityButton.GetComponent<Button>().interactable = false;
                    diceButton.GetComponent<CanvasGroup>().alpha = 0f;
                    diceButton.GetComponent<Button>().interactable = false;
                }
                break;

            case (int) MyAbilityType.Freeze:

                if (abilityUsed == false && !player.GetComponent<Player_Behavior>().isMoving && player.GetComponent<Player_Behavior>().isMyTurn)
                {
                    Debug.Log("Use ability Freeze");
                    //set frozen bool so player cannot take turn
                    player.GetComponent<Player_Behavior>().isFrozen = true;
                    abilityUsed = true;
                    //give new ability when old one is used
                    RandomAbility();

                    playerOneButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerTwoButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerThreeButton.GetComponent<CanvasGroup>().alpha = 1f;
                    playerFourButton.GetComponent<CanvasGroup>().alpha = 1f;

                    playerOneButton.GetComponent<Button>().interactable = true;
                    playerTwoButton.GetComponent<Button>().interactable = true;
                    playerThreeButton.GetComponent<Button>().interactable = true;
                    playerFourButton.GetComponent<Button>().interactable = true;

                    abilityButton.GetComponent<CanvasGroup>().alpha = 0f;
                    abilityButton.GetComponent<Button>().interactable = false;
                }              
                break;

            default:
                Debug.Log("Ability enum does not exist");
                break;
        }
    }

    public void RandomAbility()
    {
        currentAbility = Random.Range(1,4); // creates a number between 1 and 3
        Debug.Log("Current Ability = " + currentAbility);
        UpdateUI();
    }

    private void UpdateUI()
    {
        //update the text on the button
        switch (currentAbility)
        {
            case (int)MyAbilityType.ForceBack:
                text_ability.text = "Force Back";
                break;

            case (int)MyAbilityType.ForceForward:
                text_ability.text = "Force Forward";
                break;

            case (int)MyAbilityType.Freeze:
                text_ability.text = "Freeze";
                break;

            default:
                Debug.Log("Ability button text not set");
                break;
        }
    }

    public void TargetPlayerOne()
    {
        //change target player here

        playerOneButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerTwoButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerThreeButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerFourButton.GetComponent<CanvasGroup>().alpha = 0f;

        playerOneButton.GetComponent<Button>().interactable = false;
        playerTwoButton.GetComponent<Button>().interactable = false;
        playerThreeButton.GetComponent<Button>().interactable = false;
        playerFourButton.GetComponent<Button>().interactable = false;
    }
    public void TargetPlayerTwo()
    {
        //change target player here

        playerOneButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerTwoButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerThreeButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerFourButton.GetComponent<CanvasGroup>().alpha = 0f;

        playerOneButton.GetComponent<Button>().interactable = false;
        playerTwoButton.GetComponent<Button>().interactable = false;
        playerThreeButton.GetComponent<Button>().interactable = false;
        playerFourButton.GetComponent<Button>().interactable = false;
    }
    public void TargetPlayerThree()
    {
        //change target player here

        playerOneButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerTwoButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerThreeButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerFourButton.GetComponent<CanvasGroup>().alpha = 0f;

        playerOneButton.GetComponent<Button>().interactable = false;
        playerTwoButton.GetComponent<Button>().interactable = false;
        playerThreeButton.GetComponent<Button>().interactable = false;
        playerFourButton.GetComponent<Button>().interactable = false;
    }
    public void TargetPlayerFour()
    {
        //change target player here

        playerOneButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerTwoButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerThreeButton.GetComponent<CanvasGroup>().alpha = 0f;
        playerFourButton.GetComponent<CanvasGroup>().alpha = 0f;

        playerOneButton.GetComponent<Button>().interactable = false;
        playerTwoButton.GetComponent<Button>().interactable = false;
        playerThreeButton.GetComponent<Button>().interactable = false;
        playerFourButton.GetComponent<Button>().interactable = false;
    }
}
