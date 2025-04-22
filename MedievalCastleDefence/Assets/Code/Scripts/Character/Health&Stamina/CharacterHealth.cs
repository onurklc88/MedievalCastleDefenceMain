using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
using Cysharp.Threading.Tasks;
public class CharacterHealth : CharacterRegistry, IDamageable, IRPCListener
{
    public NetworkBool IsPlayerDead { get; set; }
    public bool IsPlayerGotHit { get; private set; }
    private PlayerHUD _playerHUD;
   [Networked] public float NetworkedHealth { get; set; }
   [Networked]  public LevelManager.GamePhase CurrentGamePhase { get; set; }

    private CharacterAnimationController _characterAnim;
    private CharacterMovement _characterMovement;
    private ActiveRagdoll _activeRagdoll;
    private PlayerVFXSytem _playerVFX;
    private BloodDecals _characterDecals;
    private PlayerStatsController _playerStatsController;
    private CharacterCameraController _characterCameraController;

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
        NetworkedHealth = _characterStats.TotalHealth;
        
    }
    private void Start()
    {
       if (!Object.HasStateAuthority) return;
        _playerHUD = GetScript<PlayerHUD>();
        if(_playerHUD != null)
            _playerHUD.UpdatePlayerHealthUI(NetworkedHealth);
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
       
        NetworkedHealth -= givenDamage;
        IsPlayerGotHit = true;
        _characterAnim.UpdateDamageAnimationState();
        _playerVFX.PlayBloodVFX();
        EventLibrary.OnPlayerTakeDamage.Invoke();
        ResetHitStatus().Forget();
        if (NetworkedHealth <= 0)
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
            _playerHUD.UpdatePlayerHealthUI(NetworkedHealth);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ApplyHealingRpc(float healingValue)
    {
        //Debug.Log("HealingValue: " + healingValue + " TotalHealth: " + _characterStats.TotalHealth);
        if(NetworkedHealth < _characterStats.TotalHealth)
        {
            NetworkedHealth = Mathf.Min(NetworkedHealth + healingValue, _characterStats.TotalHealth);
            _playerHUD.UpdatePlayerHealthUI(NetworkedHealth);
        }
       
    }
    private async UniTaskVoid ResetHitStatus()
    {
        await UniTask.Delay(500);
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
