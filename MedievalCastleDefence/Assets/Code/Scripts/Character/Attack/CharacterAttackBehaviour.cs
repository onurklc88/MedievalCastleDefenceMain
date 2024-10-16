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
    [SerializeField] protected LayerMask _colliders;
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
    protected virtual void SwingSwordRight() { }
    protected virtual void SwingSwordLeft() { }
    protected virtual void SwingSword() { }
    protected virtual void BlockWeapon() { }
    protected virtual void DamageToFootknight(GameObject opponent, float damageValue) { }
    protected virtual void DamageToKnightCommander(GameObject opponent, float damageValue) { }

    private static void OnNetworkBlockChanged(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour.IsPlayerBlocking = changed.Behaviour.IsPlayerBlockingLocal;
        changed.Behaviour._blockArea.enabled = changed.Behaviour.IsPlayerBlockingLocal;
    }

    private static void OnNetworkBlockPositionChanged(Changed<CharacterAttackBehaviour> changed)
    {
        changed.Behaviour.PlayerSwordPosition = changed.Behaviour.PlayerSwordPositionLocal;
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
            return character.SelectedCharacter;
        }
        else
        {
           return CharacterStats.CharacterType.None;
        }
    }
   
}
