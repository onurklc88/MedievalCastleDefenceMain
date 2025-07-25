using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
using Cysharp.Threading.Tasks;
public class CharacterHealth : CharacterRegistry, IDamageable, IRPCListener
{
    [Networked] public NetworkBool IsPlayerDead { get; set; }
    [Networked] public float NetworkedHealth { get; set; }
    public bool IsPlayerGotHit { get; private set; }
    private PlayerHUD _playerHUD;
 
   [Networked]  public LevelManager.GamePhase CurrentGamePhase { get; set; }
    
    private CharacterAnimationController _characterAnim;
    private CharacterMovement _characterMovement;
    private ActiveRagdoll _activeRagdoll;
    private PlayerVFXSytem _playerVFX;
    private BloodDecals _characterDecals;
    private PlayerStatsController _playerStatsController;
    private CharacterCameraController _characterCameraController;
    [Networked(OnChanged = nameof(OnPlayerNetworkHealthChange))] public float _localHealth { get; set; }
    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameStateRpc);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameStateRpc);
    }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        _localHealth = _characterStats.TotalHealth;
       // NetworkedHealth = _localHealth;
        
    }
    private void Start()
    {
       if (!Object.HasStateAuthority) return;
        _playerHUD = GetScript<PlayerHUD>();
        if(_playerHUD != null)
            _playerHUD.UpdatePlayerHealthUI(_localHealth);
        _activeRagdoll = GetScript<ActiveRagdoll>();
        _playerVFX = GetScript<PlayerVFXSytem>();
        _characterDecals = GetScript<BloodDecals>();
        _playerStatsController = GetScript<PlayerStatsController>();
        _characterCameraController = GetScript<CharacterCameraController>();
        switch (_characterStats.WarriorType)
        {
            case CharacterStats.CharacterType.FootKnight:
                _characterAnim = GetScript<FootknightAnimation>();
                _characterMovement = GetScript<CharacterMovement>();
                break;
            case CharacterStats.CharacterType.Gallowglass:
                _characterMovement = GetScript<CharacterMovement>();
                _characterAnim = GetScript<GallowglassAnimation>();
                break;
            case CharacterStats.CharacterType.KnightCommander:
                _characterAnim = GetScript<KnightCommanderAnimation>();
                _characterMovement = GetScript<CharacterMovement>();
                break;
            case CharacterStats.CharacterType.Ranger:
                _characterMovement = GetScript<CharacterMovement>();
                _characterAnim = GetBehaviour<RangerAnimation>();
                break;

        }

      
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRPC(float givenDamage, string opponentName, CharacterStats.CharacterType opponentWarrior)
    {
       
        _localHealth -= givenDamage;
        IsPlayerGotHit = true;
        _characterAnim.UpdateDamageAnimationState();
        if(_playerVFX != null)
        {
            _playerVFX.PlayBloodVFX();
        }
      
        EventLibrary.OnPlayerTakeDamage.Invoke();
        ResetHitStatus().Forget();
        if (_localHealth <= 0)
        {
            _playerHUD.UpdatePlayerHealthUI(-1);
            _characterMovement.IsInputDisabled = true;
            _playerHUD.ShowRespawnPanel();
            _activeRagdoll.RPCActivateRagdoll();
            if (!Object.HasStateAuthority) return;
            _playerStatsController.UpdatePlayerDieCountRpc();
            _characterCameraController.FollowTeamPlayerCams();
            IsPlayerDead = true;
        }
        else
        {
            _characterDecals.EnableRandomBloodDecal();
            _playerHUD.UpdatePlayerHealthUI(_localHealth);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ApplyHealingRpc(float healingValue)
    {
        //Debug.Log("HealingValue: " + healingValue + " TotalHealth: " + _characterStats.TotalHealth);
        if(_localHealth < _characterStats.TotalHealth)
        {
            _localHealth = Mathf.Min(NetworkedHealth + healingValue, _characterStats.TotalHealth);
            _playerHUD.UpdatePlayerHealthUI(_localHealth);
        }
       
    }
    private async UniTaskVoid ResetHitStatus()
    {
        await UniTask.Delay(100);
        EventLibrary.OnImpulseRequested.Invoke(0, 0.17f);
        await UniTask.Delay(400);
    
        IsPlayerGotHit = false;
    }
    public void DestroyObject() { }

    public void ResetPlayerHealth()
    {
       NetworkedHealth = _characterStats.TotalHealth;
        if (_playerHUD != null)
        {
            _playerHUD.UpdatePlayerHealthUI(NetworkedHealth);
        }
      
    }
    private static void OnPlayerDead(Changed<CharacterHealth> changed)
    {
        //Debug.Log("IsplayerDead: " + changed.Behaviour.IsPlayerDead);
    }

    private static void OnPlayerNetworkHealthChange(Changed<CharacterHealth> changed)
    {
        changed.Behaviour.NetworkedHealth = changed.Behaviour._localHealth;
        if (changed.Behaviour.NetworkedHealth <= 0)
            changed.Behaviour.IsPlayerDead = true;
    }

    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
    }
}
