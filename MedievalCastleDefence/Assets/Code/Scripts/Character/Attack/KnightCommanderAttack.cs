using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class KnightCommanderAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private float _lastPosX;
    private float _currentPosX;
    private KnightCommanderAnimation _knightCommanderAnimation;
  
    private void Awake()
    {
        _lastPosX = Input.GetAxis("Mouse X");
    }
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
                if (base.GetSwordPosition() == SwordPosition.Left)
                    SwingSwordLeft();
                else
                    SwingSwordRight();
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

    protected override void SwingSwordRight()
    {
        //Debug.Log("SwingSwordRight");
        if (IsPlayerBlockingLocal || !_characterController.isGrounded) return;
        _knightCommanderAnimation.UpdateAttackAnimState(1);
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _characterStamina.DecreasePlayerStamina(_weaponStats.StaminaWaste);
        StartCoroutine(CollisionDelay(0.1f, transform.position + transform.up + transform.forward + transform.right * 0.2f));
    }
    protected override void SwingSwordLeft()
    {
       // Debug.Log("SwingSwordLeft");
        if (IsPlayerBlockingLocal || !_characterController.isGrounded) return;
        _knightCommanderAnimation.UpdateAttackAnimState(2);
        AttackCooldown = TickTimer.CreateFromSeconds(Runner, _weaponStats.TimeBetweenSwings);
        _characterStamina.DecreasePlayerStamina(_weaponStats.StaminaWaste);
        StartCoroutine(CollisionDelay(0.5f, transform.position + transform.up + transform.forward + -transform.right * 0.2f));
    }
    private IEnumerator CollisionDelay(float seconds, Vector3 swingPos)
    {
        yield return new WaitForSeconds(seconds);
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f)
        {
            Collider[] _hitColliders = Physics.OverlapSphere(transform.position + transform.up + transform.forward + transform.right * 0.2f, 0.5f);
            if (_hitColliders.Length > 0)
            {
                CheckAttackCollision(_hitColliders[0].transform.gameObject);
                yield break; // Loop'u bitirir.
            }

            elapsedTime += Time.deltaTime;
            yield return null; // Bir sonraki frame'e kadar bekle.
        }
        /*
        Collider[] _hitColliders = Physics.OverlapSphere(swingPos, 0.5f);
        if (_hitColliders.Length > 0)
        {
            CheckAttackCollision(_hitColliders[0].transform.gameObject);
        }
        */
    }

    public void Test()
    {
        /*
        Debug.Log("Test");
        Collider[] _hitColliders = Physics.OverlapSphere(transform.position + transform.up + transform.forward + transform.right * 0.2f, 0.5f);
        if (_hitColliders.Length > 0)
        {
            CheckAttackCollision(_hitColliders[0].transform.gameObject);
        }
        */
    }
    protected override void BlockWeapon()
    {
       
        if (!IsPlayerBlocking)
        {
            _knightCommanderAnimation.UpdateBlockAnimState(0);
        }
        else
        {
            _knightCommanderAnimation.UpdateBlockAnimState(((int)GetSwordPosition()));
        }
      
    }
    protected override void DamageToFootknight(GameObject opponent, float damageValue)
    {
        Debug.Log("damage To footnight");
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
        /*
        if (dotValue > 0.39f && isOpponentBlocking)
        {
            var opponentSwordPos = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().PlayerSwordPosition;
           // Debug.Log("OppenentSwordPos: " + opponentSwordPos+" PlayerSwordPos: " +PlayerSwordPositionLocal);
            
            if (opponentSwordPos == GetSwordPosition())
            {
               opponentHealth.DealDamageRPC(damageValue);
            }
            else if(GetSwordPosition() != SwordPosition.None && opponentSwordPos != GetSwordPosition())
            {
                opponentStamina.DecreaseStaminaRPC(40f);
            }


        }
        else
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
        */
        /*
        else if (dotValue >= 0 && !isOpponentParrying)
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
        else if (dotValue < -0.3f)
        {
            opponentHealth.DealDamageRPC(damageValue);
        }
        */
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + -transform.right * 0.2f, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward + transform.right * 0.2f, 0.5f);
        //Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward / 0.94f, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 1.2f, transform.forward * 1.5f);

    }

}
