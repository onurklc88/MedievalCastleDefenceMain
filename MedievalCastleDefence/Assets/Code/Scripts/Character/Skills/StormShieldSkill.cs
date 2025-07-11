using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;
using System.Threading;

public class StormShieldSkill : CharacterRegistry, IReadInput, IAbility
{
    [Networked(OnChanged = nameof(OnNetworkAbilityStateChange))] public NetworkBool IsAbilityInUseLocal { get; set; }
    [Networked] public NetworkBool IsAbilityInUse { get; set; }

    private PlayerStatsController _playerStatsController;
    private CharacterMovement _characterMovement;
    private CharacterHealth _characterHealth;
    private StormshieldVFXController _playerVFX;
    private int _skillChargeCount;
    private FootknightAnimation _footnightAnimation;
    private CharacterCollision _characterCollision;
    public NetworkButtons PreviousButton { get; set; }
   
    private bool canUseAbility = true;
    private CancellationTokenSource cts;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        
        InitScript(this);
    }
    private static void OnNetworkAbilityStateChange(Changed<StormShieldSkill> changed)
    {
        changed.Behaviour.IsAbilityInUse = changed.Behaviour.IsAbilityInUseLocal;
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }

    }

    private void Update()
    {
        Debug.Log("AbilityInUsed: " + IsAbilityInUseLocal);
    }
    public void ReadPlayerInputs(PlayerInputData input)
    {
        
        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (pressedButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && _characterCollision.IsPlayerGrounded && canUseAbility)
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
        _playerVFX = GetScript<StormshieldVFXController>();
        _footnightAnimation = GetScript<FootknightAnimation>();
        _characterCollision = GetScript<CharacterCollision>();

    }


    private async void UseAbility()
    {
       
        _characterMovement.IsInputDisabled = true;
        _footnightAnimation.UpdateAbilityAnimationState();
        IsAbilityInUseLocal = true;
       
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
        IsAbilityInUseLocal = false;
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
