using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

using static BehaviourRegistry;
public class CharacterAttackBehaviour : CharacterRegistry, IReadInput, IRPCListener
{
    [Networked] protected TickTimer AttackCooldown { get; set; }
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [Networked(OnChanged = nameof(OnNetworkBlockChanged))] public NetworkBool IsPlayerBlockingLocal { get; set; }
    [Networked(OnChanged = nameof(OnNetworkBlockPositionChanged))] public SwordPosition PlayerSwordPositionLocal { get; set; }
    [Networked] public NetworkBool IsPlayerBlocking { get; set; }
    [Networked] public SwordPosition PlayerSwordPosition { get; set; }
    [Networked] public LevelManager.GamePhase CurrentGamePhase { get; set; }

    public enum SwordPosition
    {
        None,
        Right,
        Left
    }
    public enum AttackDirection
    {
        None,
        Forward,
        FromRight,
        FromLeft,
        Backward
    }
    
    [SerializeField] protected WeaponStats _weaponStats;
    [SerializeField] protected BoxCollider _blockArea;
    protected CharacterController _characterController;
    protected PlayerStatsController _playerStatsController;
    protected IDamageable _collidedObject;
    public CharacterStats.CharacterType _characterType;
    protected CharacterStamina _characterStamina;
    private SwordPosition _lastSwordPosition;
    private float _movementTreshold = 0.25f;

  
    public virtual void ReadPlayerInputs(PlayerInputData input) { }
    protected virtual void AttackCollision() { }
    protected virtual void SwingSword() { }
    protected virtual void BlockWeapon() { }
    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameStateRpc);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameStateRpc);
    }
    private static void OnNetworkBlockChanged(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour.IsPlayerBlocking = changed.Behaviour.IsPlayerBlockingLocal;
        if (changed.Behaviour._characterStats.WarriorType == CharacterStats.CharacterType.FootKnight) return;
        changed.Behaviour._blockArea.enabled = changed.Behaviour.IsPlayerBlockingLocal;
     
    }

    private static void OnNetworkBlockPositionChanged(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour.PlayerSwordPosition = changed.Behaviour.PlayerSwordPositionLocal;
    }

    protected void CheckAttackCollision(GameObject collidedObject)
    {
       if (CurrentGamePhase == LevelManager.GamePhase.Preparation) return;
       if (collidedObject.transform.GetComponentInParent<NetworkObject>() == null) return;
       if (collidedObject.transform.GetComponentInParent<NetworkObject>().Id == transform.GetComponentInParent<NetworkObject>().Id) return;
        var opponentTeam = collidedObject.transform.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;
        if (opponentTeam == _playerStatsController.PlayerNetworkStats.PlayerTeam && CurrentGamePhase == LevelManager.GamePhase.Preparation) return;
        if (collidedObject.transform.GetComponentInParent<IDamageable>() != null)
        {
            var opponentType = GetCharacterType(collidedObject);
            switch (opponentType)
            {
                case CharacterStats.CharacterType.FootKnight:
                    DamageToFootknight(collidedObject);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    DamageToGallowGlass(collidedObject);
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    DamageToKnightCommander(collidedObject);
                    break;
                case CharacterStats.CharacterType.Ranger:
                    var opponentHealth = collidedObject.transform.GetComponentInParent<CharacterHealth>();
                    opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
                    break;
            }
       }
    }

    protected void DamageToFootknight(GameObject opponent)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentParrying = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        if (opponent.gameObject.layer == 11 && !isOpponentParrying)
        {
            return;
        }

        if (opponent.gameObject.layer == 11 && isOpponentParrying)
        {
            opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
            if (IsOpponentDead(opponentHealth.NetworkedHealth))
            {
              
                if (CurrentGamePhase != LevelManager.GamePhase.Preparation && CurrentGamePhase != LevelManager.GamePhase.Warmup)
                {
                   _playerStatsController.UpdatePlayerKillCountRpc();
                }
               
                EventLibrary.OnPlayerKill.Invoke(_playerStatsController.PlayerLocalStats.PlayerWarrior, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());

            }
        }
    }

    protected void DamageToKnightCommander(GameObject opponent)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        //var isOpponentDead = (opponentHealth.NetworkedHealth - _weaponStats.Damage) <= 0;
        //Debug.Log("Ýs OpponentBlocking: " + isOpponentBlocking + " oppponent block area active?: " + opponent.transform.GetComponentInParent<CharacterAttackBehaviour>()._blockArea.enabled.ToString() + " oppponent sword position " + opponentSwordPosition);


        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
           opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
            if (IsOpponentDead(opponentHealth.NetworkedHealth))
            {
                if (CurrentGamePhase != LevelManager.GamePhase.Preparation && CurrentGamePhase != LevelManager.GamePhase.Warmup)
                {
                    if (!Object.HasStateAuthority) return; // 
                    //Debug.LogError("CurrentGamepHase: " +CurrentGamePhase);
                    _playerStatsController.UpdatePlayerKillCountRpc();
                }
                if (!Object.HasStateAuthority) return;
                EventLibrary.OnPlayerKill.Invoke(_playerStatsController.PlayerLocalStats.PlayerWarrior, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
                EventLibrary.OnPlayerKillRegistryUpdated.Invoke(_playerStatsController.PlayerLocalStats.PlayerTeam);
            }
        }
    }

    protected void DamageToGallowGlass(GameObject opponent)
    {
        /*
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var opponentBehaviour = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>();
        var isOpponentBlocking = opponentBehaviour != null && opponentBehaviour.IsPlayerBlocking;
        var opponentSwordPosition = opponentBehaviour.PlayerSwordPosition;

        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            Vector3 attackDirection = (opponent.transform.position - transform.position).normalized;
            float sideCheck = Vector3.Dot(opponent.transform.right, attackDirection);
            float distance = Vector3.Distance(transform.position, opponent.transform.position);

            // Mesafe yakýn ve blok uyumluysa parry
            if (distance < 0.5f &&
               (opponentSwordPosition == SwordPosition.Right && sideCheck > 0f ||
                opponentSwordPosition == SwordPosition.Left && sideCheck < 0f))
            {
                opponentStamina?.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
            }
            else
            {
                opponentHealth?.DealDamageRPC(_weaponStats.Damage);
            }
        }
        else
        {
            opponentHealth?.DealDamageRPC(_weaponStats.Damage);
        }
        */
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
       
        //Debug.Log("Ýs OpponentBlocking: " + isOpponentBlocking + " oppponent block area active?: " + opponent.transform.GetComponentInParent<CharacterAttackBehaviour>()._blockArea.enabled.ToString() + " oppponent sword position " + opponentSwordPosition);


        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
            if (IsOpponentDead(opponentHealth.NetworkedHealth))
            {
                if(CurrentGamePhase != LevelManager.GamePhase.Preparation && CurrentGamePhase != LevelManager.GamePhase.Warmup)
                {
                    Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAA");
                    _playerStatsController.UpdatePlayerKillCountRpc();
                }
               
                EventLibrary.OnPlayerKill.Invoke(_playerStatsController.PlayerLocalStats.PlayerWarrior, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            }
        }
    }

    private bool IsOpponentDead(float opponentHealth)
    {
        return (opponentHealth - _weaponStats.Damage) <= 0;
    }

    protected SwordPosition GetSwordPosition() 
    {
       float mouseX = Input.GetAxis("Mouse X");
       _lastSwordPosition = mouseX > _movementTreshold ? SwordPosition.Right : mouseX < -_movementTreshold ? SwordPosition.Left :_lastSwordPosition;
        return _lastSwordPosition;
    }
  
    protected float CalculateAttackPosition(Transform defenderPosition)
    {
        Vector3 directionToTarget = Vector3.Normalize(transform.position - defenderPosition.transform.position);
        var value = Vector3.Dot(defenderPosition.transform.forward, directionToTarget);
        return value;
    }


    protected CharacterStats.CharacterType GetCharacterType(GameObject collidedObject)
    {   
        if (collidedObject.transform.GetComponentInParent<PlayerStatsController>() != null)
        {
            var character = collidedObject.transform.GetComponentInParent<PlayerStatsController>();
            return character.PlayerNetworkStats.PlayerWarrior;
        }
        else
        {
           return CharacterStats.CharacterType.None;
        }
    }

    protected AttackDirection CalculateAttackDirection(Transform defenderPosition)
    {
        Vector3 directionToTarget = Vector3.Normalize(transform.position - defenderPosition.position);
        float forwardDot = Vector3.Dot(defenderPosition.forward, directionToTarget);
        Vector3 crossProduct = Vector3.Cross(defenderPosition.forward, directionToTarget);
        float forwardThreshold = 0.7f;
        float backwardThreshold = -0.7f;
        float sideThreshold = 0.3f;
         return (forwardDot > forwardThreshold) ? AttackDirection.Forward :
       (forwardDot < backwardThreshold) ? AttackDirection.Backward :
       (crossProduct.y > sideThreshold) ? AttackDirection.FromLeft :
       (crossProduct.y < -sideThreshold) ? AttackDirection.FromRight :
       AttackDirection.None;
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
        //Debug.LogError("CurrentGamePhaseUpdated: " + CurrentGamePhase);
    }
}
