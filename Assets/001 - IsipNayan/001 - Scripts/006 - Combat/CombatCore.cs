using Newtonsoft.Json;
using PlayFab.ClientModels;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static RacecarCore;

public class CombatCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum CombatStates
    {
        NONE,
        COUNTDOWN,
        TIMER,
        PLAYERTURN,
        ENEMYTURN,
        GAMEOVER
    }

    private event EventHandler combatStateChange;
    public event EventHandler onCombatStateChange
    {
        add
        {
            if (combatStateChange == null || !combatStateChange.GetInvocationList().Contains(value))
                combatStateChange += value;
        }
        remove { combatStateChange -= value; }
    }

    public CombatStates CurrentCombatState
    {
        get => combatState;
        set
        {
            combatState = value;
            combatStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private CombatStates combatState;
    #endregion

    #region VARIABLES
    //==================================================================================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("COUNTDOWN VARIABLES")]
    [SerializeField] private GameObject CountdownPanel;
    [SerializeField] private TextMeshProUGUI CountdownTMP;
    [SerializeField][ReadOnly] private float CountdownValue;

    [Header("TIMER")]
    [SerializeField] private TextMeshProUGUI TimerTMP;
    [SerializeField] private int MaxTimerValue;
    [field: SerializeField][field: ReadOnly] public int CurrentTimerValue { get; set; }
    [SerializeField][ReadOnly] private float TimerValueLeft;

    [Header("QUESTION")]
    [SerializeField] private List<QuestionData> AllQuestions;
    [SerializeField] private TextMeshProUGUI QuestionTMP;
    [SerializeField] private List<MultiplicationHandler> MultiplicationHandlers;
    [SerializeField] private List<ChoiceButtonHandler> ChoiceButtons;
    [ReadOnly] public int CurrentQuestionIndex;

    [Header("CHARACTERS")]
    public CharacterCombatCore PlayerCharacter;
    public CharacterCombatCore EnemyCharacter;

    [Header("PAUSE")]
    [SerializeField] private GameObject PausePanel;

    [Header("GAME OVER")]
    [ReadOnly] public GameManager.Result FinalResult;
    [SerializeField] private GameObject VictoryPanel;
    [SerializeField] private GameObject DefeatPanel;
    [ReadOnly] public bool EnemyWillRunAway;

    [Header("LOADING")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("ERROR")]
    [SerializeField] private GameObject ErrorPanel;
    [SerializeField] private TextMeshProUGUI ErrorTMP;

    private int failedCallbackCounter;
    //==================================================================================================================
    #endregion

    #region INITIALIZATION
    public void InitializeQuizGame()
    {
        #region COUNTDOWN
        CountdownPanel.SetActive(true);
        CountdownValue = 4;
        CountdownTMP.text = CountdownValue.ToString();
        #endregion

        #region QUESTIONS
        Shuffle(AllQuestions);
        CurrentQuestionIndex = 0;
        ToggleQuestionObjects(false);
        #endregion

        #region CHARACTERS
        PlayerCharacter.InitializeCharacter();
        EnemyCharacter.InitializeCharacter();
        EnemyWillRunAway = false;
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
            CurrentCombatState = CombatStates.TIMER;

        }
    }
    #endregion

    #region TIMER
    public void ResetTimer()
    {
        CurrentTimerValue = MaxTimerValue;
        TimerValueLeft = CurrentTimerValue;
        TimerTMP.text = CurrentTimerValue.ToString();

        DisplayCurrentQuestion();
    }

    public void DecreaseTimer()
    {
        if (CurrentTimerValue > 0)
        {
            TimerValueLeft -= Time.deltaTime;
            CurrentTimerValue = (int)TimerValueLeft;
            TimerTMP.text = CurrentTimerValue.ToString();
        }
        else
        {
            ToggleQuestionObjects(false);
            CurrentCombatState = CombatStates.ENEMYTURN;
        }
    }

    #endregion

    #region QUESTIONS
    private void ToggleQuestionObjects(bool _bool)
    {
        //QuestionTMP.gameObject.SetActive(_bool);
        foreach (ChoiceButtonHandler choice in ChoiceButtons)
            choice.gameObject.SetActive(_bool);
        TimerTMP.gameObject.SetActive(_bool);
        foreach(MultiplicationHandler mult in MultiplicationHandlers)
            mult.gameObject.SetActive(_bool);
    }

    private void DisplayCurrentQuestion()
    {
        ToggleQuestionObjects(true);

        for(int i = 0; i < MultiplicationHandlers.Count; i++)
        {
            if(i < AllQuestions[CurrentQuestionIndex].MultiplicationQuestion.Count)
            {
                MultiplicationHandlers[i].gameObject.SetActive(true);
                MultiplicationHandlers[i].DisplayProperMultiplicands(AllQuestions[CurrentQuestionIndex].MultiplicationQuestion[i].multipilicand, AllQuestions[CurrentQuestionIndex].MultiplicationQuestion[i].multiplier);
            }
            else
                MultiplicationHandlers[i].gameObject.SetActive(false);
        }


        QuestionTMP.text = AllQuestions[CurrentQuestionIndex].Question;
        Shuffle(AllQuestions[CurrentQuestionIndex].Choices);
        for (int i = 0; i < ChoiceButtons.Count; i++)
        {
            ChoiceButtons[i].AssignAnswer(AllQuestions[CurrentQuestionIndex].Choices[i]);
            if (AllQuestions[CurrentQuestionIndex].Answer == AllQuestions[CurrentQuestionIndex].Choices[i])
                ChoiceButtons[i].IsCorrectAnswer = true;
            else
                ChoiceButtons[i].IsCorrectAnswer = false;
        }
    }

    public void AssignNewQuestion()
    {
        CurrentQuestionIndex++;
        if(CurrentQuestionIndex == AllQuestions.Count)
        {
            EnemyWillRunAway = true;
            /*CurrentQuestionIndex = 0;
            Shuffle(AllQuestions);
            EnemyCharacter.CurrentCharacterCombatState = CharacterCombatCore.CharacterCombatState.DYING;*/
        }
        ToggleQuestionObjects(false);
    }
    #endregion

    #region PAUSE
    public void PauseGame()
    {
        Time.timeScale = 0;
        PausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        PlayerData.AddGameHistory(PlayerData.GameType.QUIZ, GameManager.Result.DEFEAT, EnemyCharacter.GetDamageDealt());

        if (GameManager.Instance.DebugMode)
        {
            CurrentCombatState = CombatStates.COUNTDOWN;
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
        ResumeGame();
        GameManager.Instance.SceneController.CurrentScene = "MainMenuScene";
    }
    #endregion

    #region GAME OVER
    public void ProcessVictory()
    {
        VictoryPanel.SetActive(true);
        PlayerData.AddGameHistory(PlayerData.GameType.QUIZ, FinalResult, EnemyCharacter.GetDamageDealt());
        UpdateGameHistoryPlayFab(false);
    }

    public void ProcessDefeat()
    {
        DefeatPanel.SetActive(true);
        PlayerData.AddGameHistory(PlayerData.GameType.QUIZ, FinalResult, EnemyCharacter.GetDamageDealt());
        UpdateGameHistoryPlayFab(false);
    }

    private void UpdateGameHistoryPlayFab(bool restarting)
    {
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
        updateUserData.Data.Add("GameHistory", JsonConvert.SerializeObject(PlayerData.PlayerHistory));

        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                LoadingPanel.SetActive(false);

                if (restarting)
                {
                    CurrentCombatState = CombatStates.COUNTDOWN;
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
