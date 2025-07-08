using System.Collections;
using UnityEngine;
using Fusion;

using static BehaviourRegistry;
public class CharacterAttackBehaviour : CharacterRegistry, IReadInput, IRPCListener
{
    [Networked] protected TickTimer AttackCooldown { get; set; }
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [Networked(OnChanged = nameof(OnNetworkBlockChanged))] public NetworkBool IsPlayerBlockingLocal { get; set; }
    [Networked(OnChanged = nameof(OnNetworkBlockPositionChanged))] public SwordPosition PlayerSwordPositionLocal { get; set; }
    [Networked(OnChanged = nameof(OnNetworkDummyBombStateChange))] public NetworkBool IsDummyBombActivated { get; set; }
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
    [SerializeField] protected GameObject _dummyBomb;
    protected CharacterController _characterController;
    public PlayerStatsController _playerStatsController { get; set; }
    protected CharacterHealth _characterHealth;
    protected IDamageable _collidedObject;
    protected bool _isPlayerHoldingBomb { get; set; }
    public CharacterStats.CharacterType _characterType;
    protected CharacterStamina _characterStamina;
    protected CharacterCollision _characterCollision;
    private SwordPosition _lastSwordPosition;
    private float _movementTreshold = 0.25f;
    #region Throwable
    protected float _throwDuration;
    protected float _defaultThrowDuration;
    protected bool _isBombThrown;
    private bool _wasHoldingLastFrame;
    private GameObject _opponent;
    #endregion

    public virtual void ReadPlayerInputs(PlayerInputData input) { }
    protected virtual void AttackCollision() {}
    protected virtual void SwingSword() {}
    protected virtual void BlockWeapon() {}
    protected virtual void ThrowBomb() {}
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
        //if (changed.Behaviour._characterStats.WarriorType == CharacterStats.CharacterType.FootKnight) return;
        changed.Behaviour._blockArea.enabled = changed.Behaviour.IsPlayerBlockingLocal;
    }

    private static void OnNetworkBlockPositionChanged(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour.PlayerSwordPosition = changed.Behaviour.PlayerSwordPositionLocal;
    }

    private static void OnNetworkDummyBombStateChange(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour._dummyBomb.SetActive(changed.Behaviour.IsDummyBombActivated);
    }

    protected void CheckAttackCollision(GameObject collidedObject)
    {
        _opponent = null;
        if (CurrentGamePhase == LevelManager.GamePhase.Preparation) return;
       if (collidedObject.transform.GetComponentInParent<NetworkObject>() == null) return;
       if (collidedObject.transform.GetComponentInParent<NetworkObject>().Id == transform.GetComponentInParent<NetworkObject>().Id) return;
        var opponentTeam = collidedObject.transform.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;
        Debug.Log("OpponentTeam: " + collidedObject.transform.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam);
      
        if (opponentTeam == _playerStatsController.PlayerTeam || CurrentGamePhase == LevelManager.GamePhase.Preparation) return;
        
       
        if (collidedObject.transform.GetComponentInParent<IDamageable>() != null)
        {
            _opponent = collidedObject;
             var opponentType = GetCharacterType(collidedObject);
            switch (opponentType)
            {
                case CharacterStats.CharacterType.FootKnight:
                   DamageToFootknight();
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    DamageToGallowGlass();
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    DamageToKnightCommander();
                    break;
                case CharacterStats.CharacterType.Ranger:
                    DamageToRanger();
                    break;
            }
        }
    }

    protected void DamageToFootknight()
    {
        var opponentHealth = _opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = _opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentParrying = _opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        var isOpponentUseAbility = _opponent.transform.GetComponentInParent<StormShieldSkill>().IsPlayerUseAbilityLocal;

        if (isOpponentUseAbility) return;

        if (_opponent.gameObject.layer == 11 && isOpponentParrying)
        {
            _opponent.transform.GetComponentInParent<StormshieldVFXController>().UpdateParryVFXRpc();
            opponentStamina.DecreaseDefenceStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
            StartCoroutine(VerifyOpponentDeath(opponentHealth));
           
        }
    }
    protected void DamageToKnightCommander()
    {
        var opponentHealth = _opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = _opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = _opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        var isOpponentDash = _opponent.transform.GetComponentInParent<KnightCommanderSkill>().IsPlayerUseAbilityLocal;
        Debug.Log("IsOpponentDash: " + isOpponentDash);
        if (isOpponentDash) return;

        if (_opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
           opponentStamina.DecreaseDefenceStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
           _opponent.transform.GetComponentInParent<IronheartVFXController>().UpdateParryVFXRpc();
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
            StartCoroutine(VerifyOpponentDeath(opponentHealth));
        }
    }

    protected void DamageToGallowGlass()
    {
       
        var opponentHealth = _opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = _opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = _opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        var isOpponentUseAbility = _opponent.transform.GetComponentInParent<BloodhandSkill>().IsPlayerUseAbilityLocal;
        //Debug.Log("Ýs OpponentBlocking: " + isOpponentBlocking + " oppponent block area active?: " + opponent.transform.GetComponentInParent<CharacterAttackBehaviour>()._blockArea.enabled.ToString() + " oppponent sword position " + opponentSwordPosition);
        if (isOpponentUseAbility) return;


        if (_opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            _opponent.transform.GetComponentInParent<BloodhandVFXController>().UpdateParryVFXRpc();
            opponentStamina.DecreaseDefenceStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
            StartCoroutine(VerifyOpponentDeath(opponentHealth));
        }
    }

    protected void DamageToRanger()
    {
        var isOpponentUseAbility = _opponent.transform.GetComponentInParent<TheSaxonMarkSkill>().IsPlayerUseAbilityLocal;
        if (isOpponentUseAbility) return;
        var opponentHealth = _opponent.transform.GetComponentInParent<CharacterHealth>();
        opponentHealth.DealDamageRPC(_weaponStats.Damage, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _playerStatsController.PlayerLocalStats.PlayerWarrior);
        StartCoroutine(VerifyOpponentDeath(opponentHealth));
    }
    private IEnumerator VerifyOpponentDeath(CharacterHealth opponentHealth)
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("OpponentHealth: " + opponentHealth.NetworkedHealth);


        if (opponentHealth.NetworkedHealth <= 0)
        {

            if (CurrentGamePhase != LevelManager.GamePhase.Preparation && CurrentGamePhase != LevelManager.GamePhase.Warmup)
            {
                _playerStatsController.UpdatePlayerKillCountRpc();
            }

            EventLibrary.OnKillFeedReady.Invoke(_playerStatsController.PlayerLocalStats.PlayerWarrior, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            EventLibrary.OnPlayerKillRegistryUpdated.Invoke(_playerStatsController.PlayerLocalStats.PlayerTeam);
        }

        _opponent = null;
    }



    protected bool HandleThrowDuration(bool isHolding)
    {
        if (!Object.HasStateAuthority) return false;

        
        if (isHolding)
        {
            _throwDuration -= Time.deltaTime;
            _wasHoldingLastFrame = true;
            return false;
        }

        
        if (_wasHoldingLastFrame)
        {
            _wasHoldingLastFrame = false;
            bool ready = _throwDuration <= 0f;
            _throwDuration = _defaultThrowDuration;
            return ready;
        }

        return false;
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


    #region Legacy
    /*
     * if (IsOpponentDead(opponentHealth.NetworkedHealth))
        {
            Debug.Log("ADAM ÖLDÜ Ranger");
            if (CurrentGamePhase != LevelManager.GamePhase.Preparation && CurrentGamePhase != LevelManager.GamePhase.Warmup)
            {
                _playerStatsController.UpdatePlayerKillCountRpc();
            }

            EventLibrary.OnPlayerKill.Invoke(_playerStatsController.PlayerLocalStats.PlayerWarrior, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), _opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            EventLibrary.OnPlayerKillRegistryUpdated.Invoke(_playerStatsController.PlayerLocalStats.PlayerTeam);
        }
        else
        {
            Debug.Log("OpponentHealth: " + opponentHealth.NetworkedHealth);
        }
     private bool IsOpponentDead(float opponentHealth)
    {
        Debug.Log("Test: " + opponentHealth);
        return (opponentHealth - _weaponStats.Damage) <= 0;
    }
    */
    #endregion
}
