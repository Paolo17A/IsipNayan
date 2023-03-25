using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverController : MonoBehaviour
{
    [SerializeField] private DriverCore DriverCore;
    [SerializeField] private RacecarCore RacecarCore;

    void Update()
    {
        if(RacecarCore.CurrentGameplayState == RacecarCore.GameplayStates.GAMEPLAY)
        {
            if (DriverCore.IsSwitchingLane)
                DriverCore.SwitchLane();
            else
                DriverCore.DetectSwipeInput();
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Gas")
        {
            collision.transform.GetComponent<GasAnswerHandler>().ProcessAnswer();
        }
    }
}
