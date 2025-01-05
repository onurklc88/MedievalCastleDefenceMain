using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class FootKnightAttack : CharacterAttackBehaviour
{
    [SerializeField] private RPCDebugger _rpcdebugger;
    private FootknightAnimation _animation;
    private CharacterMovement _characterMovement;
    private ActiveRagdoll _activeRagdoll;
    private PlayerVFXSytem _playerVFX;
  
    
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
        _activeRagdoll = GetScript<ActiveRagdoll>();
        _playerVFX = GetScript<PlayerVFXSytem>();
        _playerStatsController = GetScript<PlayerStatsController>();
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
        //IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
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


        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Reload) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {
            IsPlayerBlockingLocal = true;
            //_activeRagdoll.RPCActivateRagdoll();
        }

        PreviousButton = input.NetworkButtons;
    }
    
    protected override void SwingSword()
    {
        if (IsPlayerBlockingLocal || !_characterMovement.IsPlayerGrounded()) return;
        _playerVFX.EnableWeaponParticles();
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
            Vector3 swingDirection = transform.position + transform.up * 1.2f + transform.forward + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);
            int layerMask = ~LayerMask.GetMask("Ragdoll");
            Collider[] _hitColliders = Physics.OverlapSphere(swingDirection, 0.5f, layerMask);

            var target = _hitColliders.FirstOrDefault(c => c.gameObject.layer == 10 || c.gameObject.layer == 11)
                         ?? _hitColliders.FirstOrDefault();

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
