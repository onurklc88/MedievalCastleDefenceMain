using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class FootKnightAttack : CharacterAttackBehaviour
{
    [Networked(OnChanged = nameof(OnSwordStateChangedNetwork))] private bool _swordInput { get; set; }
    [SerializeField] private RPCDebugger _rpcdebugger;
    [SerializeField] private Rigidbody _shieldRigidbody;
    private FootknightAnimation _animation;
    private CharacterMovement _characterMovement;
    [SerializeField] private GameObject _smokeBomb;
    [SerializeField] private GameObject _smokeBombPosition;
    [SerializeField] private GameObject _sword;
    private bool _hasResetBombAnimation { get; set; }


    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.FootKnight;
        _shieldRigidbody.mass = 0;
        InitScript(this);
       _animation = transform.GetComponent<FootknightAnimation>();
        base.PlayerSwordPositionLocal = SwordPosition.Right;
    }
    private void Start()
    {
        _characterMovement = GetScript<CharacterMovement>();
        _characterStamina = GetScript<CharacterStamina>();
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
    private static void OnSwordStateChangedNetwork(Changed<FootKnightAttack> changed)
    {
        var behaviour = changed.Behaviour;

       
        if (behaviour._isBombThrown)
        {
            behaviour._sword.SetActive(true);
        }
        else 
        {
            behaviour._sword.SetActive(!behaviour._swordInput);
        }
    }
    public override void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
      
        if (_characterMovement != null && _characterMovement.IsInputDisabled)
        {
            IsPlayerBlockingLocal = false;
            _animation.IsPlayerParry = IsPlayerBlockingLocal;
            return;
        }
        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
       // IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
         
        _isPlayerHoldingBomb = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Throwable);
        _swordInput = _isPlayerHoldingBomb;
        //UpdateBombVisuals();
        if (!_isPlayerHoldingBomb && !IsPlayerBlockingLocal && !_isBombThrown && _wasHoldingLastFrame)
        {
            ThrowBomb();
        }
        _wasHoldingLastFrame = _isPlayerHoldingBomb;
        if (_animation != null)
            _animation.IsPlayerParry = IsPlayerBlockingLocal;

        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (IsPlayerBlockingLocal && !_isPlayerHoldingBomb)
        {
            //_characterMovement.CurrentMoveSpeed = _characterStats.MoveSpeed;

            if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && AttackCooldown.ExpiredOrNotRunning(Runner) && !_characterHealth.IsPlayerGotHit)
            {
                //ParryAttack();

            }
           
        }
        else if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner) && !_characterHealth.IsPlayerGotHit && !_isPlayerHoldingBomb)
        {
           
            if (_characterStamina.CurrentAttackStamina > _weaponStats.StaminaWaste)
            {
                SwingSword();
            }
        }
        else
        {
            if(_characterMovement != null && _characterMovement.IsPlayerSlowed)
            {
                _characterMovement.CurrentMoveSpeed = _characterStats.SprintSpeed;
            }

        }


        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UtilitySkill) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {
            //_characterStamina.DecreaseDefenceStaminaRPC(50f);
            //transform.GetComponentInParent<StormshieldVFXController>().UpdateParryVFXRpc();
            // IsPlayerBlockingLocal = true;
            //_activeRagdoll.RPCActivateRagdoll();
            //_animation.UpdateDamageAnimationState();
            
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
        if (!Object.HasStateAuthority) return;
        if (IsPlayerBlockingLocal || !_characterCollision.IsPlayerGrounded) return;
        if (_characterMovement.IsPlayerSlowed)
        {
            if (_animation.GetCurrentAnimationState("UpperBody") == "Slowed1") return;
        }
        //_playerVFX.EnableWeaponParticles();
        _animation.UpdateSwingAnimationState(true);
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

        var bomb = Runner.Spawn(_smokeBomb, _smokeBombPosition.transform.position, Quaternion.identity, Runner.LocalPlayer);
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
        StartCoroutine(ResetBombStateAfterDelay(30));
    }

    public override void InterruptEnemyAction()
    {
        _isPlayerHoldingBomb = false;
        _swordInput = false;
        _isBombThrown = false;
        _animation.UpdateThrowingAnimation(_isPlayerHoldingBomb);
        IsDummyBombActivated = false;

    }
    private void UpdateBombVisuals()
    {
        if (_isBombThrown && !_hasResetBombAnimation)
        {
            IsDummyBombActivated = false;
            _animation.UpdateThrowingAnimation(false);
            _hasResetBombAnimation = true;
            return;
        }

        if (!IsPlayerBlockingLocal && _animation != null && !_isBombThrown)
        {
            IsDummyBombActivated = _isPlayerHoldingBomb;
            _animation.UpdateThrowingAnimation(_isPlayerHoldingBomb);
            _hasResetBombAnimation = false;
        }
    }
    private IEnumerator PerformAttack()
    {

        _blockColliders[0].enabled = false;
        yield return new WaitForSeconds(0.10f);
        float elapsedTime = 0f;
        while (elapsedTime < 0.4f)
        {
            Vector3 swingDirection = transform.position + transform.up * 1.2f + transform.forward + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);
            int layerMask = ~LayerMask.GetMask("Ragdoll");
            Collider[] _hitColliders = Physics.OverlapSphere(swingDirection, 0.6f, layerMask);

            var target = _hitColliders.FirstOrDefault(c => c.gameObject.layer == 10 || c.gameObject.layer == 11)
                         ?? _hitColliders.FirstOrDefault();


            if (target != null)
            {
                Debug.LogError("ATTACKCLASS_________________GameObjectName: " + target.transform.gameObject.name + " GameObjectLayer: " + target.transform.gameObject.layer);
                CheckAttackCollision(target.transform.gameObject);
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        _blockColliders[0].enabled = true;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward, 0.6f);
        //Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward / 0.94f, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }
}
