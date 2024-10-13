using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace test
{


    public class CharacterMovement1 : CharacterRegistry, IReadInput
    {
        public NetworkBool IsInputDisabled { get; set; }
        public float CurrentMoveSpeed { get; set; }
        [Networked] public NetworkButtons PreviousButton { get; set; }
        [SerializeField] private CharacterController _characterController;
       
        private Vector3 _currentMovement;
        private float _gravity = -7.96f;
        private float _velocity;
        private float _gravityMultiplier = 2f;
        private float _jumpPower = 4.5f;
        private CharacterAnimationController _animController;

        private float _knockbackDuration = 0.7f;
        private TestClassA _classA;
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
                    //_animController = GetScript(FootknightAnimation);
                    //var test = GetScript<CharacterMovement1>();
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    break;
            }
            _classA = GetScript<TestClassA>();
            _classA.TestZ();
        }
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input) && !IsInputDisabled)
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
            if (input.VerticalInput < 0)
            {
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
            _animController.UpdateJumpAnimationState(true);
            _velocity += _jumpPower;
        }

        public IEnumerator KnockbackPlayer()
        {
            IsInputDisabled = true;
            float elapsedTime = 0f;
            while (elapsedTime < _knockbackDuration)
            {
                _characterController.Move(-transform.forward * Time.deltaTime * 1.5f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            IsInputDisabled = false;
        }


    }
}
