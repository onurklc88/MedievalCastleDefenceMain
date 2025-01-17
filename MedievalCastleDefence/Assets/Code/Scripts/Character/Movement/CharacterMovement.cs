using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

public class CharacterMovement : CharacterRegistry, IReadInput
{
    public NetworkBool IsInputDisabled { get; set; }
    public float CurrentMoveSpeed { get; set; }
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [SerializeField] private CharacterController _characterController;
    public bool IsPlayerStunned { get; set; }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }

    private Vector3 _currentMovement;
    private float _gravity = -7.96f;
    private float _velocity;
    private float _gravityMultiplier = 2f;
    private float _jumpPower = 4.5f;
    private CharacterAnimationController _animController;

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
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || _characterController.enabled == false) return;
       
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input) && !IsPlayerStunned && !IsInputDisabled)
        {
            ReadPlayerInputs(input);
            _currentMovement = GetInputDirection(input);
            ApplyGravity();
            CalculateCharacterDirection(input);
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

        PreviousButton = input.NetworkButtons;
    }
    private void CalculateCharacterDirection(PlayerInputData input)
    {
        if(input.VerticalInput < 0)
        {
            CurrentMoveSpeed = _characterStats.MoveSpeed;
        }
        else
        {
            CurrentMoveSpeed = _characterStats.SprintSpeed;
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
        
            
       if(_characterStats.WarriorType != CharacterStats.CharacterType.Gallowglass)
       {
            _animController.UpdateJumpAnimationState(true);
            _velocity += _jumpPower;
       }
          
       
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void HandleKnockBackRPC(CharacterAttackBehaviour.AttackDirection attackDirection)
    {
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
       
        
        IsPlayerStunned = true;
        float elapsedTime = 0f;
        while(elapsedTime < 0.7f)
        {
            _characterController.Move(movePos * Time.deltaTime * 1.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        IsPlayerStunned = false;
    }


    private void OnDrawGizmos()
    {
        float rayLength = 0.3f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayOrigin, Vector3.down * rayLength);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
        switch (CurrentGamePhase)
        {
            case LevelManager.GamePhase.Preparation:
                _characterController.enabled = false;
                break;
            case LevelManager.GamePhase.RoundStart:
                _characterController.enabled = true;
                break;
        }
    }
}
