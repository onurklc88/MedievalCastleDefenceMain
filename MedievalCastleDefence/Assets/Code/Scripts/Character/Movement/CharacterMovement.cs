using Cysharp.Threading.Tasks;
using UnityEngine;
using Fusion;
using System;
using static BehaviourRegistry;

public class CharacterMovement : CharacterRegistry, IReadInput
{
    //public NetworkBool IsInputDisabled { get; set; }
    private bool _isInputDisabled;
    public float CurrentMoveSpeed { get; set; }
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [SerializeField] private CharacterController _characterController;
    public LevelManager.GamePhase CurrentGamePhase { get; set; }
    private Vector3 _currentMovement;
    private float _gravity = -7.96f;
    private float _velocity;
    private float _gravityMultiplier = 2f;
   [SerializeField]  private float _jumpPower = 3.3f;
   [SerializeField]  private float _highJumpPower = 3.3f;
    private CharacterAnimationController _animController;
    private CharacterStamina _characterStamina;
    private PlayerHUD _playerHUD;

    //test
    private float _currentDirection = 1f;
    private bool _isAutoMoving = false;
    private float _movementBound = 5f; // 5 metre sýnýr
    private Vector3 _startPosition; // Baþlangýç pozisyonu
    public override void Spawned()
    {
       if (!Object.HasStateAuthority) return;
       IsInputDisabled = false;
        InitScript(this);
        CurrentMoveSpeed = _characterStats.SprintSpeed;
    }

    public NetworkBool IsInputDisabled
    {
        get => _isInputDisabled;
        set
        {
            _isInputDisabled = value;
            if (_isInputDisabled)
            {
                _currentMovement.x = 0;
                _currentMovement.z = 0;
            }
        }
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
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            //ReadPlayerInputs(input);
            if (!IsInputDisabled)
            {
                var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);
               
                if (pressedButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Interact) && !_isAutoMoving)
                {
                    
                    _isAutoMoving = true;
                    _startPosition = transform.position;
                }
                _currentMovement = GetInputDirection(input); 
                CalculateCharacterSpeed(input);
            }
           
        }
        ApplyGravity();
       _characterController.Move(_currentMovement * CurrentMoveSpeed * Runner.DeltaTime);
        
       
        PreviousButton = input.NetworkButtons;

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
            //Test();
        }

        PreviousButton = input.NetworkButtons;
    }

    private float _accelerationTime = 2f;
    private float _elapsedTime = 0f;

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
            _elapsedTime += Time.fixedDeltaTime;
            CurrentMoveSpeed = Mathf.Lerp(_characterStats.MoveSpeed, _characterStats.SprintSpeed, _elapsedTime / _accelerationTime);
        }
        else
        {
            _elapsedTime = 0f;
            CurrentMoveSpeed = _characterStats.MoveSpeed;
        }
     }
    private Vector3 GetInputDirection(PlayerInputData input)
    {
        if (!_isAutoMoving)
        {
            float verticalSpeed = input.VerticalInput;
            float horizontalSpeed = input.HorizontalInput;
            Vector3 horizontalMovement = new Vector3(horizontalSpeed, 0, verticalSpeed).normalized;
            horizontalMovement = transform.rotation * horizontalMovement;
            horizontalMovement.y = _currentMovement.y;

            return horizontalMovement;
        }
        else
        {
            float currentDistance = transform.position.x - _startPosition.x;

            // Sýnýr aþýldýysa yönü tersine çevir
            if (Mathf.Abs(currentDistance) >= _movementBound)
            {
                _currentDirection *= -1f; // Yön deðiþtir
                _startPosition = transform.position; // Sýnýrý resetle
            }

            return new Vector3(_currentDirection, 0, 0);
        }
       
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

        if (_characterStats.WarriorType != CharacterStats.CharacterType.Gallowglass && _characterStamina.CanPlayerJump())
        {
            _animController.UpdateJumpAnimationState(true);
            _velocity = _jumpPower; 
        }
    }
   
    private void Test()
    {
          
       
        //_characterStamina.StunPlayerRpc(3);
    }
    private bool _pushApplied = false;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
    }

    private void ResetPush()
    {
        _pushApplied = false;
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
        //_animController.UpdateStunAnimationState(attackDirection);
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


    #region legacy
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

    #endregion

}
