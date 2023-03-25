using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RacecarCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum GameplayStates
    {
        NONE,
        COUNTDOWN,
        GAMEPLAY,
        GAMEOVER
    }

    private event EventHandler gameplayStateChange;
    public event EventHandler onGameplayStateChange
    {
        add
        {
            if (gameplayStateChange == null || !gameplayStateChange.GetInvocationList().Contains(value))
                gameplayStateChange += value;
        }
        remove { gameplayStateChange -= value; }
    }

    public GameplayStates CurrentGameplayState
    {
        get => gameplayState;
        set
        {
            gameplayState = value;
            gameplayStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private GameplayStates gameplayState;
    #endregion

    #region VARIABLES
    //=========================================================================================================
    [SerializeField] private DriverCore DriverCore;
    [SerializeField] private PlayerData PlayerData;

    [Header("COUNTDOWN VARIABLES")]
    [SerializeField] private GameObject CountdownPanel;
    [SerializeField] private TextMeshProUGUI CountdownTMP;
    [SerializeField][ReadOnly] private float CountdownValue;

    [Header("PANEL VARIABLES")]
    [SerializeField] private GameObject BottomPanel;
    [SerializeField] private GameObject TopPanel;
    [SerializeField][ReadOnly] private GameObject OriginalBottomPanel;
    [SerializeField][ReadOnly] private GameObject OriginalTopPanel;
    [SerializeField] private float TopPanelConstant;
    [SerializeField] private float startingPanelMoveSpeed;
    [SerializeField][ReadOnly] private float panelMoveSpeed;

    [Header("QUESTION VARIABLES")]
    [SerializeField] private List<QuestionData> AllQuestions;
    [SerializeField] private List<GasAnswerHandler> GasAnswers;
    [SerializeField] private TextMeshProUGUI QuestionTMP;
    [SerializeField][ReadOnly] private int CurrentQuestionIndex;
    [ReadOnly] public bool WillResetQuestion;
    private GameObject HolderPanel;

    [Header("PAUSE VARIABLES")]
    [SerializeField] private GameObject PausePanel;

    [Header("GAMEOVER VARIABLES")]
    [ReadOnly] public GameManager.Result FinalResult;
    [SerializeField] private GameObject VictoryPanel;
    [SerializeField] private GameObject DefeatPanel;
    [SerializeField] private GameObject LoadingPanel;

    [Header("ERROR")]
    [SerializeField] private GameObject ErrorPanel;
    [SerializeField] private TextMeshProUGUI ErrorTMP;
    //=========================================================================================================
    #endregion

    #region INITIALIZATION
    public void SetOriginalPanels()
    {
        OriginalBottomPanel = BottomPanel;
        OriginalTopPanel = TopPanel;
    }

    public void InitializeRacecarGame()
    {
        DriverCore.ResetDriver();

        #region COUNTDOWN
        CountdownPanel.SetActive(true);
        CountdownValue = 4;
        CountdownTMP.text = CountdownValue.ToString();
        #endregion

        #region PANEL
        panelMoveSpeed = startingPanelMoveSpeed;
        BottomPanel = OriginalBottomPanel;
        TopPanel = OriginalTopPanel;
        BottomPanel.transform.position = Vector3.zero;
        TopPanel.transform.position = new Vector3(0, 4.9f, 0);
        #endregion

        #region QUESTION
        Shuffle(AllQuestions);
        CurrentQuestionIndex = 0;
        QuestionTMP.gameObject.SetActive(false);
        #endregion

        #region GAMEOVER
        FinalResult = GameManager.Result.NONE;
        VictoryPanel.SetActive(false);
        DefeatPanel.SetActive(false);
        LoadingPanel.SetActive(false);
        #endregion
    }

    public void ReduceCountdownTimer()
    {
        if (CountdownValue > 1)
        {
            CountdownValue -= Time.deltaTime;
            CountdownTMP.text = Mathf.FloorToInt(Math.Max(0, CountdownValue)).ToString();
        }
        else
        {
            CountdownPanel.SetActive(false);
            CurrentGameplayState = GameplayStates.GAMEPLAY;
        }
    }
    #endregion

    #region PANEL
    public void MovePanels()
    {
        BottomPanel.transform.Translate(Vector3.down * panelMoveSpeed * Time.deltaTime);
        TopPanel.transform.Translate(Vector3.down * panelMoveSpeed * Time.deltaTime);
        if (TopPanel.transform.position.y <= 0)
        {
            BottomPanel.gameObject.transform.position = new Vector3(BottomPanel.transform.position.x, TopPanelConstant, BottomPanel.transform.position.z);
            HolderPanel = BottomPanel;
            BottomPanel = TopPanel;
            TopPanel = HolderPanel;
            HolderPanel = null;

            if (WillResetQuestion)
            {
                WillResetQuestion = false;
                if(CurrentQuestionIndex == AllQuestions.Count - 1)
                {
                    Shuffle(AllQuestions);
                    CurrentQuestionIndex = 0;
                }
                else
                    CurrentQuestionIndex++;

                DisplayNewQuestion();
            }
        }
    }

   
    #endregion

    #region QUESTION
    public void DisplayNewQuestion()
    {
        ToggleGasAnswers(true);
        QuestionTMP.text = AllQuestions[CurrentQuestionIndex].Question;
        Shuffle(AllQuestions[CurrentQuestionIndex].Choices);
        for(int i = 0; i < GasAnswers.Count; i++)
        {
            GasAnswers[i].AssignAnswer(AllQuestions[CurrentQuestionIndex].Choices[i]);
            if (AllQuestions[CurrentQuestionIndex].Answer == AllQuestions[CurrentQuestionIndex].Choices[i])
                GasAnswers[i].IsCorrectAnswer = true;
            else
                GasAnswers[i].IsCorrectAnswer = false;
        }
    }
    
    public void ToggleGasAnswers(bool displayState)
    {
        QuestionTMP.gameObject.SetActive(displayState);
        foreach (GasAnswerHandler gas in GasAnswers)
            gas.gameObject.SetActive(displayState);
    }
    #endregion


    #region PAUSE
    public void PauseGame()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        PausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        CurrentGameplayState = GameplayStates.COUNTDOWN;
        ResumeGame();
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.SceneController.CurrentScene = "MainMenuScene";
    }
    #endregion

    #region GAMEOVER
    public void ProcessVictory()
    {
        VictoryPanel.SetActive(true);
        PlayerData.AddGameHistory(PlayerData.GameType.RACECAR, FinalResult, DriverCore.CurrentScore);
        if(!GameManager.Instance.DebugMode)
        {
            LoadingPanel.SetActive(true);
        }
    }

    public void ProcessDefeat()
    {
        DefeatPanel.SetActive(true);
        PlayerData.AddGameHistory(PlayerData.GameType.RACECAR, FinalResult, DriverCore.CurrentScore);
        if (!GameManager.Instance.DebugMode)
        {
            LoadingPanel.SetActive(true);
        }
    }
    #endregion

    #region UTILITY
    public void Shuffle<Transform>(List<Transform> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
    #endregion
}

