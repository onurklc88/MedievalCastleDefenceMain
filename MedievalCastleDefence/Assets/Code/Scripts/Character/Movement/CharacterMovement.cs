using Cysharp.Threading.Tasks;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : CharacterRegistry, IReadInput
{
    public bool IsPlayerSlowed { get; set; }   
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [SerializeField] private float _accelerationTime = 2f;
    public float CurrentMoveSpeed;
   
    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    private float _elapsedTime; 
    private CharacterAnimationController _animController;
    private CharacterStamina _characterStamina;
    private CharacterCollision _characterCollision;
    private bool _isInputDisabled;

    private bool autoMoveEnabled = false; 
    private Vector3 _autoMoveStartPos;
    private int _autoMoveDirection = 1;
    [Networked] public NetworkButtons PreviousButton { get; set; }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _rigidbody.drag = 0f;
        _autoMoveStartPos = transform.position;
    }

    private void Start()
    {
        switch (base._characterStats.WarriorType)
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
        _characterStamina = GetScript<CharacterStamina>();
        _characterCollision = GetScript<CharacterCollision>();
    }

    public NetworkBool IsInputDisabled
    {
        get => _isInputDisabled;
        set
        {
            _isInputDisabled = value;
            if (_isInputDisabled)
            {
                _moveDirection = Vector3.zero;
                _rigidbody.velocity = Vector3.zero;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || IsInputDisabled || _characterCollision == null) return;

        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                autoMoveEnabled = !autoMoveEnabled;
            }
         
            if (!_characterCollision.IsPlayerGrounded)
            {
               _rigidbody.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
            }
           
            CalculateCurrentSpeed(input); 
            HandleMovement(input);
            HandleJump(input);

            PreviousButton = input.NetworkButtons;
        }
      
    }

    
    private void CalculateCurrentSpeed(PlayerInputData input)
    {
        if (IsPlayerSlowed) return;
        float targetSpeed = (input.VerticalInput > 0 || input.HorizontalInput != 0) ? _characterStats.SprintSpeed : _characterStats.MoveSpeed;

        if (targetSpeed == _characterStats.SprintSpeed)
        {
            _elapsedTime += Runner.DeltaTime;
            CurrentMoveSpeed = Mathf.Lerp(_characterStats.MoveSpeed, _characterStats.SprintSpeed, _elapsedTime / _accelerationTime);
        }
        else
        {
            _elapsedTime = 0f;
            CurrentMoveSpeed = _characterStats.MoveSpeed;
        }
    }

    private void HandleMovement(PlayerInputData input)
    {
        if (autoMoveEnabled)
        {
            AutoMove();
            return;
        }
        Vector3 inputDir = new Vector3(input.HorizontalInput, 0f, input.VerticalInput);
        _moveDirection = transform.TransformDirection(inputDir.normalized);

        Vector3 targetVelocity = _moveDirection * CurrentMoveSpeed;
        targetVelocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = targetVelocity;
    }

    private void AutoMove()
    {
        
        Vector3 direction = Vector3.right * _autoMoveDirection;
        Vector3 moveDir = transform.TransformDirection(direction.normalized);

        Vector3 velocity = moveDir * CurrentMoveSpeed;
        velocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = velocity;
        if (Mathf.Abs(transform.position.x - _autoMoveStartPos.x) >= 5f)
        {
            _autoMoveDirection *= -1;
            _autoMoveStartPos = transform.position;
        }
    }
    private void HandleJump(PlayerInputData input)
    {
        var pressed = input.NetworkButtons.GetPressed(PreviousButton);
        if (pressed.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Jump) && _characterCollision.IsPlayerGrounded &&
            _characterStamina.CanPlayerJump())
        {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _animController?.UpdateJumpAnimationState(true);
        }
    }

    public void ThrowCharacter()
    {
        float force = 150f; 
        Vector3 direction = (Vector3.forward * 1f + Vector3.up * 0.5f).normalized;

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(Vector3.up + transform.forward * force, ForceMode.Impulse);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void RPC_SlowPlayerSpeed()
    {
       
        if (!HasStateAuthority || _isInputDisabled || IsPlayerSlowed) return;
      
        IsPlayerSlowed = true;
        CurrentMoveSpeed = CurrentMoveSpeed / 2;
        _animController.ChangeAnimationSpeed(true);
        await UniTask.Delay(2500);
        CurrentMoveSpeed = _characterStats.MoveSpeed;
        IsPlayerSlowed = false;
        _animController.ChangeAnimationSpeed(false);
    }

    public void ReadPlayerInputs(PlayerInputData input) { }
   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * _groundCheckDistance);
    }
}

/*
[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public async void HandleKnockBackRPC(CharacterAttackBehaviour.AttackDirection attackDirection)
    {
        Vector3 knockbackDirection = GetKnockbackDirection(attackDirection);
        IsInputDisabled = true;
        _playerHUD.IsStunnedBarActive = true;

        float elapsedTime = 0f;
        while (elapsedTime < 3f)
        {
            _rigidbody.AddForce(knockbackDirection * 5f, ForceMode.Force);
            _playerHUD.UpdateStunBarFiller(elapsedTime / 3f);
            elapsedTime += Runner.DeltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        _playerHUD.IsStunnedBarActive = false;
        IsInputDisabled = false;
    }

    private Vector3 GetKnockbackDirection(CharacterAttackBehaviour.AttackDirection direction)
    {
        return direction switch
        {
            CharacterAttackBehaviour.AttackDirection.Forward => -transform.forward,
            CharacterAttackBehaviour.AttackDirection.FromRight => transform.right,
            CharacterAttackBehaviour.AttackDirection.FromLeft => -transform.right,
            CharacterAttackBehaviour.AttackDirection.Backward => transform.forward,
            _ => Vector3.zero
        };
    }

    public bool IsPlayerGrounded() => _isGrounded;

    public void ReadPlayerInputs(PlayerInputData input)
    {
        // Implement if needed
    }
*/


#region legacy
/*
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
    private PlayerInputData _playerInputData;
    private bool _test;
    //test
    private float _currentDirection = 1f;
    private bool _isAutoMoving = false;
    private float _movementBound = 5f; // 5 metre sınır
    private Vector3 _startPosition; // Başlangıç pozisyonu
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
        _test = false; // Her frame başında sıfırla
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
            _playerInputData = input;
        }
        ApplyGravity();
        // CheckPlayerCollision();
        MoveCharacter(input);

        //Debug.Log("TESTBOOL : " + _test);
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

            // Sınır aşıldıysa yönü tersine çevir
            if (Mathf.Abs(currentDistance) >= _movementBound)
            {
                _currentDirection *= -1f; // Yön değiştir
                _startPosition = transform.position; // Sınırı resetle
            }

            return new Vector3(_currentDirection, 0, 0);
        }
       
    }

    private void MoveCharacter(PlayerInputData input)
    {
        if (_test)
        {
            if (input.HorizontalInput != 0)
                _currentMovement.x = 0;

            if (input.VerticalInput != 0)
                _currentMovement.z = 0;
         
        }
        Debug.Log("CurrentMovement: " + _currentMovement+ "bool: " +_test);
        _characterController.Move(_currentMovement * CurrentMoveSpeed * Runner.DeltaTime);
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
    

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {


        if (hit.gameObject.layer == 6)
        {
            _test = true;
        }



    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            _test = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
       
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



//}

#endregion
