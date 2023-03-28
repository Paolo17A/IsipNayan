using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class MainMenuCore : MonoBehaviour
{
    #region STATE MACHINE
    //================================================================================================================
    public enum MainMenuStates
    {
        NONE,
        LOGIN,
        REGISTER,
        WELCOME,
        STORY,
        GAMESELECT,
        PROFILE
    }

    private event EventHandler mainMenuStateChange;
    public event EventHandler onMainMenuStateChange
    {
        add
        {
            if (mainMenuStateChange == null || !mainMenuStateChange.GetInvocationList().Contains(value))
                mainMenuStateChange += value;
        }
        remove { mainMenuStateChange -= value; }
    }

    public MainMenuStates CurrentMainMenuState
    {
        get => mainMenuStates;
        set
        {
            mainMenuStates = value;
            mainMenuStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private MainMenuStates mainMenuStates;
    //=============================================================================================================
    #endregion

    #region VARIABLES
    //=============================================================================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("PANELS")]
    [SerializeField] private RectTransform SplashRT;
    [SerializeField] private CanvasGroup SplashCG;
    [SerializeField] private RectTransform LoginRT;
    [SerializeField] private CanvasGroup LoginCG;
    [SerializeField] private RectTransform RegisterRT;
    [SerializeField] private CanvasGroup RegisterCG;
    [SerializeField] private RectTransform WelcomeRT;
    [SerializeField] private CanvasGroup WelcomeCG;
    [SerializeField] private RectTransform StoryRT;
    [SerializeField] private CanvasGroup StoryCG;
    [SerializeField] private RectTransform GameSelectRT;
    [SerializeField] private CanvasGroup GameSelectCG;
    [SerializeField] private RectTransform ProfileRT;
    [SerializeField] private CanvasGroup ProfileCG;

    [Header("LOADING")]
    [SerializeField] private GameObject LoadingPanel;

    [Header("ERROR")]
    [SerializeField] private GameObject ErrorPanel;
    [SerializeField] private TextMeshProUGUI ErrorTMP; 

    [Header("LOGIN")]
    [SerializeField] private TMP_InputField LoginEmailTMPInput;
    [SerializeField] private TMP_InputField LoginPasswordTMPInput;

    [Header("REGISTRATIONS")]
    [SerializeField] private TMP_InputField RegisterUsernameTMPInput;
    [SerializeField] private TMP_InputField RegisterEmailTMPInput;
    [SerializeField] private TMP_InputField RegisterPasswordTMPInput;
    [SerializeField] private TMP_InputField RegisterConfirmPasswordTMPInput;

    [Header("WELCOME")]
    [SerializeField] private TextMeshProUGUI WelcomeTMP;

    [Header("STORY")]
    [SerializeField] private List<StoryPanelData> StoryPanels;
    [SerializeField][ReadOnly] private int CurrentStoryPageIndex;
    [SerializeField] private Button PreviousPageBtn;
    [SerializeField] private Button NextPageBtn;
    [SerializeField] private Image StoryImage;
    [SerializeField] private TextMeshProUGUI StoryText;

    [Header("PROFILE")]
    [SerializeField] private TextMeshProUGUI ProfileUsernameTMP;
    [SerializeField] private TextMeshProUGUI ProfileEmailTMP;
    [SerializeField] private Transform HistoryContainer;
    [SerializeField] private HistoryDataHandler HistoryPrefab;

    int failedCallbackCounter;
    //=============================================================================================================
    #endregion

    #region PANELS
    public void ShowLoginPanel()
    {
        ResetLoginFields();
        GameManager.Instance.AnimationsLT.FadePanel(LoginRT, null, LoginCG, 0, 1, () => { });
    }

    public void ShowRegisterPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(LoginRT, LoginRT, LoginCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(RegisterRT, null, RegisterCG, 0, 1, () => { });
    }

    public void HideRegisterPanel()
    {
        ResetRegistrationFields();
        GameManager.Instance.AnimationsLT.FadePanel(RegisterRT, RegisterRT, RegisterCG, 1, 0, () => { });
    }

    public void ShowWelcomePanel()
    {
        WelcomeTMP.text = "Hi " + PlayerData.Username + " welcome to iSIPNAYAN\r\n\r\nA mobile application empoyed with a game-based learning approach that will assist you in learning Mathematics! Here, you will explore different games, have fun and learn all at once! Before that, let's meet Naya through the Storyboard!";
        GameManager.Instance.AnimationsLT.FadePanel(LoginRT, LoginRT, LoginCG, 1, 0, () => { });
        GameManager.Instance.AnimationsLT.FadePanel(WelcomeRT, null, WelcomeCG, 0, 1, () => { });
    }

    public void HideWelcomePanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(WelcomeRT, WelcomeRT, WelcomeCG, 1, 0, () => { });
    }

    public void ShowStoryPanel()
    {
        CurrentStoryPageIndex = 0;
        DisplayCurrentStoryPage();
        GameManager.Instance.AnimationsLT.FadePanel(StoryRT, null, StoryCG, 0, 1, () => { });
    }

    public void HideStoryPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(StoryRT, StoryRT, StoryCG, 1, 0, () => { });
    }

    public void ShowGameSelectPanel()
    {
        ProfileUsernameTMP.text = "Username: " + PlayerData.Username;
        ProfileEmailTMP.text = "Email Address: " + PlayerData.EmailAddress;
        foreach (Transform child in HistoryContainer)
            Destroy(child.gameObject);
        HistoryDataHandler historyInstance;
        foreach (PlayerData.GameHistory history in PlayerData.PlayerHistory)
        {
            historyInstance = Instantiate(HistoryPrefab);
            historyInstance.transform.SetParent(HistoryContainer);
            historyInstance.transform.localScale = Vector3.one;
            historyInstance.transform.localPosition = new Vector3(historyInstance.transform.localPosition.x, historyInstance.transform.localPosition.y, 0);
            historyInstance.InitializeThisHistory(history);
        }
        GameManager.Instance.AnimationsLT.FadePanel(GameSelectRT, null, GameSelectCG, 0, 1, () => { });
    }

    public void HideGameSelectPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(GameSelectRT, GameSelectRT, GameSelectCG, 1, 0, () => { });
    }

    public void ShowProfilePanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, null, ProfileCG, 0, 1, () => { });
    }

    public void HideProfilePanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(ProfileRT, ProfileRT, ProfileCG, 1, 0, () => { });
    }

    public void MainMenuStateToIndex(int index)
    {
        switch(index) 
        {
            case (int)MainMenuStates.LOGIN:
                CurrentMainMenuState = MainMenuStates.LOGIN;
                break;
            case (int)MainMenuStates.REGISTER:
                CurrentMainMenuState = MainMenuStates.REGISTER;
                break;
            case (int)MainMenuStates.WELCOME:
                CurrentMainMenuState = MainMenuStates.WELCOME;
                break;
            case (int)MainMenuStates.STORY:
                CurrentMainMenuState = MainMenuStates.STORY;
                break;
            case (int)MainMenuStates.GAMESELECT:
                CurrentMainMenuState = MainMenuStates.GAMESELECT;
                break;
            case (int)MainMenuStates.PROFILE:
                CurrentMainMenuState = MainMenuStates.PROFILE;
                break;
        }
    }
    #endregion

    #region LOGIN
    public void LoginUser()
    {
        if (LoginEmailTMPInput.text == "" || LoginPasswordTMPInput.text == "")
        {
            DisplayError("Please input both your email and password");
            ResetLoginFields();
            return;
        }

        if(!LoginEmailTMPInput.text.Contains("@")|| !LoginEmailTMPInput.text.Contains(".com"))
        {
            DisplayError("Please input a valid email address");
            ResetLoginFields();
            return;
        }

        if (GameManager.Instance.DebugMode)
        {
            //PlayerData.Username = Login.text;
            PlayerData.EmailAddress = LoginEmailTMPInput.text;
            ProfileUsernameTMP.text = "Username: " + PlayerData.Username;
            ProfileEmailTMP.text = "Email Address: " + PlayerData.EmailAddress;
            CurrentMainMenuState = MainMenuStates.WELCOME;
        }
        else
        {
            LoadingPanel.SetActive(true);
            LoginWithPlayFab();
        }
    }

    public void LoginWithPlayFab()
    {
        LoginWithEmailAddressRequest loginWithEmail = new LoginWithEmailAddressRequest();
        loginWithEmail.Email = LoginEmailTMPInput.text;
        loginWithEmail.Password = LoginPasswordTMPInput.text;

        PlayFabClientAPI.LoginWithEmailAddress(loginWithEmail,
            resultCallback =>
            {
                failedCallbackCounter = 0;

                PlayerData.PlayFabID = resultCallback.PlayFabId;
                GetAccountInfoPlayFab();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                   FailedAction,
                   LoginWithPlayFab,
                   () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void GetAccountInfoPlayFab()
    {
        GetAccountInfoRequest getAccountInfo = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(getAccountInfo,
            resultCallback =>
            {
                failedCallbackCounter = 0;

                PlayerData.Username = resultCallback.AccountInfo.Username;
                PlayerData.EmailAddress = LoginEmailTMPInput.text;
                ProfileUsernameTMP.text = "Username: " + PlayerData.Username;
                ProfileEmailTMP.text = "Email Address: " + PlayerData.EmailAddress;
                GetUserDataPlayFab();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                   FailedAction,
                   GetAccountInfoPlayFab,
                   () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void GetUserDataPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            resultCallback =>
            {
                if(resultCallback.Data.ContainsKey("GameHistory"))
                {
                    PlayerData.PlayerHistory = JsonConvert.DeserializeObject<List<PlayerData.GameHistory>>(resultCallback.Data["GameHistory"].Value);

                }
                LoadingPanel.SetActive(false);
                CurrentMainMenuState = MainMenuStates.WELCOME;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                   FailedAction,
                   GetUserDataPlayFab,
                   () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void ResetLoginFields()
    {
        LoginEmailTMPInput.text = "";
        LoginPasswordTMPInput.text = "";
    }
    #endregion

    #region REGISTRATION
    public void RegisterNewUser()
    {
        if(RegisterEmailTMPInput.text == "" || RegisterUsernameTMPInput.text == "" || RegisterPasswordTMPInput.text == "" || RegisterConfirmPasswordTMPInput.text == "")
        {
            DisplayError("Please fill up all fields");
            return;
        }
        if(!RegisterEmailTMPInput.text.Contains('@') || !RegisterEmailTMPInput.text.Contains(".com"))
        {
            DisplayError("Please input a valid email");
            return;
        }
        if(RegisterUsernameTMPInput.text.Length < 6 || RegisterUsernameTMPInput.text.Length > 20)
        {
            DisplayError("Username must only be 6-20 characters");
            return;
        }
        if(RegisterPasswordTMPInput.text != RegisterConfirmPasswordTMPInput.text)
        {
            DisplayError("The passwords do not match");
            return;
        }
        if (RegisterPasswordTMPInput.text.Length < 6)
        {
            DisplayError("Password must be at least six characters long");
            return;
        }

        if (GameManager.Instance.DebugMode)
        {
            HideRegisterPanel();
            CurrentMainMenuState = MainMenuStates.LOGIN;
        }   
        else
        {
            LoadingPanel.SetActive(true);
            RegisterNewUserPlayfab();
        }
    }

    public void RegisterNewUserPlayfab()
    {
        RegisterPlayFabUserRequest registerNewUser = new RegisterPlayFabUserRequest();
        registerNewUser.Email = RegisterEmailTMPInput.text;
        registerNewUser.Username = RegisterUsernameTMPInput.text;
        registerNewUser.DisplayName = RegisterUsernameTMPInput.text;
        registerNewUser.Password = RegisterPasswordTMPInput.text;

        PlayFabClientAPI.RegisterPlayFabUser(registerNewUser,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                LoadingPanel.SetActive(false);
                Debug.Log("successfully registered new user");
                SetUserDataPlayfab();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    FailedAction,
                    RegisterNewUserPlayfab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void SetUserDataPlayfab()
    {
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>
        {
            { "GameHistory", "[]" }
        };

        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback => 
            {
                failedCallbackCounter = 0;
                HideRegisterPanel();
                PlayFabClientAPI.ForgetAllCredentials();
                CurrentMainMenuState = MainMenuStates.LOGIN;
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    FailedAction,
                    SetUserDataPlayfab,
                    () => DisplayError(errorCallback.ErrorMessage));
            });
    }

    private void ResetRegistrationFields()
    {
        RegisterUsernameTMPInput.text = "";
        RegisterEmailTMPInput.text = "";
        RegisterPasswordTMPInput.text = "";
        RegisterConfirmPasswordTMPInput.text = "";
    }
    #endregion

    #region STORY
    private void DisplayCurrentStoryPage()
    {
        StoryImage.sprite = StoryPanels[CurrentStoryPageIndex].StorySprite;
        StoryText.text = StoryPanels[CurrentStoryPageIndex].StoryText;

        if (CurrentStoryPageIndex == 0)
            PreviousPageBtn.interactable = false;
        else
            PreviousPageBtn.interactable = true;
    }

    public void PreviousStoryPage()
    {
        CurrentStoryPageIndex--;
        DisplayCurrentStoryPage();
    }

    public void HandleNextStoryButton()
    {
        if (CurrentStoryPageIndex == StoryPanels.Count - 1)
        {
            HideStoryPanel();
            CurrentMainMenuState = MainMenuStates.GAMESELECT;
        }
        else
        {
            CurrentStoryPageIndex++;
            DisplayCurrentStoryPage();
        }
    }
    #endregion

    #region GAME SELECT
    public void OpenRacecarScene()
    {
        GameManager.Instance.SceneController.CurrentScene = "RacecarScene";
    }
    #endregion

    #region LOG-OUT
    public void LogOut()
    {  
        if(!GameManager.Instance.DebugMode)
            PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        HideGameSelectPanel();
        CurrentMainMenuState = MainMenuStates.LOGIN;
    }
    #endregion

    #region ERROR
    private void DisplayError(string errorMessage)
    {
        ErrorPanel.SetActive(true);
        ErrorTMP.text = errorMessage;
    }

    public void HideErrorPanel()
    {
        ErrorPanel.SetActive(false);
    }

    private void ErrorCallback(PlayFabErrorCode errorCode, Action failedAction, Action restartAction, Action processError)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                failedAction();
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
        {
            if (processError != null)
                processError();
        }
    }

    private void FailedAction()
    {
        LoadingPanel.SetActive(false);
        DisplayError("Connectivity error. Please connect to strong internet");
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        ResetLoginFields();
        ResetRegistrationFields();
    }

    private void ProcessError(string error)
    {
        LoadingPanel.SetActive(false);
        DisplayError(error);
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        ResetLoginFields();
        ResetRegistrationFields(); ;
    }

    private void ProcessSpecialError()
    {
        LoadingPanel.SetActive(false);
        DisplayError("Server Error. Please restart the game");
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        ResetLoginFields();
        ResetRegistrationFields();
    }
    #endregion
}
