using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private MainMenuCore MainMenuCore;

    private void Awake()
    {
        MainMenuCore.onMainMenuStateChange += MainMenuStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        MainMenuCore.onMainMenuStateChange -= MainMenuStateChange;
    }

    private void Start()
    {

        MainMenuCore.CurrentMainMenuState = MainMenuCore.MainMenuStates.LOGIN;
    }

    private void MainMenuStateChange(object sender, EventArgs e)
    {
        switch(MainMenuCore.CurrentMainMenuState)
        {
            case MainMenuCore.MainMenuStates.LOGIN:
                MainMenuCore.ShowLoginPanel();
                break;
            case MainMenuCore.MainMenuStates.REGISTER: 
                MainMenuCore.ShowRegisterPanel(); 
                break;
            case MainMenuCore.MainMenuStates.WELCOME:
                MainMenuCore.ShowWelcomePanel();
                break;
            case MainMenuCore.MainMenuStates.STORY:
                MainMenuCore.ShowStoryPanel();
                break;
            case MainMenuCore.MainMenuStates.GAMESELECT:
                MainMenuCore.ShowGameSelectPanel();
                break;
            case MainMenuCore.MainMenuStates.PROFILE:
                MainMenuCore.ShowProfilePanel();
                break;
        }
    }
}
