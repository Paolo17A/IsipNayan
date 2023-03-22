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
        SceneController.CurrentScene = "MainMenuScene";
    }
    #endregion
}
