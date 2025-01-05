using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterHealth : BehaviourRegistry, IDamageable
{
    
    private PlayerHUD _playerHUD;
   [Networked] public float NetworkedHealth { get; set; }
    private CharacterAnimationController _characterAnim;
    private CharacterMovement _characterMovement;
    private ActiveRagdoll _activeRagdoll;
    private PlayerVFXSytem _playerVFX;
    private CharacterDecals _characterDecals;
    private PlayerStatsController _playerStatsController;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        NetworkedHealth = _characterStats.TotalHealth;
    }
    private void Start()
    {
        _playerHUD = GetScript<PlayerHUD>();
        if(_playerHUD != null)
            _playerHUD.UpdatePlayerHealthUI(NetworkedHealth);
        _activeRagdoll = GetScript<ActiveRagdoll>();
        _playerVFX = GetScript<PlayerVFXSytem>();
        _characterDecals = GetScript<CharacterDecals>();
        _playerStatsController = GetScript<PlayerStatsController>();
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
        Debug.Log("Given Damage: " + givenDamage + " DamageDealer: " + opponentName + " OpponentWarrior: " + opponentWarrior);
        NetworkedHealth -= givenDamage;
        _characterAnim.UpdateDamageAnimationState();
        _playerVFX.PlayBloodVFX();
        if (NetworkedHealth <= 0)
        {
            _playerHUD.UpdatePlayerHealthUI(-1);
            _characterMovement.IsInputDisabled = true;
            _playerHUD.ShowRespawnPanel();
            _activeRagdoll.RPCActivateRagdoll();
            _playerStatsController.UpdatePlayerDieCount();
        }
        else
        {
            _characterDecals.EnableRandomBloodDecal();
            _playerHUD.UpdatePlayerHealthUI(NetworkedHealth);
        }
    }
   
    public void DestroyObject()
    {
      
    }

    private void KillPlayer()
    {
        if (!Object.HasStateAuthority) return;


    }
   


}
