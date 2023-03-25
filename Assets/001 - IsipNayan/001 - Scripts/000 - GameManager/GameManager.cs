using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The GameManager is the central core of the game. It persists all throughout run-time 
 * and stores universal game objects and variables that need to be used in multiple scenes. */
public class GameManager : MonoBehaviour
{
    #region VARIABLES
    //===========================================================
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                    _instance = new GameObject().AddComponent<GameManager>();
            }

            return _instance;
        }
    }

    public enum Result { NONE, VICTORY, DEFEAT};
    [field: SerializeField] public List<GameObject> GameMangerObj { get; set; }

    [field: SerializeField] public bool DebugMode { get; set; }
    [SerializeField] private string SceneToLoad;
    [field: SerializeField][field: ReadOnly] public bool CanUseButtons { get; set; }

    [field: Header("CAMERA")]
    [field: SerializeField] public Camera MainCamera { get; set; }
    [field: SerializeField] public Camera MyUICamera { get; set; }

    [field: Header("MISCELLANEOUS SCRIPTS")]
    [field: SerializeField] public SceneController SceneController { get; set; }
    [field: SerializeField] public AnimationsLT AnimationsLT { get; set; }
    //===========================================================
    #endregion

    #region CONTROLLER FUNCTIONS
    private void Awake()
    {
        if (_instance != null)
        {
            for (int a = 0; a < GameMangerObj.Count; a++)
                Destroy(GameMangerObj[a]);
        }

        for (int a = 0; a < GameMangerObj.Count; a++)
            DontDestroyOnLoad(GameMangerObj[a]);
    }

    private void Start()
    {
        if (DebugMode)
            SceneController.CurrentScene = SceneToLoad;
        else
        {
            SceneController.CurrentScene = "MainMenuScene";

        }
    }
    #endregion

    #region UTILITY
    public string DeserializeStringValue(string value, string key)
    {
        Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

        return result[key];
    }

    public int DeserializeIntValue(string value, string key)
    {
        Dictionary<string, int> result = JsonConvert.DeserializeObject<Dictionary<string, int>>(value);

        return result[key];
    }

    public string SerializeIntValue(List<string> keyValues, List<int> values)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();

        for (int a = 0; a < keyValues.Count; a++)
            dict.Add(keyValues[a], values[a]);

        return JsonConvert.SerializeObject(dict);
    }
    #endregion
}
