using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;


public class GallowglassAttack : CharacterAttackBehaviour
{
    [SerializeField] private GameObject _explosiveBomb;
    [SerializeField] private GameObject _explosiveBombPos;
    
    private PlayerHUD _playerHUD;
    private GallowglassAnimation _gallowGlassAnimation;
    private CharacterMovement _characterMovement;
    private BloodhandSkill _bloodhandSkill;
    private BloodhandVFXController _bloodhandVFX;
    private TickTimer _blockReleaseCooldown;
    private int _lockedBlockDirection = 0;
    private int _lastBlockDirection = 0;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.Gallowglass;
        InitScript(this);
    }
    private void Start()
    {
        base._playerStatsController = GetScript<PlayerStatsController>();
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
        _characterMovement = GetScript<CharacterMovement>();
        _gallowGlassAnimation = GetScript<GallowglassAnimation>();
        //_activeRagdoll = GetScript<ActiveRagdoll>();
        _bloodhandVFX = GetScript<BloodhandVFXController>();
        _bloodhandSkill = GetScript<BloodhandSkill>();
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
        if (_characterMovement != null && _characterMovement.IsInputDisabled)
        {
            return;
        }

        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (!IsPlayerBlocking && _playerHUD != null) _playerHUD.HandleArrowImages(GetSwordPosition());


        bool wasBlocking = IsPlayerBlockingLocal;
        //IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        _isPlayerHoldingBomb = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Throwable);
        UpdateBombVisuals();
        // && !_isBombThrown
        if (!_isPlayerHoldingBomb && !IsPlayerBlockingLocal && _wasHoldingLastFrame && !_isBombThrown)
        {
            ThrowBomb();
        }
        _wasHoldingLastFrame = _isPlayerHoldingBomb;

        //Debug.Log("_isPlayerHoldingBomb: " + _isPlayerHoldingBomb);

        if (wasBlocking && !IsPlayerBlockingLocal)
        {
            _blockReleaseCooldown = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }

        if (!IsPlayerBlockingLocal) PlayerSwordPositionLocal = base.GetSwordPosition();
        if (_gallowGlassAnimation != null) BlockWeapon();


        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner) && !_bloodhandSkill.IsAbilityInUseLocal && !IsPlayerBlocking && (_blockReleaseCooldown.ExpiredOrNotRunning(Runner)) && !_characterHealth.IsPlayerGotHit && !_isPlayerHoldingBomb)
        {
            if (_characterStamina.CurrentAttackStamina > 30)
            {

                SwingSword();

            }
        }
        else if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UtilitySkill) && !_bloodhandSkill.IsAbilityInUseLocal)
        {
            
           if(IsPlayerBlockingLocal == true)
            {
                IsPlayerBlockingLocal = false;
            }
            else
            {
                IsPlayerBlockingLocal = true;
            }
          
            // _characterStamina.DecreaseDefenceStaminaRPC(60f);
            // transform.GetComponentInParent<BloodhandVFXController>().UpdateParryVFXRpc();
        }

        PreviousButton = input.NetworkButtons;
    }

    private void LateUpdate()
    {
        UpdateBombVisuals();
        if (Input.GetKeyDown(KeyCode.C))
        {
            _gallowGlassAnimation.UpdateDamageAnimationState();
        }
    }
    private void UpdateBombVisuals()
    {
        if(!IsPlayerBlockingLocal && _gallowGlassAnimation != null && !_isBombThrown)
        {
            IsDummyBombActivated = _isPlayerHoldingBomb;
            _gallowGlassAnimation.UpdateThrowingAnimation(_isPlayerHoldingBomb);
        }
    }

    protected override void BlockWeapon()
    {
        if (IsPlayerBlocking && !_isPlayerHoldingBomb)
        {
            int currentDirection = (int)GetSwordPosition();


            if (_lockedBlockDirection == 0 || _lockedBlockDirection == currentDirection)
            {
                _lockedBlockDirection = currentDirection;
                _lastBlockDirection = currentDirection;
            }


            _gallowGlassAnimation.UpdateBlockAnimState(_lockedBlockDirection);
        }
        else
        {

            _lockedBlockDirection = 0;
            _gallowGlassAnimation.UpdateBlockAnimState(0);
        }
    }
   
    protected override void SwingSword()
    {
        if (IsPlayerBlockingLocal || !_characterCollision.IsPlayerGrounded) return;
        if (_characterMovement.IsPlayerSlowed)
        {
            if (_gallowGlassAnimation.GetCurrentAnimationState("UpperBody") == "Slowed1") return;
        }
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, 1f);
        _characterStamina.DecreaseCharacterAttackStamina(_weaponStats.StaminaWaste);
        _gallowGlassAnimation.UpdateAttackAnimState(((int)base.GetSwordPosition() == 0 ? 2 : (int)base.GetSwordPosition()));
        if(base.GetSwordPosition() == SwordPosition.Right)
        {
            StartCoroutine(PerformAttack(0.24f));
        }
        else
        {
            StartCoroutine(PerformAttack(0f));
        }
      
    }

    protected override void ThrowBomb()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * 10f;
        IThrowable bombInterface = null;
      
       var bomb = Runner.Spawn(_explosiveBomb, _explosiveBombPos.transform.position, Quaternion.identity, Runner.LocalPlayer);
       bombInterface = bomb.GetComponent<IThrowable>();
       if (bombInterface == null)
       {
            Debug.LogError("Bomb scripti bulunamadý!");
            return;
       }
        bombInterface.SetOwner(_playerStatsController.PlayerNetworkStats);
        Vector3 initialForce = ray.direction * 25f + transform.forward + Vector3.up * 1.75f + Vector3.right * 0.5f;
        transform.rotation = Quaternion.LookRotation(initialForce);
        bomb.GetComponent<Rigidbody>().AddForce(initialForce, ForceMode.Impulse);
       _isBombThrown = true;
        StartCoroutine(ResetBombStateAfterDelay(20f));
    }
    private IEnumerator PerformAttack(float duration)
    {
        //base._blockArea.enabled = false;
        //base._testAreas[0].enabled = false;
        //base._testAreas[1].enabled = false;
        _bloodhandVFX.ActivateAxeTrail(true);
        yield return new WaitForSeconds(0.24f);

        float elapsedTime = 0f;


        while (elapsedTime < 0.5f)
        {
            Vector3 swingDirection = transform.position + transform.up * 1.2f + transform.forward * 1.2f + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.7f : -0.7f);
            int layerMask = ~LayerMask.GetMask("Ragdoll");
            Collider[] hitColliders = Physics.OverlapSphere(swingDirection, 0.7f, layerMask);

            var target = hitColliders.FirstOrDefault(c => c.gameObject.layer == 10 || c.gameObject.layer == 11)
                         ?? hitColliders.FirstOrDefault();

            if (target != null)
            {
                Debug.Log("ObjeÝsmi: " +target.transform.gameObject.name);
                CheckAttackCollision(target.transform.gameObject);
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        //base._blockArea.enabled = true;
        _bloodhandVFX.ActivateAxeTrail(false);
    }

    public override void InterruptEnemyAction()
    {
        if (!_characterMovement.IsInputDisabled) return;
        _isPlayerHoldingBomb = false;
      
        _isBombThrown = false;
        _gallowGlassAnimation.UpdateThrowingAnimation(_isPlayerHoldingBomb);
        IsDummyBombActivated = false;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward * 1.2f + -transform.right * 0.7f, 0.7f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward * 1.2f + transform.right * 0.7f, 0.7f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);
    }

    #region Legacy
    private IEnumerator PerformKickAction()
    {
        yield return new WaitForSeconds(0.65f);

        Vector3 kickPosition = transform.position + transform.up + transform.forward * 1.7f + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);

        Collider[] hitColliders = Physics.OverlapSphere(kickPosition, 0.55f);
        GameObject lastHitOpponent = null;


        foreach (var collider in hitColliders)
        {
            GameObject opponent = collider.transform.root.gameObject;

            if (opponent.layer != 10 && opponent != lastHitOpponent && opponent.transform.GetComponentInParent<NetworkObject>().Id != transform.GetComponentInParent<NetworkObject>().Id)
            {
                lastHitOpponent = opponent;
                KickOpponent(opponent);
                break;
            }
        }

        yield return new WaitForSeconds(1f);
        _characterMovement.IsInputDisabled = false;
    }


    private void KickOpponent(GameObject opponent)
    {
        var opponentType = base.GetCharacterType(opponent);
        if (opponentType == CharacterStats.CharacterType.None) return;
        var attackDirection = base.CalculateAttackDirection(opponent.transform);
        var opponentMovement = opponent.transform.GetComponentInParent<CharacterMovement>();
        if (opponentMovement == null) return;
        _characterStamina.DecreaseCharacterAttackStamina(10f);
        //opponentMovement.HandleKnockBackRPC(attackDirection);
    }

    #endregion

}
