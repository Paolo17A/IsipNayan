using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattleController : MonoBehaviour
{
    [SerializeField] private CharacterCombatCore CharacterCombatCore;
    [SerializeField] private CombatCore CombatCore;

    private void Awake()
    {
        CharacterCombatCore.onCharacterCombatStateChange += CharacterCombatStateChange;
    }

    private void OnDisable()
    {
        CharacterCombatCore.onCharacterCombatStateChange -= CharacterCombatStateChange;
    }

    private void CharacterCombatStateChange(object sender, EventArgs e)
    {
        CharacterCombatCore.anim.SetInteger("index", (int)CharacterCombatCore.CurrentCharacterCombatState);
    }

    private void Update()
    {
        //  Handling Player movement
        if(CharacterCombatCore.thisCharacterType == CharacterCombatCore.CharacterType.PLAYER && CombatCore.CurrentCombatState == CombatCore.CombatStates.PLAYERTURN)
        {
            if(CharacterCombatCore.thisAttackType == CharacterCombatCore.AttackType.RANGED && CharacterCombatCore.ProjectileLaunched)
            {
                CharacterCombatCore.FlyProjectile();
            }
            else if (CharacterCombatCore.thisAttackType == CharacterCombatCore.AttackType.MELEE)
            {
                if (CharacterCombatCore.CurrentTravelState == CharacterCombatCore.TravelState.APPROACH)
                    CharacterCombatCore.ApproachOpponent();
                else if (CharacterCombatCore.CurrentTravelState == CharacterCombatCore.TravelState.RETURN)
                    CharacterCombatCore.ReturnToOrigin();
            }
        }

        //  Handling enemy movement
        else if (CharacterCombatCore.thisCharacterType == CharacterCombatCore.CharacterType.ENEMY && CombatCore.CurrentCombatState == CombatCore.CombatStates.ENEMYTURN)
        {
            if (CharacterCombatCore.thisAttackType == CharacterCombatCore.AttackType.RANGED && CharacterCombatCore.ProjectileLaunched)
            {
                CharacterCombatCore.FlyProjectile();
            }
            else if (CharacterCombatCore.thisAttackType == CharacterCombatCore.AttackType.MELEE)
            {
                if (CharacterCombatCore.CurrentTravelState == CharacterCombatCore.TravelState.APPROACH)
                    CharacterCombatCore.ApproachOpponent();
                else if (CharacterCombatCore.CurrentTravelState == CharacterCombatCore.TravelState.RETURN)
                    CharacterCombatCore.ReturnToOrigin();
            }
        }
    }
}
