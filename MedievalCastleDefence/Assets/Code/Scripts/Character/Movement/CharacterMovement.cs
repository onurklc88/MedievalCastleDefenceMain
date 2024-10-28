using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class CharacterMovement : BehaviourRegistry, IReadInput
{
    public NetworkBool IsInputDisabled { get; set; }
    public float CurrentMoveSpeed { get; set; }
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [SerializeField] private CharacterController _characterController;
    public bool IsPlayerStunned { get; set; }
  
    private Vector3 _currentMovement;
    private float _gravity = -7.96f;
    private float _velocity;
    private float _gravityMultiplier = 2f;
    private float _jumpPower = 4.5f;
    private CharacterAnimationController _animController;
    
  
    
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

        }
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
       
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input) && !IsPlayerStunned)
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
    private void IncreasePlayerSpeed()
    {

    }
    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity < 0.0f)
             _velocity = -1.0f;
        else
         _velocity += _gravity * _gravityMultiplier * Runner.DeltaTime;
        
        _currentMovement.y = _velocity;
    }

    private void JumpPlayer()
    {
        if (!_characterController.isGrounded) return;
        if(_animController != null)
        {
            _animController.UpdateJumpAnimationState(true);
        }
        else
        {
            Debug.Log("Anim controller yok");
        }
       
       _velocity += _jumpPower;
    }

    public IEnumerator KnockbackPlayer()
    {
        IsPlayerStunned = true;
        float elapsedTime = 0f;
        while(elapsedTime < 0.7f)
        {
            _characterController.Move(-transform.forward * Time.deltaTime * 1.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(10f);
        IsPlayerStunned = false;
    }

    
}
