using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;

public class KnightCommanderSkill : CharacterRegistry, IReadInput
{
    public int SlideCharges { get; set; }
    private CharacterStamina _characterStamina;
    private PlayerHUD _playerHUD;
    private int _slideChargeCount = 3;
    private CharacterController _characterController;
    public NetworkButtons PreviousButton { get; set; }
    private float _duration = 0.2f;
    private float _distance = 0.35f;
    private bool _isSliding = false;
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
        _characterStamina = GetScript<CharacterStamina>();
        _playerHUD = GetScript<PlayerHUD>();
        _slideChargeCount = MAX_SLIDE_CHARGE_COUNT;
        _playerHUD.UpdateSlideChargeCount(_slideChargeCount);
    }


    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
            if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
            {
               ReadPlayerInputs(input);
            }
        
    }


    private void ActivateUtilitySkill()
    {
        if(_slideChargeCount > 0)
        {
            _slideChargeCount -= 1;
            _playerHUD.UpdateSlideChargeCount(_slideChargeCount);
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
        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);



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
        if (_isSliding) return;
        
        if (_slideChargeCount < 0) return;
        _isSliding = true;
        EventLibrary.OnPlayerDash.Invoke(_isSliding);
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
        _isSliding = false;
        EventLibrary.OnPlayerDash.Invoke(_isSliding);
    }


}
