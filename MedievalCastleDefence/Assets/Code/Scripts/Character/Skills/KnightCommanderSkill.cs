using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;

public class KnightCommanderSkill : CharacterRegistry, IReadInput
{
    [Networked(OnChanged = nameof(OnNetworkDashStateChange))] public NetworkBool IsPlayerDash { get; set; }
    public int SlideCharges { get; set; }
    private CharacterStamina _characterStamina;
    private PlayerHUD _playerHUD;
    private PlayerVFXSytem _characterVFX;
    private int _slideChargeCount;
    private CharacterController _characterController;
    private CharacterMovement _characterMovement;
    public NetworkButtons PreviousButton { get; set; }
    private float _duration = 0.2f;
    private float _distance = 0.35f;
  
    private Vector3 _slideDirection;
    private const int MAX_SLIDE_CHARGE_COUNT = 3;
    private bool _isRefilling = false;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        InitScript(this);
        if (!Runner.IsSharedModeMasterClient)
             _distance = 0.17f;
        
    }

    private void Start()
    {
        _characterMovement = GetScript<CharacterMovement>();
        _characterStamina = GetScript<CharacterStamina>();
        _playerHUD = GetScript<PlayerHUD>();
        _characterVFX = GetScript<PlayerVFXSytem>();
        _slideChargeCount = MAX_SLIDE_CHARGE_COUNT;
        if (_playerHUD != null)
            _playerHUD.UpdateSlideChargeCount(_slideChargeCount);

    }


    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || _characterMovement == null) return;
        
            if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input) && !_characterMovement.IsInputDisabled)
            {
               ReadPlayerInputs(input);
            }
        
    }
    
    private static void OnNetworkDashStateChange(Changed<KnightCommanderSkill> changed)
    {

    }

    private void ActivateUtilitySkill()
    {
        if(_slideChargeCount > 0 && _characterStamina.CurrentAttackStamina > 10)
        {
            _slideChargeCount -= 1;
            _playerHUD.UpdateSlideChargeCount(_slideChargeCount);
            _characterStamina.DecreaseCharacterAttackStamina(10f);
            SlideCharacter(_slideDirection).Forget();

            if (_slideChargeCount < 3 && !_isRefilling)
            {
                RechargeSlideAsync().Forget();
            }
        }
        else
        {
            Debug.Log("No slide charges left! Waiting for recharge...");
        }
    }

  
    public void ReadPlayerInputs(PlayerInputData input)
    {
      
        if (input.ForwardDoubleTab)
            _slideDirection = transform.forward;
        else if (input.RightDoubleTab)
            _slideDirection = transform.right;
        else if (input.LeftDoubleTab)
            _slideDirection = -transform.right;
        else if (input.BacwardsDoubleTab)
            _slideDirection = -transform.forward;
        else
            return;

        ActivateUtilitySkill();
        PreviousButton = input.NetworkButtons;
    }


    private async UniTaskVoid RechargeSlideAsync()
    {
        _isRefilling = true; 

        while (_slideChargeCount < 3) 
        {
            await UniTask.Delay(4000);
            _slideChargeCount += 1; 
            _playerHUD.UpdateSlideChargeCount(_slideChargeCount);
        }

        _isRefilling = false;
    }



    private async UniTaskVoid SlideCharacter(Vector3 direction)
    {
        if (IsPlayerDash) return;
        
        if (_slideChargeCount < 0) return;
        IsPlayerDash = true;
        _characterVFX.ActivateSwordTrail(IsPlayerDash);
        EventLibrary.OnPlayerDash.Invoke(IsPlayerDash);
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction * _distance;

        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            Vector3 movement = Vector3.Lerp(Vector3.zero, targetPos - startPos, elapsedTime / _duration);
            _characterController.Move(movement);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        transform.position = targetPos;
        IsPlayerDash = false;
       
        EventLibrary.OnPlayerDash.Invoke(IsPlayerDash);
        await UniTask.Delay(1000);
        _characterVFX.ActivateSwordTrail(IsPlayerDash);

    }


}
