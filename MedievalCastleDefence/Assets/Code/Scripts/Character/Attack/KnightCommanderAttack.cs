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
    private ActiveRagdoll _ragdollManager;
    private PlayerVFXSytem _playerVFXSystem;
   

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
        _ragdollManager = GetScript<ActiveRagdoll>();
        _playerVFXSystem = GetScript<PlayerVFXSytem>();
        _playerStatsController = GetScript<PlayerStatsController>();
        _characterHealth = GetScript<CharacterHealth>();
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
            _knightCommanderAnimation.IsPlayerParry = IsPlayerBlocking;
            return; 
        }
        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (!IsPlayerBlocking && _playerHUD != null) _playerHUD.HandleArrowImages(GetSwordPosition());
        IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        //IsPlayerBlockingLocal = true;
        if (!IsPlayerBlockingLocal) PlayerSwordPositionLocal = base.GetSwordPosition();
        if (_knightCommanderAnimation != null) BlockWeapon();

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner) && !_characterHealth.IsPlayerGotHit)
        {

            if (_characterStamina.CurrentAttackStamina > 30)
            {
                SwingSword();
            }
        }

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {
            //IsPlayerBlockingLocal = true;
            //_ragdollManager.RPCActivateRagdoll();
        }
        PreviousButton = input.NetworkButtons;
    }
    protected override void SwingSword()
    {
        if (IsPlayerBlockingLocal || !_characterMovement.IsPlayerGrounded()) return;
        //_playerVFXSystem.EnableWeaponParticles();
        if(_knightCommanderAnimation == null)
        {
            Debug.Log("is null? :");
        }
        Debug.Log("test1-2");
        _knightCommanderAnimation.UpdateAttackAnimState(((int)base.GetSwordPosition() == 0 ? 2 : (int)base.GetSwordPosition()));
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _characterStamina.DecreaseCharacterAttackStamina(_weaponStats.StaminaWaste);
         StartCoroutine(PerformAttack());
    }
    private IEnumerator PerformAttack()
    {
        _playerVFXSystem.ActivateSwordTrail(true);
        yield return new WaitForSeconds(0.27f);
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
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

        yield return new WaitForSeconds(1f);
        _playerVFXSystem.ActivateSwordTrail(false);
    }
    protected override void BlockWeapon()
    {
        _knightCommanderAnimation.UpdateBlockAnimState(IsPlayerBlocking ? (int)GetSwordPosition() : 0);
        //_debugger.ShowDebugMessageRPC("Is Block area active: " + base._blockArea.enabled.ToString());
        //Debug.Log("IsplayerBlocking: " + IsPlayerBlocking + " Sword pos: " + PlayerSwordPositionLocal);
    }
  
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward + -transform.right * 0.3f, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up * 1.2f + transform.forward + transform.right * 0.3f, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }


  
}
