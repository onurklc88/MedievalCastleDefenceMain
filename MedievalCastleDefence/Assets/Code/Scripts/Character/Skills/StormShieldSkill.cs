using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;
using System.Threading;

public class StormShieldSkill : CharacterRegistry, IReadInput
{
    private PlayerStatsController _playerStatsController;
    private CharacterMovement _characterMovement;
    private CharacterHealth _characterHealth;
    private PlayerVFXSytem _playerVFX;
    private int _skillChargeCount;
    private FootknightAnimation _footnightAnimation;
    public NetworkButtons PreviousButton { get; set; }
    private bool canUseAbility = true;
    private CancellationTokenSource cts;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        
        InitScript(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }

    }
    public void ReadPlayerInputs(PlayerInputData input)
    {
        
        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (pressedButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && _characterMovement.IsPlayerGrounded() && canUseAbility)
        {
            canUseAbility = false;
            UseAbility();
            StartCooldown().Forget();
        }

        PreviousButton = input.NetworkButtons;
    }
    private async UniTaskVoid StartCooldown()
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        await UniTask.Delay(90000, cancellationToken: cts.Token); 
        canUseAbility = true;
    }
    private void Start()
    {

        //_playerHUD = GetScript<PlayerHUD>();

        //_skillChargeCount = 1;
        _playerStatsController = GetScript<PlayerStatsController>();
        _characterMovement = GetScript<CharacterMovement>();
        _characterHealth = GetScript<CharacterHealth>();
        _playerVFX = GetScript<PlayerVFXSytem>();
        _footnightAnimation = GetScript<FootknightAnimation>();

    }


    private async void UseAbility()
    {
       
        _characterMovement.IsInputDisabled = true;
        _footnightAnimation.UpdateAbilityAnimationState();
       
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f);
        HashSet<NetworkBehaviourId> healedCharacters = new HashSet<NetworkBehaviourId>();
        for (int i = 0; i < hitColliders.Length; i++)
        {
            var characterStatsController = hitColliders[i].transform.GetComponentInParent<PlayerStatsController>();
            if (hitColliders[i].GetComponentInParent<IDamageable>() != null && characterStatsController != null)
            {
                var playerID = hitColliders[i].transform.GetComponentInParent<PlayerStatsController>().Id;
               //Debug.Log("Name: " + _hitColliders[i].transform.GetComponentInParent<PlayerStatsController>().ýd);
               // Debug.Log("charStats: " + characterStatsController.PlayerTeam + " HealerTeam: " + _playerStatsController.PlayerTeam);
                if (characterStatsController.PlayerTeam == _playerStatsController.PlayerTeam && !healedCharacters.Contains(playerID))
                {
                    Debug.Log("HealingRPC called");
                    hitColliders[i].transform.GetComponentInParent<CharacterHealth>().ApplyHealingRpc(40f);
                    if(_playerStatsController.Id != playerID)
                    {
                      hitColliders[i].transform.GetComponentInParent<PlayerVFXSytem>().PlayHealingRpc();
                    }
                        
                    healedCharacters.Add(playerID);
                }

            }
            else
            {
                Debug.Log("NothingFound");
            }
        }
        await UniTask.Delay(300);
        _playerVFX.PlayUltimateVFX();
        await UniTask.Delay(1450);
        
        
        _characterMovement.IsInputDisabled = false;

    }
    private void OnDestroy()
    {
        cts?.Cancel();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 5f);
      

    }
}
