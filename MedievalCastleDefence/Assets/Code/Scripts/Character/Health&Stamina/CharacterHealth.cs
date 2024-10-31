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
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRPC(float givenDamage)
    {
        NetworkedHealth -= givenDamage;
        _characterAnim.UpdateDamageAnimationState();
        if (NetworkedHealth <= 0)
        {
            _playerHUD.UpdatePlayerHealthUI(-1);
            _characterMovement.IsInputDisabled = true;
            _playerHUD.ShowRespawnPanel();
        }
        else
        {
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
