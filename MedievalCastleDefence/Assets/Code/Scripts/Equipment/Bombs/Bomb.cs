using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bomb : CharacterAttackBehaviour
{
    public bool IsBombThrown { get; set; }
    [SerializeField] protected ParticleSystem _bombEffect;


    [Networked(OnChanged = nameof(OnBombStateChange))] public NetworkBool IsBombReadyToExplode { get; set; }

    [Networked]
    public NetworkBool IsObjectCollided { get; set; }
    public NetworkId OwnerID { get; set; }
    public Rigidbody Rigidbody;
    [SerializeField] protected WeaponStats _weapon;
    protected bool _isArrowCollided = false;
    protected Transform _parentTransform;
    [SerializeField] protected Transform _interpolationTarget;
    [SerializeField] protected Collider _collison;

    private static void OnBombStateChange(Changed<Bomb> changed)
    {
        changed.Behaviour._bombEffect.Play();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    protected void RPC_SetEffectPosition(Vector3 pos)
    {
        _bombEffect.transform.position = pos;
    }
}
