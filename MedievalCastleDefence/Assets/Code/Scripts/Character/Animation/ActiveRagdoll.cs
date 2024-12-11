using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ActiveRagdoll : BehaviourRegistry
{
    [SerializeField] private CharacterController _charcterController;
    [SerializeField] private Animator _animator;
    [SerializeField] private NetworkTransform _networkTransform;
    private Collider[] _ragdollColliders;
    private Rigidbody[] _limbsRigidbodies;
    

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    private void Start()
    {
        GetRagdollBits();
        RPCDisableRagdoll();
    }
    private void GetRagdollBits()
    {
        _ragdollColliders = GetComponentsInChildren<CapsuleCollider>();
        _limbsRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCActivateRagdoll()
    {
       
        foreach (Collider col in _ragdollColliders)
        {
            col.enabled = true;
        }

        foreach (Rigidbody rb in _limbsRigidbodies)
        {
            rb.isKinematic = false;
        }

        _animator.enabled = false;
        ApplyForce();

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPCDisableRagdoll()
    {
        foreach (Collider rb in _ragdollColliders)
        {
            rb.enabled = false;
            
        }

        foreach (Rigidbody rb in _limbsRigidbodies)
        {
            rb.isKinematic = true;
        }


    }

    private void ApplyForce()
    {
        Vector3 attackDirection = new Vector3(-1f, 0f, 0f);
        float forceMagnitude = 10f; 
        foreach (Rigidbody rb in _limbsRigidbodies)
        {
            rb.AddForce(attackDirection * forceMagnitude, ForceMode.Impulse);
        }
    }

}
