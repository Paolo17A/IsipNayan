using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacecarController : MonoBehaviour
{
    [SerializeField] RacecarCore RacecarCore;

    private void Awake()
    {
        RacecarCore.onGameplayStateChange += GameplayStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void Start()
    {
        RacecarCore.SetOriginalPanels();
        RacecarCore.CurrentGameplayState = RacecarCore.GameplayStates.TUTORIAL;
    }

    private void Update()
    {
        if (RacecarCore.CurrentGameplayState == RacecarCore.GameplayStates.COUNTDOWN)
            RacecarCore.ReduceCountdownTimer();
        else if (RacecarCore.CurrentGameplayState == RacecarCore.GameplayStates.GAMEPLAY) 
            RacecarCore.MovePanels();
    }

    private void GameplayStateChange(object sender, EventArgs e)
    {
        switch (RacecarCore.CurrentGameplayState)
        {
            case RacecarCore.GameplayStates.TUTORIAL:
                RacecarCore.DisplayTutorialPanel();
                break;
            case RacecarCore.GameplayStates.COUNTDOWN:
                RacecarCore.InitializeRacecarGame();
                break;
            case RacecarCore.GameplayStates.GAMEPLAY:
                RacecarCore.DisplayNewQuestion();
                break;
            case RacecarCore.GameplayStates.GAMEOVER:
                if (RacecarCore.FinalResult == GameManager.Result.VICTORY)
                    RacecarCore.ProcessVictory();
                else if (RacecarCore.FinalResult == GameManager.Result.DEFEAT)
                    RacecarCore.ProcessDefeat();
                break;
        }
    }
}
