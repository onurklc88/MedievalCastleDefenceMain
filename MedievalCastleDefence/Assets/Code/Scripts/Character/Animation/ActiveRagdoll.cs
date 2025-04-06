using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

public class ActiveRagdoll : CharacterRegistry
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Animator _animator;
    [Networked(OnChanged = nameof(DisableRagdollStateOnChange))] public NetworkBool IsGameStart { get; set; }
    public Collider[] _ragdollColliders;
    public Rigidbody[] _limbsRigidbodies;
    

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        //IsGameStart = true;
    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        GetRagdollBits();
        RPCDisableRagdoll();
    }

    private void GetRagdollBits()
    {
        if (!Object.HasStateAuthority) return;
       _ragdollColliders = GetComponentsInChildren<Collider>();
       _limbsRigidbodies = GetComponentsInChildren<Rigidbody>();

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
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
        _characterController.enabled = false;
        ApplyForce();

    }

    private static void DisableRagdollStateOnChange(Changed<ActiveRagdoll> changed)
    {
        
        if (changed.Behaviour._ragdollColliders == null || changed.Behaviour._limbsRigidbodies == null)
        {
            return;
        }

        foreach (Collider col in changed.Behaviour._ragdollColliders)
        {

            if (col.gameObject.GetComponent<Rigidbody>() != null && col.gameObject.layer != 10)
            {
                col.enabled = false;

            }
        }

        foreach (Rigidbody rb in changed.Behaviour._limbsRigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPCDisableRagdoll()
    {
        if (_ragdollColliders == null|| _limbsRigidbodies == null)
        {
           return;
        }
      
        foreach (Collider col in _ragdollColliders)
        {
           
            if (col.gameObject.GetComponent<Rigidbody>() != null && col.gameObject.layer != 10)
            {
                col.enabled = false;
               
            }
        }

        foreach (Rigidbody rb in _limbsRigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    private void ApplyForce()
    {
        Vector3 attackDirection = new Vector3(0f, 0f, 1f);
        float forceMagnitude = 10f; 
        foreach (Rigidbody rb in _limbsRigidbodies)
        {
            rb.AddForce(attackDirection * forceMagnitude, ForceMode.Impulse);
        }
    }

    private IEnumerator Test()
    {
        yield return new WaitForSeconds(1f);
        
    }
}
