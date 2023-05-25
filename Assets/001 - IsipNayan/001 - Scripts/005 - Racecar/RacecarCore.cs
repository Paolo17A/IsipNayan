using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;

public class RacecarCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum GameplayStates
    {
        NONE,
        TUTORIAL,
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

    [Header("TUTORIAL VARIABLES")]
    [SerializeField] private GameObject TutorialPanel;

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

    int failedCallbackCounter;
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

    #region TUTORIAL
    public void DisplayTutorialPanel()
    {
        TutorialPanel.SetActive(true);
    }

    public void ProceedToCountdown()
    {
        TutorialPanel.SetActive(false);
        CurrentGameplayState = GameplayStates.COUNTDOWN;
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
                CurrentQuestionIndex++;
                if(CurrentQuestionIndex == AllQuestions.Count)
                {
                    FinalResult = GameManager.Result.VICTORY;
                    CurrentGameplayState = GameplayStates.GAMEOVER;
                }
                /*(if(CurrentQuestionIndex == AllQuestions.Count - 1)
                {
                    Shuffle(AllQuestions);
                    CurrentQuestionIndex = 0;
                }
                else
                {
                    CurrentQuestionIndex++;
                }*/
                else
                    DisplayNewQuestion();
            }
        }
    }

   
    #endregion

    #region QUESTION
    public void DisplayNewQuestion()
    {
        panelMoveSpeed += 0.1f;
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
        PlayerData.AddGameHistory(PlayerData.GameType.RACECAR, GameManager.Result.DEFEAT, DriverCore.CurrentScore);

        if (GameManager.Instance.DebugMode)
        {
            CurrentGameplayState = GameplayStates.COUNTDOWN;
            ResumeGame();
        }
        else
        {
            LoadingPanel.SetActive(true);
            UpdateGameHistoryPlayFab(true);
        }
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
        Debug.Log(JsonConvert.SerializeObject(PlayerData.PlayerHistory));
        if(!GameManager.Instance.DebugMode)
        {
            LoadingPanel.SetActive(true);
            UpdateGameHistoryPlayFab(false);
        }
    }

    public void ProcessDefeat()
    {
        DefeatPanel.SetActive(true);
        PlayerData.AddGameHistory(PlayerData.GameType.RACECAR, FinalResult, DriverCore.CurrentScore);
        Debug.Log(JsonConvert.SerializeObject(PlayerData.PlayerHistory));
        if (!GameManager.Instance.DebugMode)
        {
            LoadingPanel.SetActive(true);
            UpdateGameHistoryPlayFab(false);
        }
    }

    private void UpdateGameHistoryPlayFab(bool restarting)
    {
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>
        {
            { "GameHistory", JsonConvert.SerializeObject(PlayerData.PlayerHistory) }
        };

        PlayFabClientAPI.UpdateUserData(updateUserData, 
            resultCallback =>
            {
                failedCallbackCounter = 0;
                LoadingPanel.SetActive(false);

                if(restarting)
                {
                    CurrentGameplayState = GameplayStates.COUNTDOWN;
                    ResumeGame();
                }
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => UpdateGameHistoryPlayFab(restarting),
                    () => DisplayErrorPanel(errorCallback.ErrorMessage));
            });
    }
    #endregion

    #region ERROR
    private void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                DisplayErrorPanel("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else
            errorAction();
    }
    public void DisplayErrorPanel(string errorMessage)
    {
        LoadingPanel.SetActive(false);
        ErrorPanel.SetActive(true);
        ErrorTMP.text = errorMessage;
    }

    public void CloseErrorPanel()
    {
        ErrorPanel.SetActive(false);
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

