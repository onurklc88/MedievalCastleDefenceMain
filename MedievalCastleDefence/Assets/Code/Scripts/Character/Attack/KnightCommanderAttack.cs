using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class KnightCommanderAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private KnightCommanderAnimation _knightCommanderAnimation;
  
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
        if (collidedObject.transform.gameObject.layer == 3 && !collidedObject.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking) return;

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
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            Collider[] _hitColliders = Physics.OverlapSphere(transform.position + transform.up + transform.forward + transform.right * 0.2f, 0.5f);
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


        if (dotValue > 0 && isOpponentParrying)
        {
            opponentStamina.DecreaseStaminaRPC(20f);

        }
        else if (dotValue >= 0 && !isOpponentParrying && !IsSwordHitShield())
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
        else if (dotValue < -0.3f)
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
    }

    protected override void DamageToKnightCommander(GameObject opponent, float damageValue)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
       
        if(opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            //Debug.Log("Block");
            opponentStamina.DecreaseStaminaRPC(50f);
        }
        else
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + -transform.right * 0.2f, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + transform.right * 0.2f, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }

}
