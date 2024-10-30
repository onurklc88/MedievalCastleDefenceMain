using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class KnightCommanderAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private KnightCommanderAnimation _knightCommanderAnimation;
    private CharacterMovement _characterMovement;
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
        if (!IsPlayerBlocking && _playerHUD != null) _playerHUD.HandleArrowImages(GetSwordPosition());
        IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        //IsPlayerBlockingLocal = true;
        if(_knightCommanderAnimation != null) BlockWeapon();

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {

            if (_characterStamina.CurrentStamina > 30)
            {
                SwingSword();
            }
        }
        
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
            Collider[] _hitColliders = Physics.OverlapSphere(swingDirection, 0.5f);
           
            if (_hitColliders.Length > 0)
            {
               
                CheckAttackCollision(_hitColliders[0].transform.gameObject);
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
        Debug.Log("damage to knightCommander");
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
       
        if(opponent.gameObject.layer == 10 && isOpponentBlocking)
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
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + -transform.right * 0.3f, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + transform.right * 0.3f, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }

}
