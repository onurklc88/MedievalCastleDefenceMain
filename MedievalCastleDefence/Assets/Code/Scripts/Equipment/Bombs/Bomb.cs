using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bomb : CharacterAttackBehaviour
{
    public bool IsBombThrown { get; set; }
    
    public bool IsObjectCollided { get; set; }
    public NetworkId OwnerID { get; set; }
    public Rigidbody Rigidbody;
    [SerializeField] protected WeaponStats _weapon;
    protected bool _isArrowCollided = false;
    protected Transform _parentTransform;
    [SerializeField] protected Transform _interpolationTarget;
    [SerializeField] protected Collider _collison;
   
   
}
