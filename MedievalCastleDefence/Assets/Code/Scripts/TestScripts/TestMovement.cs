using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TestMovement : NetworkBehaviour
{
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CharacterStats _characterStats;
  

    private Vector3 _currentMovement;
    private float _gravity = -7.96f;
    private float _velocity;
    private float _gravityMultiplier = 2f;
    private float _jumpPower = 4.5f;
    private CharacterAnimationController _animController;

    public override void Spawned()
    {
        _characterController = GetComponent<CharacterController>();
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
            _currentMovement = GetInputDirection(input);
            ApplyGravity();
            _characterController.Move(_currentMovement * _characterStats.MoveSpeed * Runner.DeltaTime);
            
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
    private Vector3 GetInputDirection(PlayerInputData input)
    {
        float verticalSpeed = input.VerticalInput;
        float horizontalSpeed = input.HorizontalInput;
        Vector3 horizontalMovement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
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
       // _animController.UpdateJumpAnimationState(true);
        _velocity += _jumpPower;
    }

}
