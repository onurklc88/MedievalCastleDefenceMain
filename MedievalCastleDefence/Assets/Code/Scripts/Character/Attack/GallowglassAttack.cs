using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GallowglassAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private GallowglassAnimation _gallowGlassAnimation;
    private CharacterMovement _characterMovement;
    private Vector3 _halfExtens = new Vector3(1.7f, 0.9f, 0.5f);
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
        if (_characterMovement != null && _characterMovement.IsPlayerStunned)
        {
            //IsPlayerBlocking = false;
            //_gallowGlassAnimation.IsPlayerParry = IsPlayerBlocking;
            return;
        }

        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (!IsPlayerBlocking && _playerHUD != null) _playerHUD.HandleArrowImages(GetSwordPosition());
        IsPlayerBlockingLocal = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
       // IsPlayerBlockingLocal = true;
        if (!IsPlayerBlockingLocal) PlayerSwordPositionLocal = base.GetSwordPosition();
        if (_gallowGlassAnimation != null) BlockWeapon();
        
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Mouse0) && AttackCooldown.ExpiredOrNotRunning(Runner))
        {

            if (_characterStamina.CurrentStamina > 30)
            {
                SwingSword();
            }
        }
        else if(attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Jump) && _kickCooldown.ExpiredOrNotRunning(Runner) && input.HorizontalInput == 0 && input.VerticalInput >= 0)
        {
            _characterMovement.IsInputDisabled = true;
            KickAction();
        }

         if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Reload) && _kickCooldown.ExpiredOrNotRunning(Runner))
         {
           
         }
    }
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
    protected override void SwingSword()
    {
       if (IsPlayerBlockingLocal || !_characterMovement.IsPlayerGrounded()) return;
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _gallowGlassAnimation.UpdateAttackAnimState(((int)base.GetSwordPosition()));
        _characterStamina.DecreasePlayerStamina(_weaponStats.StaminaWaste);
        float swingTime = (base.GetSwordPosition() == SwordPosition.Right) ? 0.5f : 0.5f;
        StartCoroutine(PerformAttack(swingTime));
    }
    private IEnumerator PerformAttack(float time)
    {
        yield return new WaitForSeconds(time);
        HashSet<NetworkId> hitPlayers = new HashSet<NetworkId>();
      
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            Vector3 swingDirection = transform.position + transform.forward + Vector3.up + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.1f : -0.1f);
            Collider[] _hitColliders = Physics.OverlapBox(swingDirection, _halfExtens, Quaternion.Euler(0, transform.eulerAngles.y, 0));

            for (int i = 0; i < _hitColliders.Length; i++)
            {
                GameObject collidedObject = _hitColliders[i].transform.gameObject;
                NetworkObject networkObject = collidedObject.GetComponentInParent<NetworkObject>();
                if(networkObject != null && !hitPlayers.Contains(networkObject.Id))
                {
                    hitPlayers.Add(networkObject.Id);
                    CheckAttackCollision(_hitColliders[i].transform.gameObject);
                }
               
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void KickAction()
    {
        _kickCooldown = TickTimer.CreateFromSeconds(Runner, 2f);
        _gallowGlassAnimation.UpdateJumpAnimationState(true);
        StartCoroutine(PerformKickAction());
    }

    private IEnumerator PerformKickAction()
    {
        yield return new WaitForSeconds(0.65f);
      
        Vector3 kickPostion = transform.position + transform.up + transform.forward * 1.1f + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);
       
        Collider[] _hitColliders = Physics.OverlapSphere(kickPostion, 0.55f);
        for(int i = 0; i < _hitColliders.Length; i++)
        {
            if (_hitColliders[i].gameObject.layer != 10)
                KickOpponent(_hitColliders[i].gameObject);
        }
        yield return new WaitForSeconds(1f);
        _characterMovement.IsInputDisabled = false;
    }
    protected override void BlockWeapon()
    {
       _gallowGlassAnimation.UpdateBlockAnimState(IsPlayerBlocking ? (int)GetSwordPosition() : 0);
    }
    protected override void DamageToFootknight(GameObject opponent, float damageValue)
    {
        Debug.Log("damage to footnight");
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

    protected override void DamageToGallowGlass(GameObject opponent)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        var opponentSwordPosition = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().PlayerSwordPosition;
       
        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            Debug.Log("OpponentSwingPOs: " + opponentSwordPosition + " My sword position: " +PlayerSwordPositionLocal);
            if (opponentSwordPosition == PlayerSwordPositionLocal)
            {
                
                 opponentHealth.DealDamageRPC(_weaponStats.Damage);
            }
            else
            {
                Debug.Log("Blocked");
                opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
            }
           
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage);
        }
       
      
    }

    private void KickOpponent(GameObject opponent) 
    {
        var opponentType = base.GetCharacterType(opponent);
        if (opponentType == CharacterStats.CharacterType.None) return;
        var attackDirection = base.CalculateAttackDirection(opponent.transform);
        var opponentMovement = opponent.transform.GetComponentInParent<CharacterMovement>();
        if (opponentMovement == null) return;
        _characterStamina.DecreasePlayerStamina(10f);
        opponentMovement.HandleKnockBackRPC(attackDirection);
    }

   


    private void OnDrawGizmos()
    {

        Gizmos.color = Color.yellow;
        Vector3 gizmoPosition = transform.position + transform.forward * 0.82f + Vector3.up + transform.right * (GetSwordPosition() == SwordPosition.Right ? 0.3f : -0.3f);
        Gizmos.matrix = Matrix4x4.TRS(gizmoPosition, Quaternion.Euler(0, transform.eulerAngles.y, 0), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.7f, 0.9f, 0.5f));
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.matrix = Matrix4x4.identity; // Varsayýlan matris
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward * 1.1f + transform.right * 0.3f, 0.55f);
    }

}
