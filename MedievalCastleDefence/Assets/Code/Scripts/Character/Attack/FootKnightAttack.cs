using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class FootKnightAttack : CharacterAttackBehaviour
{
    [SerializeField] private RPCDebugger _rpcdebugger;
    [SerializeField] private Rigidbody _shieldRigidbody;
    private FootknightAnimation _animation;
    private CharacterMovement _characterMovement;
    
   
  
    
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.FootKnight;
        _shieldRigidbody.mass = 0;
        InitScript(this);
       _animation = transform.GetComponent<FootknightAnimation>();
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
        IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        //IsPlayerBlockingLocal = true;
        if (_animation != null)
            _animation.IsPlayerParry = IsPlayerBlockingLocal;

        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (IsPlayerBlockingLocal)
        {
            //_characterMovement.CurrentMoveSpeed = _characterStats.MoveSpeed;

            if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && AttackCooldown.ExpiredOrNotRunning(Runner) && !_characterHealth.IsPlayerGotHit)
            {
                //ParryAttack();

            }
           
        }
        else if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner) && !_characterHealth.IsPlayerGotHit)
        {
           
            if (_characterStamina.CurrentAttackStamina > _weaponStats.StaminaWaste)
            {
                SwingSword();
            }
        }
        else
        {
            if(_characterMovement != null && !_characterMovement.IsPlayerSlowed)
            {
                _characterMovement.CurrentMoveSpeed = _characterStats.SprintSpeed;
            }

        }


        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UtilitySkill) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {
            //_characterStamina.DecreaseDefenceStaminaRPC(50f);
            //transform.GetComponentInParent<StormshieldVFXController>().UpdateParryVFXRpc();
            //IsPlayerBlockingLocal = true;
            //_activeRagdoll.RPCActivateRagdoll();
        }

        

        PreviousButton = input.NetworkButtons;
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
   
    private IEnumerator PerformAttack()
    {

       _blockArea.enabled = false;
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
       _blockArea.enabled = true;
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
