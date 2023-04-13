using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCombatCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum CharacterCombatState
    {
        NONE,
        IDLE,
        ATTACKING,
        ATTACKED,
        DYING
    }

    private event EventHandler characterCombatStateChange;
    public event EventHandler onCharacterCombatStateChange
    {
        add
        {
            if (characterCombatStateChange == null || !characterCombatStateChange.GetInvocationList().Contains(value))
                characterCombatStateChange += value;
        }
        remove
        {
            characterCombatStateChange -= value;
        }
    }
    public CharacterCombatState CurrentCharacterCombatState
    {
        get { return currentCharacterCombatState; }
        set
        {
            currentCharacterCombatState = value;
            characterCombatStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField][ReadOnly] private CharacterCombatState currentCharacterCombatState;
    #endregion

    #region VARIABLES
    //==========================================================================================================
    public enum CharacterType { NONE, PLAYER, ENEMY }
    public enum AttackType { NONE, MELEE, RANGED }
    public enum TravelState { NONE, APPROACH, RETURN }
    [SerializeField] private CombatCore CombatCore;

    [Header("CHARACTER")]
    public CharacterType thisCharacterType;
    public AttackType thisAttackType;
    public Animator anim;
    [SerializeField] private Vector3 CharacterOriginPoint;
    [SerializeField] private Vector3 CharacterAttackPoint;

    [Header("HEALTH")]
    [SerializeField] private int MaxHealth;
    [SerializeField] private Slider HealthSlider;
    [SerializeField][ReadOnly] private int CurrentHealth;

    [Header("MELEE ONLY: TRAVEL")]
    [ReadOnly] public TravelState CurrentTravelState;

    [Header("RANGED ONLY: PROJECTILE")]
    [SerializeField] private GameObject Projectile;
    [SerializeField] private Vector3 ProjectileOriginPoint;
    [SerializeField] private float ProjectileSpeed;
    [ReadOnly] public bool ProjectileLaunched;
    //==========================================================================================================
    #endregion

    #region INITIALIZATION
    public void InitializeCharacter()
    {
        CurrentCharacterCombatState = CharacterCombatState.IDLE;

        #region HEALTH
        CurrentHealth = MaxHealth;
        HealthSlider.value = (float) CurrentHealth / (float) MaxHealth;
        #endregion

        #region PROJECTILE
        if(thisAttackType == AttackType.RANGED)
        {
            Projectile.transform.position = ProjectileOriginPoint;
            Projectile.SetActive(false);
        }
        
        #endregion  
    }
    #endregion

    #region HEALTH
    public void TakeDamage()
    {
        CurrentHealth--;
        HealthSlider.value = (float)CurrentHealth / (float)MaxHealth;

        if (CurrentHealth <= 0)
        {
            CurrentCharacterCombatState = CharacterCombatState.DYING;
            if ((thisCharacterType == CharacterType.PLAYER && CombatCore.EnemyCharacter.thisAttackType == AttackType.RANGED) ||
               (thisCharacterType == CharacterType.ENEMY && CombatCore.PlayerCharacter.thisAttackType == AttackType.RANGED))
            {
                if (thisCharacterType == CharacterType.PLAYER)
                    CombatCore.FinalResult = GameManager.Result.DEFEAT;
                else if (thisCharacterType == CharacterType.ENEMY)
                    CombatCore.FinalResult = GameManager.Result.VICTORY;
                CombatCore.CurrentCombatState = CombatCore.CombatStates.GAMEOVER;
            }
        }
        else
        {
            CurrentCharacterCombatState = CharacterCombatState.IDLE;
            if((thisCharacterType == CharacterType.PLAYER && CombatCore.EnemyCharacter.thisAttackType == AttackType.RANGED) || 
               (thisCharacterType == CharacterType.ENEMY && CombatCore.PlayerCharacter.thisAttackType == AttackType.RANGED))
                CombatCore.CurrentCombatState = CombatCore.CombatStates.TIMER;
        }
    }

    public int GetDamageDealt()
    {
        return MaxHealth - CurrentHealth;
    }
    #endregion

    #region TRAVEL
    public void ApproachOpponent()
    {
        if (Vector2.Distance(transform.position, CharacterAttackPoint) > Mathf.Epsilon)
            transform.position = Vector2.MoveTowards(transform.position, CharacterAttackPoint, 5 * Time.deltaTime);
        else
        {
            CurrentTravelState = TravelState.NONE;
            CurrentCharacterCombatState = CharacterCombatState.ATTACKING;
        }
    }

    public void ReturnToOrigin()
    {
        if (Vector2.Distance(transform.position, CharacterOriginPoint) > Mathf.Epsilon)
            transform.position = Vector2.MoveTowards(transform.position, CharacterOriginPoint, 5 * Time.deltaTime);
        else
        {
            CurrentTravelState = TravelState.NONE;
            if (CombatCore.PlayerCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING || CombatCore.EnemyCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING)
            {
                if (CombatCore.PlayerCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING)
                    CombatCore.FinalResult = GameManager.Result.DEFEAT;
                else if (CombatCore.EnemyCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING)
                    CombatCore.FinalResult = GameManager.Result.VICTORY;
                CombatCore.CurrentCombatState = CombatCore.CombatStates.GAMEOVER;
            }
            else
                CombatCore.CurrentCombatState = CombatCore.CombatStates.TIMER;
        }
    }

    public void FinishMeleeAttack()
    {
        CurrentCharacterCombatState = CharacterCombatState.IDLE;
        CurrentTravelState = TravelState.RETURN;
    }
    #endregion

    #region PROJECTILE
    public void ActivateProjectile()
    {
        ProjectileLaunched = false;
        Projectile.SetActive(true);
    }

    public void LaunchProjectile()
    {
        ProjectileLaunched = true;
    }

    public void FlyProjectile()
    {
        if(Vector2.Distance(Projectile.transform.position, CharacterAttackPoint) > Mathf.Epsilon)
            Projectile.transform.position = Vector2.MoveTowards(Projectile.transform.position, CharacterAttackPoint, ProjectileSpeed * Time.deltaTime);
        
        else
        {
            AttackOpponent();
            ProjectileLaunched = false;
            Projectile.SetActive(false);
            Projectile.transform.position = ProjectileOriginPoint;
        }
    }
    #endregion

    #region UTILITY
    public void AttackOpponent()
    {
        if (thisCharacterType == CharacterType.PLAYER)
            CombatCore.EnemyCharacter.CurrentCharacterCombatState = CharacterCombatState.ATTACKED;
        else if (thisCharacterType == CharacterType.ENEMY)
            CombatCore.PlayerCharacter.CurrentCharacterCombatState = CharacterCombatState.ATTACKED;
    }
    #endregion
}
