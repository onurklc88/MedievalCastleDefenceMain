using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;


public class BloodhandSkill : CharacterRegistry, IReadInput
{
   private CharacterMovement _characterMovement;
   private PlayerVFXSytem _playerVFX;
    [Networked] private TickTimer _kickCooldown { get; set; }
    public NetworkButtons PreviousButton { get; set; }
    private GallowglassAttack _bloodhandAttack;
    public bool CanUseAbility { get; private set; }
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

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        CanUseAbility = true;
        _bloodhandAttack = GetScript<GallowglassAttack>();
        _characterMovement = GetScript<CharacterMovement>();
        _playerVFX = GetScript<PlayerVFXSytem>();
    }

   


    public async void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
        if (_characterMovement != null && _characterMovement.IsInputDisabled)
        {
            //IsPlayerBlocking = false;
            //_gallowGlassAnimation.IsPlayerParry = IsPlayerBlocking;
            return;
        }

        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Jump) && CanUseAbility && input.HorizontalInput == 0 && input.VerticalInput >= 0)
        {
             CanUseAbility = false;
            _characterMovement.IsInputDisabled = true;
           
            _bloodhandAttack.KickAction();
            StartBloodhandCooldown().Forget();
            await UniTask.Delay(600);
            _playerVFX.PlayUltimateVFX();
        }
        PreviousButton = input.NetworkButtons;
    }
    private async UniTaskVoid StartBloodhandCooldown()
    {
        await UniTask.Delay(100);
        CanUseAbility = true;
    }
}
