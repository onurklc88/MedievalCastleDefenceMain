using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class KnightCommanderAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private KnightCommanderAnimation _knightCommanderAnimation;
    private CharacterMovement _characterMovement;
    private ActiveRagdoll _ragdollManager;
    [SerializeField] private GameObject test;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.FootKnight;
        InitScript(this);
    }
    private void Start()
    {
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
        _knightCommanderAnimation = GetScript<KnightCommanderAnimation>();
        _characterMovement = GetScript<CharacterMovement>();
        _ragdollManager = GetScript<ActiveRagdoll>();
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
            _knightCommanderAnimation.IsPlayerParry = IsPlayerBlocking;
            return; 
        }
        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (!IsPlayerBlocking && _playerHUD != null) _playerHUD.HandleArrowImages(GetSwordPosition());
        IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        //IsPlayerBlockingLocal = true;
        if (!IsPlayerBlockingLocal) PlayerSwordPositionLocal = base.GetSwordPosition();
        if (_knightCommanderAnimation != null) BlockWeapon();

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {

            if (_characterStamina.CurrentStamina > 30)
            {
                SwingSword();
            }
        }

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Reload) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {
            _ragdollManager.RPCActivateRagdoll();
        }
    }
    protected override void SwingSword()
    {
        if (IsPlayerBlockingLocal || !_characterMovement.IsPlayerGrounded()) return;
        _knightCommanderAnimation.UpdateAttackAnimState(((int)base.GetSwordPosition()));
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _characterStamina.DecreasePlayerStamina(_weaponStats.StaminaWaste);
        StartCoroutine(PerformAttack());
    }
    private IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(0.27f);
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
           
            Vector3 swingDirection = transform.position + transform.up + transform.forward + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);
            int layerMask = ~LayerMask.GetMask("Ragdoll");
            Collider[] _hitColliders = Physics.OverlapSphere(swingDirection, 0.5f, layerMask);
           
            if (_hitColliders.Length > 0)
            {
                Debug.Log("Collided Object: " + _hitColliders[0].transform.gameObject.name + "PlayerID: " + _hitColliders[0].transform.GetComponentInParent<NetworkObject>().Id);
                CheckAttackCollisionTest(_hitColliders[0].transform.gameObject);
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null; 
        }
    }
    protected override void BlockWeapon()
    {
        _knightCommanderAnimation.UpdateBlockAnimState(IsPlayerBlocking ? (int)GetSwordPosition() : 0);
    }
  
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + -transform.right * 0.3f, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + transform.right * 0.3f, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }

}
