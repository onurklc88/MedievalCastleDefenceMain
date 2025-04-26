using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;

public class GallowglassAttack : CharacterAttackBehaviour
{
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
        IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);

      
        if (wasBlocking && !IsPlayerBlockingLocal)
        {
            _blockReleaseCooldown = TickTimer.CreateFromSeconds(Runner, 0.5f);
        }

        if (!IsPlayerBlockingLocal) PlayerSwordPositionLocal = base.GetSwordPosition();
        if (_gallowGlassAnimation != null) BlockWeapon();

        
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner) && _bloodhandSkill.CanUseAbility && !IsPlayerBlocking && (_blockReleaseCooldown.ExpiredOrNotRunning(Runner)) && !_characterHealth.IsPlayerGotHit)
        {
            if (_characterStamina.CurrentAttackStamina > 30)
            {
               
                SwingSword();
              
            }
        }
        else if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UtilitySkill) && _bloodhandSkill.CanUseAbility)
        {
            //IsPlayerBlockingLocal = true;
           // _characterStamina.DecreaseDefenceStaminaRPC(60f);
           // transform.GetComponentInParent<BloodhandVFXController>().UpdateParryVFXRpc();
        }

        PreviousButton = input.NetworkButtons;
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
       AttackCooldown = TickTimer.CreateFromSeconds(Runner, 1f);
       _characterStamina.DecreaseCharacterAttackStamina(_weaponStats.StaminaWaste);
       _gallowGlassAnimation.UpdateAttackAnimState(((int)base.GetSwordPosition() == 0 ? 2 : (int)base.GetSwordPosition()));
        float swingTime = (base.GetSwordPosition() == SwordPosition.Right) ? 0.5f : 0.5f;
       
        StartCoroutine(PerformAttack(swingTime));

       
    }
    private IEnumerator PerformAttack(float time)
    {
        base._blockArea.enabled = false;
        _bloodhandVFX.ActivateAxeTrail(true);
        yield return new WaitForSeconds(0.24f);
        float elapsedTime = 0f;
        //AttackCooldown = TickTimer.CreateFromSeconds(Runner, 1.5f);

        while (elapsedTime < 0.5f)
        {

            Vector3 swingDirection = transform.position + transform.up * 1.2f + transform.forward * 1.2f + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);
            int layerMask = ~LayerMask.GetMask("Ragdoll");
            Collider[] hitColliders = Physics.OverlapSphere(swingDirection, 0.6f, layerMask);

            var target = hitColliders.FirstOrDefault(c => c.gameObject.layer == 10 || c.gameObject.layer == 11)
                         ?? hitColliders.FirstOrDefault();

            if (target != null)
            {
                CheckAttackCollision(target.transform.gameObject);
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        base._blockArea.enabled = true;
        _bloodhandVFX.ActivateAxeTrail(false);
    }

    public void KickAction()
    {
      
       _gallowGlassAnimation.UpdateJumpAnimationState(true);
       // StartCoroutine(PerformKickAction());
    }

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


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward * 1.2f + -transform.right * 0.3f, 0.6f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward * 1.2f + transform.right * 0.3f, 0.6f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);
    }
   
    
   
}
