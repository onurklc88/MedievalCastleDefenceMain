using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : CharacterAttackBehaviour, IThrowable
{
    public bool IsBombThrown { get; set; }
    public Rigidbody ComponentRigidbody { get; set; }
    public bool IsObjectCollided { get; set; }

    //[SerializeField] protected Rigidbody _rigidbody;
    [SerializeField] protected WeaponStats _weapon;
    protected bool _isArrowCollided = false;
    protected Transform _parentTransform;
    [SerializeField] protected Transform _interpolationTarget;
    [SerializeField] protected BoxCollider _collison;

    public void InitOwnerStats(PlayerStatsController ownerInfo)
    {
       
    }
}
