using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DriverCore : MonoBehaviour
{
    //=========================================================================================================
    [SerializeField] private RacecarCore RacecarCore;

    [Header("LANE VARIABLES")]
    [SerializeField] private float laneSwitchSpeed;
    [SerializeField][ReadOnly] private int CurrentLane;
    [SerializeField][ReadOnly] public bool IsSwitchingLane;
    [SerializeField][ReadOnly] private Vector3 startingPosition;
    [SerializeField][ReadOnly] private Vector3 targetPosition;

    [Header("DRAG VARIABLES")]
    [SerializeField][ReadOnly] private Vector3 initialTouchPoint;
    [SerializeField][ReadOnly] private Vector3 previousTouchPoint;
    [SerializeField][ReadOnly] private Vector3 nextTouchPoint;
    [SerializeField][ReadOnly] private float draggedDirection;

    [Header("DRIVER VARIABLES")]
    [SerializeField] private int StartingLives;
    [SerializeField] private TextMeshProUGUI RemainingLivesTMP;
    [SerializeField][ReadOnly] private int CurrentLivesLeft;
    [ReadOnly] public int CurrentScore;
    [SerializeField] private TextMeshProUGUI CurrentScoreTMP;
    [SerializeField] private float yPos;
    [SerializeField] private TextMeshProUGUI PopUpResultTMP;

    [Header("DEBUGGER")]
    private Vector3 mousePos;
    private Vector2 mousePos2D;
    private RaycastHit2D hit;
    //=========================================================================================================
    public void ResetDriver()
    {
        CurrentLane = 1;
        transform.position = new Vector3(-0.15f, yPos, transform.position.z);
        CurrentLivesLeft = StartingLives;
        RemainingLivesTMP.text = "Remaining Lives: " + CurrentLivesLeft;
        CurrentScore = 0;
        CurrentScoreTMP.text = "Current Score: " + CurrentScore;
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
        transform.GetChild(Random.Range(0, transform.childCount)).gameObject.SetActive(true);
    }

    public void DetectSwipeInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initialTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, 0));
            previousTouchPoint = initialTouchPoint;

            mousePos = GameManager.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos2D = new Vector2(mousePos.x, mousePos.y);

        }
        else if (Input.GetMouseButtonUp(0))
        {
            SetDriverToProperLane();

            initialTouchPoint = Vector3.zero;
            previousTouchPoint = Vector3.zero;
            nextTouchPoint = Vector3.zero;
            draggedDirection = 0;
        }

        if (Input.GetMouseButton(0))
        {
            nextTouchPoint = GameManager.Instance.MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, 0));
            if (nextTouchPoint.x != previousTouchPoint.x)
                draggedDirection = nextTouchPoint.x - previousTouchPoint.x;
        }
    }

    #region ANSWER HANDLING
    public void ProcessCorrectAnswer()
    {
        PopUpResultTMP.color = Color.yellow;
        PopUpResultTMP.text = "CORRECT ANSWER!";
        LeanTween.moveLocalY(PopUpResultTMP.gameObject, -820, 0.5f).setOnComplete(() => LeanTween.moveLocalY(PopUpResultTMP.gameObject, -1000, 0.5f).setDelay(1));
        CurrentScore++;
        CurrentScoreTMP.text = "Current Score: " + CurrentScore;
        if(CurrentScore == 10)
        {
            RacecarCore.FinalResult = GameManager.Result.VICTORY;
            RacecarCore.CurrentGameplayState = RacecarCore.GameplayStates.GAMEOVER;
        }
    }

    public void ProcessIncorrectAnswer()
    {
        PopUpResultTMP.color = Color.red;
        PopUpResultTMP.text = "WRONG ANSWER!";
        LeanTween.moveLocalY(PopUpResultTMP.gameObject, -820, 0.5f).setOnComplete(() => LeanTween.moveLocalY(PopUpResultTMP.gameObject, -1000, 0.5f).setDelay(1));
        CurrentLivesLeft--;
        RemainingLivesTMP.text = "Remaining Lives: " + CurrentLivesLeft;
        if (CurrentLivesLeft == 0)
        {
            RacecarCore.FinalResult = GameManager.Result.DEFEAT;
            RacecarCore.CurrentGameplayState = RacecarCore.GameplayStates.GAMEOVER;
        }
    }
    #endregion

    #region LANE
    private void SetDriverToProperLane()
    {
        if (CurrentLane == 2 && draggedDirection > 0)
            return;

        if (CurrentLane == 0 && draggedDirection < 0)
            return;

        if (draggedDirection >= 0.3)
            CurrentLane++;
        else if (draggedDirection <= -0.3)
            CurrentLane--;

        switch (CurrentLane)
        {
            case 0:
                targetPosition = new Vector3(-0.9f, yPos, transform.position.z);
                break;
            case 1:
                targetPosition = new Vector3(-0.15f, yPos, transform.position.z);
                break;
            case 2:
                targetPosition = new Vector3(0.6f, yPos, transform.position.z);
                break;
        }

        IsSwitchingLane = true;
    }

    public void SwitchLane()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, laneSwitchSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPosition) < Mathf.Epsilon)
            IsSwitchingLane = false;
    }
    #endregion
}
