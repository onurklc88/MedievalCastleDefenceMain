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
    private bool _isCharacterSpawned;
    
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
        if (_characterMovement != null && _characterMovement.IsPlayerStunned) return;
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
                ParryAttack();
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
        if (pressedButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Reload))
        {
           
            
            var test = Runner.GetPlayerObject(Runner.LocalPlayer);
           //IsPlayerParry = true;
            
            if(test != null)
            {
                
               
                _isCharacterSpawned = false;
                //EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer);
                base.OnObjectDestroy();
            }
            
        }
      

        PreviousButton = input.NetworkButtons;
    }
   private void ParryAttack()
    {

    } 
    private void CheckAttackCollision(GameObject collidedObject)
    {
        if (collidedObject.transform.GetComponentInParent<NetworkObject>() == null) return;
        if (collidedObject.transform.GetComponentInParent<NetworkObject>().Id == transform.GetComponentInParent<NetworkObject>().Id) return;
        //if (collidedObject.transform.gameObject.layer == 3 && !collidedObject.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking) return;
        
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
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    DamageToKnightCommander(collidedObject, _weaponStats.Damage);
                    break;

            }
        }
    }
    
    protected override void SwingSword()
    {
        if (IsPlayerBlockingLocal || !_characterController.isGrounded) return;
        _animation.UpdateSwingAnimationState(true);
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _characterStamina.DecreasePlayerStamina(_weaponStats.StaminaWaste);
        StartCoroutine(CollisionDelay(0.20f));
    }
    private IEnumerator CollisionDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Collider[] _hitColliders = Physics.OverlapSphere(transform.position + transform.up + transform.forward, 0.5f);
        for (int i = 0; i < _hitColliders.Length; i++)
        {
           CheckAttackCollision(_hitColliders[i].transform.gameObject);
        }
    }
   
    protected override void DamageToFootknight(GameObject opponent, float damageValue)
    {
        Debug.Log("damage to footnight");
        var dotValue = base.CalculateAttackPosition(opponent.transform);
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentParrying = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        if (opponent.gameObject.layer == 10 && !isOpponentParrying)
        {
            return;
        }
      

        if (opponent.gameObject.layer == 10 && isOpponentParrying)
        {
            opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry, CalculateAttackDirection(opponent.transform));
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
           opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry, CalculateAttackDirection(opponent.transform));
        }
        else
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward, 0.5f);
        //Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward / 0.94f, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }
}
