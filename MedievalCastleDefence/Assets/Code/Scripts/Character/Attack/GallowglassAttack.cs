using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GallowglassAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private GallowglassAnimation _gallowGlassAnimation;
    private CharacterMovement _characterMovement;
    private float _kickTimer = 1.5f;
    private Vector3 _halfExtens = new Vector3(1.7f, 0.9f, 0.7f);
    [Networked] private TickTimer _kickCooldown { get; set; }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.Gallowglass;
        InitScript(this);
    }
    private void Start()
    {
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
        _characterMovement = GetScript<CharacterMovement>();
        _gallowGlassAnimation = GetScript<GallowglassAnimation>();
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
        //IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        //IsPlayerBlockingLocal = true;
       // if (_knightCommanderAnimation != null) BlockWeapon();

        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {

            if (_characterStamina.CurrentStamina > 30)
            {
                SwingSword();
            }
        }
        if(attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Jump) && _kickCooldown.ExpiredOrNotRunning(Runner) && input.HorizontalInput == 0 && input.VerticalInput >= 0)
        {
            _characterMovement.IsInputDisabled = true;
            KickAction();
        }
        if(attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Reload))
        {
           
           StartCoroutine(_characterMovement.KnockbackPlayer(AttackDirection.Forward));
            //_gallowGlassAnimation.UpdateStunAnimationState();
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
                    DamageToGallowGlass(collidedObject);
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
       // _knightCommanderAnimation.UpdateAttackAnimState(((int)base.GetSwordPosition()));
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
            Collider[] _hitColliders = Physics.OverlapBox(swingDirection, _halfExtens, Quaternion.identity);
         
            for (int i = 0; i < _hitColliders.Length; i++)
            {
                CheckAttackCollision(_hitColliders[i].transform.gameObject);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void KickAction()
    {
        _kickCooldown = TickTimer.CreateFromSeconds(Runner, 1f);
        _gallowGlassAnimation.UpdateJumpAnimationState(true);
        StartCoroutine(PerformKickAction());
    }

    private IEnumerator PerformKickAction()
    {
        yield return new WaitForSeconds(0.5f);
        Vector3 kickPostion = transform.position + transform.up + transform.forward + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);
        Collider[] _hitColliders = Physics.OverlapSphere(kickPostion, 0.5f);
        if(_hitColliders.Length > 0)
        {
            KickOpponent(_hitColliders[0].gameObject);
        }
       
        yield return new WaitForSeconds(1f);
        _characterMovement.IsInputDisabled = false;
    }
    protected override void DamageToFootknight(GameObject opponent, float damageValue)
    {
        Debug.Log("damage to footnight");
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

        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            //Debug.Log("Block");
            opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry, CalculateAttackDirection(opponent.transform));
        }
        else
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
    }

    protected override void DamageToGallowGlass(GameObject opponent)
    {
       // Debug.Log("damage to gallowglass");
        var dotValue = base.CalculateAttackPosition(opponent.transform);
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;

        Debug.Log("DotValue: " + dotValue);
    }

    private void KickOpponent(GameObject opponent) 
    {
        var opponentType = base.GetCharacterType(opponent);
        Debug.Log("A");
        if (opponentType == CharacterStats.CharacterType.None) return;
        Debug.Log("B");
        var attackDirection = base.CalculateAttackDirection(opponent.transform);
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        if (opponentStamina == null) return;
        opponentStamina.DecreaseStaminaRPC(10f, attackDirection);


    }

   


    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCube(transform.position + Vector3.up + Vector3.forward + transform.right * 0.3f, new Vector3(1.7f, 0.9f, 0.7f));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + transform.right * 0.3f, 0.3f);
      

    }

}
