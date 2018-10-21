using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public Button playButton, optionsButton, creditsButton, exitButton;
    public GameObject instMenu, playMenu, optionsMenu, creditsMenu;
    private GameObject subMenu;
    //Animator animator;

    // Use this for initialization
    void Start()
    {
        subMenu = instMenu;
        playButton.onClick.AddListener(clickPlay);
        optionsButton.onClick.AddListener(clickOptions);
        exitButton.onClick.AddListener(clickExit);
        //animator = GetComponent<Animator>();
    }

    void switchMenu(GameObject newMenu)
    {
        subMenu.SetActive(false);
        newMenu.SetActive(true);
        subMenu = newMenu;
    }

    void clickPlay()
    {
        switchMenu(playMenu);
    }

    void clickOptions()
    {
        switchMenu(optionsMenu);
    }

    void clickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
