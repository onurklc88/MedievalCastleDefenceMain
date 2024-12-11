using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class FootKnightAttack : CharacterAttackBehaviour
{
    [SerializeField] private RPCDebugger _rpcdebugger;
    private FootknightAnimation _animation;
    private CharacterMovement _characterMovement;
  
    
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.FootKnight;
        InitScript(this);
        
        _animation = transform.GetComponent<FootknightAnimation>();
        
    }
    private void Start()
    {
        _characterMovement = GetScript<CharacterMovement>();
        _characterStamina = GetScript<CharacterStamina>();
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
        if (_characterMovement != null && _characterMovement.IsPlayerStunned)
        {
            IsPlayerBlockingLocal = false;
            _animation.IsPlayerParry = IsPlayerBlockingLocal;
            return;
        }
        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        //IsPlayerBlockingLocal = true;
        if (_animation != null)
            _animation.IsPlayerParry = IsPlayerBlockingLocal;

        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (IsPlayerBlockingLocal)
        {
            _characterMovement.CurrentMoveSpeed = _characterStats.MoveSpeed;

            if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Skill) && AttackCooldown.ExpiredOrNotRunning(Runner))
            {
                //ParryAttack();
            }
           
        }
        else if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {
           
            if (_characterStamina.CurrentStamina > 30)
            {
                SwingSword();
            }
        }
        else
        {
            if(_characterMovement != null)
                _characterMovement.CurrentMoveSpeed = _characterStats.SprintSpeed;
        }
       
        PreviousButton = input.NetworkButtons;
    }
    /*
    private void CheckAttackCollision(GameObject collidedObject)
    {
        if (collidedObject.transform.GetComponentInParent<NetworkObject>() == null) return;
        if (collidedObject.transform.GetComponentInParent<NetworkObject>().Id == transform.GetComponentInParent<NetworkObject>().Id) return;
       
        if (collidedObject.transform.GetComponentInParent<IDamageable>() != null)
        {
            var opponentType = base.GetCharacterType(collidedObject);
           
            switch (opponentType)
            {
                case CharacterStats.CharacterType.None:
                   break;
                case CharacterStats.CharacterType.FootKnight:
                   DamageToFootknight(collidedObject, _weaponStats.Damage);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    DamageToGallowGlass(collidedObject);
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    DamageToKnightCommander(collidedObject, _weaponStats.Damage);
                    break;
                case CharacterStats.CharacterType.Ranger:
                    var opponentHealth = collidedObject.transform.GetComponentInParent<CharacterHealth>();
                    opponentHealth.DealDamageRPC(_weaponStats.Damage);
                    break;

            }
        }
    }
    */
    protected override void SwingSword()
    {
        if (IsPlayerBlockingLocal || !_characterMovement.IsPlayerGrounded()) return;
        _animation.UpdateSwingAnimationState(true);
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _characterStamina.DecreasePlayerStamina(_weaponStats.StaminaWaste);
        StartCoroutine(PerformAttack());
    }
   
    private IEnumerator PerformAttack()
    {
        base._blockArea.enabled = false;
        yield return new WaitForSeconds(0.20f);
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            Collider[] _hitColliders = Physics.OverlapSphere(transform.position + transform.up + transform.forward, 0.5f);

            if (_hitColliders.Length > 0)
            {

                CheckAttackCollisionTest(_hitColliders[0].transform.gameObject);
                yield break;
            }
            base._blockArea.enabled = true;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    /*
    protected override void DamageToFootknight(GameObject opponent, float damageValue)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentParrying = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
      
        if (opponent.gameObject.layer == 11 && !isOpponentParrying)
        {
           return;
        }
        
        if (opponent.gameObject.layer == 11 && isOpponentParrying)
        {
           opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
           opponentHealth.DealDamageRPC(damageValue);
        }
    }
    protected override void DamageToKnightCommander(GameObject opponent, float damageValue)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;

        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
    }

    protected override void DamageToGallowGlass(GameObject opponent)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        var opponentSwordPosition = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().PlayerSwordPosition;

        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            if (opponentSwordPosition == PlayerSwordPositionLocal)
            {
                opponentHealth.DealDamageRPC(_weaponStats.Damage);
            }
            else
            {
                opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
            }

        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage);
        }


    }
    */
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward, 0.5f);
        //Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward / 0.94f, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }
}
