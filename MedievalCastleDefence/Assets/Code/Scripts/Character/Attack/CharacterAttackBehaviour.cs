using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class CharacterAttackBehaviour : BehaviourRegistry, IReadInput
{
    [Networked] protected TickTimer AttackCooldown { get; set; }
    [Networked] public NetworkButtons PreviousButton { get; set; }
    [Networked(OnChanged = nameof(OnNetworkBlockChanged))] public NetworkBool IsPlayerBlockingLocal { get; set; }
    [Networked(OnChanged = nameof(OnNetworkBlockPositionChanged))] public SwordPosition PlayerSwordPositionLocal { get; set; }
 
    
    [Networked] public NetworkBool IsPlayerBlocking { get; set; }
    [Networked] public SwordPosition PlayerSwordPosition { get; set; }
    public enum SwordPosition
    {
        None,
        Right,
        Left
    }
    public enum AttackDirection
    {
        None,
        Forward,
        FromRight,
        FromLeft,
        Backward
    }
    
    [SerializeField] protected WeaponStats _weaponStats;
    [SerializeField] protected BoxCollider _blockArea;
    protected CharacterController _characterController;
    protected IDamageable _collidedObject;
    public CharacterStats.CharacterType _characterType;
    protected CharacterStamina _characterStamina;
    private SwordPosition _lastSwordPosition;
    private float _movementTreshold = 0.25f;
    
    public virtual void ReadPlayerInputs(PlayerInputData input) { }
    protected virtual void AttackCollision() { }
    protected virtual void SwingSword() { }
    protected virtual void BlockWeapon() { }
    protected virtual void DamageToFootknight(GameObject opponent, float damageValue) { }
    protected virtual void DamageToKnightCommander(GameObject opponent, float damageValue) { }
    protected virtual void DamageToGallowGlass(GameObject opponent) { }
    protected virtual void DamageToRanger(GameObject opponent) { }

    private static void OnNetworkBlockChanged(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour.IsPlayerBlocking = changed.Behaviour.IsPlayerBlockingLocal;
        if (changed.Behaviour._characterStats.WarriorType == CharacterStats.CharacterType.FootKnight) return;
        changed.Behaviour._blockArea.enabled = changed.Behaviour.IsPlayerBlockingLocal;
     
    }

    private static void OnNetworkBlockPositionChanged(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour.PlayerSwordPosition = changed.Behaviour.PlayerSwordPositionLocal;
    }

    protected void CheckAttackCollisionTest(GameObject collidedObject)
    {

        if (collidedObject.transform.GetComponentInParent<NetworkObject>() == null) return;
        if (collidedObject.transform.GetComponentInParent<NetworkObject>().Id == transform.GetComponentInParent<NetworkObject>().Id) return;
        if (collidedObject.transform.GetComponentInParent<IDamageable>() != null)
        {
            var opponentType = GetCharacterType(collidedObject);
            switch (opponentType)
            {
                case CharacterStats.CharacterType.FootKnight:
                    DamageToFootknightTest(collidedObject);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    DamageToGallowGlassTest(collidedObject);
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    DamageToKnightCommanderTest(collidedObject);
                    break;
                case CharacterStats.CharacterType.Ranger:
                    var opponentHealth = collidedObject.transform.GetComponentInParent<CharacterHealth>();
                    opponentHealth.DealDamageRPC(_weaponStats.Damage);
                    break;
            }
        }
    }

    protected void DamageToFootknightTest(GameObject opponent)
    {
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
            opponentHealth.DealDamageRPC(_weaponStats.Damage);
        }
    }

    protected void DamageToKnightCommanderTest(GameObject opponent)
    {
        var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = opponent.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
        var opponentSwordPosition = opponent.transform.GetComponentInParent<CharacterAttackBehaviour>().PlayerSwordPosition;
        Debug.Log("Ýs OpponentBlocking: " + isOpponentBlocking + " oppponent block area active?: " + opponent.transform.GetComponentInParent<CharacterAttackBehaviour>()._blockArea.enabled.ToString() + " oppponent sword position " + opponentSwordPosition);


        if (opponent.gameObject.layer == 10 && isOpponentBlocking)
        {
            opponentStamina.DecreaseStaminaRPC(_weaponStats.WeaponStaminaReductionOnParry);
        }
        else
        {
            opponentHealth.DealDamageRPC(_weaponStats.Damage);
        }
    }

    protected void DamageToGallowGlassTest(GameObject opponent)
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
    protected SwordPosition GetSwordPosition() 
    {
       float mouseX = Input.GetAxis("Mouse X");
       _lastSwordPosition = mouseX > _movementTreshold ? SwordPosition.Right : mouseX < -_movementTreshold ? SwordPosition.Left :_lastSwordPosition;
        return _lastSwordPosition;
    }
  
    protected float CalculateAttackPosition(Transform defenderPosition)
    {
        Vector3 directionToTarget = Vector3.Normalize(transform.position - defenderPosition.transform.position);
        var value = Vector3.Dot(defenderPosition.transform.forward, directionToTarget);
        return value;
    }

    protected bool IsSwordHitShield()
    {
        Ray shieldRay = new Ray(transform.position + transform.up * 1.2f, transform.forward * 1.5f);
        return Physics.Raycast(shieldRay, out RaycastHit hit, 5f) && hit.collider.transform.gameObject.layer == 3;
    }

    protected CharacterStats.CharacterType GetCharacterType(GameObject collidedObject)
    {   
        if (collidedObject.transform.GetComponentInParent<PlayerStatsController>() != null)
        {
            var character = collidedObject.transform.GetComponentInParent<PlayerStatsController>();
            return character.PlayerNetworkStats.PlayerWarrior;
        }
        else
        {
           return CharacterStats.CharacterType.None;
        }
    }

    protected AttackDirection CalculateAttackDirection(Transform defenderPosition)
    {
        Vector3 directionToTarget = Vector3.Normalize(transform.position - defenderPosition.position);
        float forwardDot = Vector3.Dot(defenderPosition.forward, directionToTarget);
        Vector3 crossProduct = Vector3.Cross(defenderPosition.forward, directionToTarget);
        float forwardThreshold = 0.7f;
        float backwardThreshold = -0.7f;
        float sideThreshold = 0.3f;
         return (forwardDot > forwardThreshold) ? AttackDirection.Forward :
       (forwardDot < backwardThreshold) ? AttackDirection.Backward :
       (crossProduct.y > sideThreshold) ? AttackDirection.FromLeft :
       (crossProduct.y < -sideThreshold) ? AttackDirection.FromRight :
       AttackDirection.None;
    }


}
