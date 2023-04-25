using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private CombatCore CombatCore;
    private void Awake()
    {
        CombatCore.onCombatStateChange += CombatStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        CombatCore.onCombatStateChange -= CombatStateChange;
    }

    private void Start()
    {
        CombatCore.CurrentCombatState = CombatCore.CombatStates.COUNTDOWN;
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        switch (CombatCore.CurrentCombatState)
        {
            case CombatCore.CombatStates.COUNTDOWN:
                CombatCore.InitializeQuizGame();
                break;
            case CombatCore.CombatStates.TIMER:
                CombatCore.ResetTimer();
                break;
            case CombatCore.CombatStates.PLAYERTURN:
                if (CombatCore.PlayerCharacter.thisAttackType == CharacterCombatCore.AttackType.MELEE)
                    CombatCore.PlayerCharacter.CurrentTravelState = CharacterCombatCore.TravelState.APPROACH;
                else if (CombatCore.PlayerCharacter.thisAttackType == CharacterCombatCore.AttackType.RANGED)
                    CombatCore.PlayerCharacter.CurrentCharacterCombatState = CharacterCombatCore.CharacterCombatState.ATTACKING;
                break;
            case CombatCore.CombatStates.ENEMYTURN:
                if (CombatCore.EnemyCharacter.thisAttackType == CharacterCombatCore.AttackType.MELEE)
                    CombatCore.EnemyCharacter.CurrentTravelState = CharacterCombatCore.TravelState.APPROACH;
                else if (CombatCore.EnemyCharacter.thisAttackType == CharacterCombatCore.AttackType.RANGED)
                    CombatCore.EnemyCharacter.CurrentCharacterCombatState = CharacterCombatCore.CharacterCombatState.ATTACKING;
                break;
            case CombatCore.CombatStates.GAMEOVER:
                if (CombatCore.FinalResult == GameManager.Result.VICTORY)
                    CombatCore.ProcessVictory();
                else if (CombatCore.FinalResult == GameManager.Result.DEFEAT)
                    CombatCore.ProcessDefeat();
                break;
        }
    }

    private void Update()
    {
        if (CombatCore.CurrentCombatState == CombatCore.CombatStates.COUNTDOWN)
            CombatCore.ReduceCountdownTimer();
        /*else if (CombatCore.CurrentCombatState == CombatCore.CombatStates.TIMER)
            CombatCore.DecreaseTimer();*/
    }
}
