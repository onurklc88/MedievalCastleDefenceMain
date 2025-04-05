using Cysharp.Threading.Tasks;
using UnityEngine;
using Fusion;
using System;
using static BehaviourRegistry;

public class CharacterMovement : CharacterRegistry, IReadInput
{
    public NetworkBool IsInputDisabled { get; set; }
    public float CurrentMoveSpeed { get; set; }
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [SerializeField] private CharacterController _characterController;
    public LevelManager.GamePhase CurrentGamePhase { get; set; }
    private Vector3 _currentMovement;
    private float _gravity = -7.96f;
    private float _velocity;
    private float _gravityMultiplier = 2f;
    private float _jumpPower = 4.5f;
    private CharacterAnimationController _animController;
    private CharacterStamina _characterStamina;
    private PlayerHUD _playerHUD;
    public override void Spawned()
    {
       if (!Object.HasStateAuthority) return;
       IsInputDisabled = false;
        InitScript(this);
        CurrentMoveSpeed = _characterStats.SprintSpeed;
    }

    private void Start()
    {
        switch (_characterStats.WarriorType)
        {
            case CharacterStats.CharacterType.FootKnight:
                _animController = GetScript<FootknightAnimation>();
               break;
            case CharacterStats.CharacterType.Gallowglass:
                _animController = GetScript<GallowglassAnimation>();
                break;
            case CharacterStats.CharacterType.KnightCommander:
                _animController = GetScript<KnightCommanderAnimation>();
                break;
            case CharacterStats.CharacterType.Ranger:
                _animController = GetScript<RangerAnimation>();
                break;
        }
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || _characterController.enabled == false) return;
       
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input) && !IsInputDisabled)
        {
            ReadPlayerInputs(input);
            _currentMovement = GetInputDirection(input);
            ApplyGravity();
            CalculateCharacterSpeed(input);
            _characterController.Move(_currentMovement * CurrentMoveSpeed * Runner.DeltaTime);
        }
      
    }
    public void ReadPlayerInputs(PlayerInputData input)
    {
        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);
        
        if (pressedButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Jump))
        {
           JumpPlayer();
        }

        if(pressedButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UtilitySkill))
        {
            //HandleKnockBackRPC(CharacterAttackBehaviour.AttackDirection.Forward);
            Test();
        }

        PreviousButton = input.NetworkButtons;
    }

    private float accelerationTime = 2f;
    private float elapsedTime = 0f;

    private void CalculateCharacterSpeed(PlayerInputData input)
    {
        float targetSpeed = _characterStats.MoveSpeed;

       
        if (input.VerticalInput > 0)
        {
            targetSpeed = _characterStats.SprintSpeed;
        }
       
        else if (input.HorizontalInput != 0)
        {
            targetSpeed = _characterStats.SprintSpeed;
        }

       
        if (targetSpeed == _characterStats.SprintSpeed)
        {
            elapsedTime += Time.fixedDeltaTime;
            CurrentMoveSpeed = Mathf.Lerp(_characterStats.MoveSpeed, _characterStats.SprintSpeed, elapsedTime / accelerationTime);
        }
        else
        {
            elapsedTime = 0f;
            CurrentMoveSpeed = _characterStats.MoveSpeed;
        }
     }
    private Vector3 GetInputDirection(PlayerInputData input)
    {
        float verticalSpeed = input.VerticalInput;
        float horizontalSpeed = input.HorizontalInput;
        Vector3 horizontalMovement = new Vector3(horizontalSpeed, 0, verticalSpeed).normalized;
        horizontalMovement = transform.rotation * horizontalMovement;
        return horizontalMovement;
    }
  
    private void ApplyGravity()
    {
        if (IsPlayerGrounded() && _velocity < 0.0f)
             _velocity = -1.0f;
        else
         _velocity += _gravity * _gravityMultiplier * Runner.DeltaTime;
        
        _currentMovement.y = _velocity;
    }

    public bool IsPlayerGrounded()
    {
        float rayLength = 0.5f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(rayOrigin, Vector3.down, rayLength);
    }
    private void JumpPlayer()
    {
        if (!IsPlayerGrounded()) return;
        
            
       if(_characterStats.WarriorType != CharacterStats.CharacterType.Gallowglass && _characterStamina.CanPlayerJump())
       {
            _animController.UpdateJumpAnimationState(true);
            _velocity += _jumpPower;
       }
          
       
    }
/*
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void HandleKnockBackRPC(CharacterAttackBehaviour.AttackDirection attackDirection)
    {
        Debug.Log("ZAZA");
        StartCoroutine(KnockbackPlayer(attackDirection));
        _animController.UpdateStunAnimationState(attackDirection);
    }

    public IEnumerator KnockbackPlayer(CharacterAttackBehaviour.AttackDirection attackDirection)
    {
        Vector3 movePos = Vector3.zero;
        switch (attackDirection)
        {
            case CharacterAttackBehaviour.AttackDirection.Forward:
                movePos = -transform.forward;
                break;
            case CharacterAttackBehaviour.AttackDirection.FromRight:
                movePos = transform.right;
                break;
            case CharacterAttackBehaviour.AttackDirection.FromLeft:
                movePos = -transform.right;
                break;
            case CharacterAttackBehaviour.AttackDirection.Backward:
                movePos = transform.forward;
                break;
        }

        IsInputDisabled = true;
        _playerHUD.IsStunnedBarActive = IsInputDisabled;
        float elapsedTime = 0f;
        while(elapsedTime < 3f)
        {
            _characterController.Move(movePos * Time.deltaTime * 0.5f);
            _playerHUD.UpdateStunBarFiller(elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _playerHUD.IsStunnedBarActive = false;
        IsInputDisabled = false;
        yield return new WaitForSeconds(2f);
       
       
    }
    */

    private void Test()
    {
        //_characterStamina.StunPlayerRpc(3);
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public async void HandleKnockBackRPC(CharacterAttackBehaviour.AttackDirection attackDirection)
    {
        Debug.Log("ZAZA");


        Vector3 movePos = attackDirection switch
        {
            CharacterAttackBehaviour.AttackDirection.Forward => -transform.forward,
            CharacterAttackBehaviour.AttackDirection.FromRight => transform.right,
            CharacterAttackBehaviour.AttackDirection.FromLeft => -transform.right,
            CharacterAttackBehaviour.AttackDirection.Backward => transform.forward,
            _ => Vector3.zero
        };

        IsInputDisabled = true;
        _playerHUD.IsStunnedBarActive = true;
        float elapsedTime = 0f;
        _animController.UpdateStunAnimationState(attackDirection);
        while (elapsedTime < 3f)
        {
            _characterController.Move(movePos * Time.deltaTime * 0.5f);
            _playerHUD.UpdateStunBarFiller(elapsedTime);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        _playerHUD.IsStunnedBarActive = false;
        IsInputDisabled = false;

        await UniTask.Delay(TimeSpan.FromSeconds(2));
       
    }


    private void OnDrawGizmos()
    {
        float rayLength = 0.3f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayOrigin, Vector3.down * rayLength);
    }

    
}
