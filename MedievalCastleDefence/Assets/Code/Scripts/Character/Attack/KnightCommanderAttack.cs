using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
public class KnightCommanderAttack : CharacterAttackBehaviour
{
    
    private PlayerHUD _playerHUD;
    private KnightCommanderAnimation _knightCommanderAnimation;
    private CharacterMovement _characterMovement;
    private PlayerVFXSytem _playerVFXSystem;
    private int _lockedBlockDirection = 0;
    private int _lastBlockDirection = 0;
    [SerializeField] private GameObject _explosiveBomb;
    [SerializeField] private GameObject _explosiveBombPos;
    [SerializeField] private RPCDebugger _debugger;
    [SerializeField] private GameObject test;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.FootKnight;
        InitScript(this);
        _knightCommanderAnimation = GetScript<KnightCommanderAnimation>();
    }
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
        _characterMovement = GetScript<CharacterMovement>();
        _playerVFXSystem = GetScript<IronheartVFXController>();
        _playerStatsController = GetScript<PlayerStatsController>();
        _characterHealth = GetScript<CharacterHealth>();
        _characterCollision = GetScript<CharacterCollision>();
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }
    }

    public override void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
        if (_characterMovement != null && _characterMovement.IsInputDisabled && !_isPlayerHoldingBomb) 
        {
            IsPlayerBlockingLocal = false;
            _knightCommanderAnimation.IsPlayerParry = IsPlayerBlocking;
            return; 
        }
        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
       _isPlayerHoldingBomb = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Throwable);
        //UpdateBombVisuals();
        if (!_isPlayerHoldingBomb && !IsPlayerBlockingLocal && !_isBombThrown && _wasHoldingLastFrame)
        {
            ThrowBomb();
        }
        _wasHoldingLastFrame = _isPlayerHoldingBomb;

        if (!IsPlayerBlocking && _playerHUD != null) _playerHUD.HandleArrowImages(GetSwordPosition());
       
       // IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
      
      
        if (!IsPlayerBlockingLocal) PlayerSwordPositionLocal = base.GetSwordPosition();
        if (_knightCommanderAnimation != null) BlockWeapon();
        //if (!IsPlayerBlocking && _knightCommanderAnimation != null) BlockWeapon();

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner) && !_characterHealth.IsPlayerGotHit && !_isPlayerHoldingBomb && _characterStamina.CurrentAttackStamina > 30)
        {

            if (_characterStamina.CurrentAttackStamina > 30)
            {
                SwingSword();
            }
            else
            {
                //not enough stamina
            }
        }

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {
           // _knightCommanderAnimation.UpdateDamageAnimationState();
            //_isPlayerHoldingBomb = true;
            //_characterStamina.DecreaseDefenceStaminaRPC(28f);
            //_bloodDecals.EnableRandomBloodDecal();
            //IsPlayerBlockingLocal = true;
            //_ragdollManager.RPCActivateRagdoll();
            
            if (IsPlayerBlockingLocal == true)
            {
                IsPlayerBlockingLocal = false;
            }
            else
            {
                IsPlayerBlockingLocal = true;
            }
            
        }

       
        PreviousButton = input.NetworkButtons;
    }

    private void LateUpdate()
    {
        UpdateBombVisuals();
    }
    protected override void SwingSword()
    {
        if (IsPlayerBlockingLocal || !_characterCollision.IsPlayerGrounded || _isPlayerHoldingBomb) return;
      
        if (_characterMovement.IsPlayerSlowed)
        {
            if (_knightCommanderAnimation.GetCurrentAnimationState("UpperBody") == "Slowed1") return;
        }
        //_playerVFXSystem.EnableWeaponParticles();
        if (_knightCommanderAnimation == null)
        {
            Debug.Log("is null? :");
        }
      
        _knightCommanderAnimation.UpdateAttackAnimState(((int)base.GetSwordPosition() == 0 ? 2 : (int)base.GetSwordPosition()));
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _characterStamina.DecreaseCharacterAttackStamina(_weaponStats.StaminaWaste);
         StartCoroutine(PerformAttack());
    }

    protected override void ThrowBomb()
    {
        if (_characterMovement.IsInputDisabled) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
       // Vector3 endPoint = ray.origin + ray.direction * 10f;
        IThrowable bombInterface = null;

        var bomb = Runner.Spawn(_explosiveBomb, _explosiveBombPos.transform.position, Quaternion.identity, Runner.LocalPlayer);
        bombInterface = bomb.GetComponent<IThrowable>();
        if (bombInterface == null)
        {
            Debug.LogError("Bomb cannot find!");
            return;
        }

        bombInterface.SetOwner(_playerStatsController.PlayerNetworkStats);
        Vector3 initialForce = ray.direction * 30f + _dummyBomb.transform.forward + Vector3.up * 1.75f;
        transform.rotation = Quaternion.LookRotation(initialForce);
        bomb.GetComponent<Rigidbody>().AddForce(initialForce, ForceMode.Impulse);
        _isBombThrown = true;
        StartCoroutine(ResetBombStateAfterDelay(17f));
    }
    private bool _hasResetBombAnimation; 

    private void UpdateBombVisuals()
    {
        if (_isBombThrown && !_hasResetBombAnimation)
        {
            IsDummyBombActivated = false;
            _knightCommanderAnimation.UpdateThrowingAnimation(false);
            _hasResetBombAnimation = true;
            return;
        }

        if (!IsPlayerBlockingLocal && _knightCommanderAnimation != null && !_isBombThrown)
        {
            IsDummyBombActivated = _isPlayerHoldingBomb;
            _knightCommanderAnimation.UpdateThrowingAnimation(_isPlayerHoldingBomb);
            _hasResetBombAnimation = false; 
        }
    }

    public override void InterruptEnemyAction()
    {
        _isPlayerHoldingBomb = false;
        _isBombThrown = false;
        _knightCommanderAnimation.UpdateThrowingAnimation(_isPlayerHoldingBomb);
        IsDummyBombActivated = false;

    }
    private IEnumerator PerformAttack()
    {
        _playerVFXSystem.ActivateSwordTrail(true);
        yield return new WaitForSeconds(0.27f);
        float elapsedTime = 0f;
        while (elapsedTime < 0.4f)
        {
           
            Vector3 swingDirection = transform.position + transform.up * 1.2f + transform.forward * 1.1f + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.68f : -0.68f);
            int layerMask = ~LayerMask.GetMask("Ragdoll");
            Collider[] _hitColliders = Physics.OverlapSphere(swingDirection, 0.6f, layerMask);

            var target = _hitColliders.FirstOrDefault(c => c.gameObject.layer == 10 || c.gameObject.layer == 11)
                         ?? _hitColliders.FirstOrDefault();

            if (target != null)
            {
                
                CheckAttackCollision(target.transform.gameObject);
                break;
            }
          
            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        yield return new WaitForSeconds(0.3f);
        _playerVFXSystem.ActivateSwordTrail(false);
    }
   

    protected override void BlockWeapon()
    {
        if (IsPlayerBlocking)
        {
            int currentDirection = (int)GetSwordPosition();

           
            if (_lockedBlockDirection == 0 || _lockedBlockDirection == currentDirection)
            {
                _lockedBlockDirection = currentDirection;
                _lastBlockDirection = currentDirection;
            }

        
            _knightCommanderAnimation.UpdateBlockAnimState(_lockedBlockDirection);
        }
        else
        {
            
            _lockedBlockDirection = 0;
            _knightCommanderAnimation.UpdateBlockAnimState(0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward * 1.1f + -transform.right * 0.68f, 0.6f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward * 1.1f + transform.right * 0.68f, 0.6f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }


  
}
