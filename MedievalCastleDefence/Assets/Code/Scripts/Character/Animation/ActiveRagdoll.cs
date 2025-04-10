using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

public class ActiveRagdoll : CharacterRegistry
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Animator _animator;
    
   
    public Collider[] _ragdollColliders;
    public Rigidbody[] _limbsRigidbodies;

    public override void Spawned()
    {
        GetRagdollBits();
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        RPCDisableRagdoll();
    }

    private void GetRagdollBits()
    {
        List<Collider> ragdollCols = new List<Collider>();
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            if (col.gameObject == this.gameObject) continue;
            if (col.GetComponent<CharacterController>() != null) continue;
            ragdollCols.Add(col);
        }
        _ragdollColliders = ragdollCols.ToArray();
        _limbsRigidbodies = GetComponentsInChildren<Rigidbody>(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCActivateRagdoll()
    {
        foreach (var col in _ragdollColliders)
            col.enabled = true;

        foreach (var rb in _limbsRigidbodies)
            rb.isKinematic = false;

        _animator.enabled = false;
       
        ApplyForce();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCDisableRagdoll()
    {
        if (_ragdollColliders == null || _limbsRigidbodies == null)
        {
            Debug.LogWarning("Ragdoll parts missing on: " + Object.InputAuthority);
            return;
        }

        foreach (var col in _ragdollColliders)
        {
            int layer = col.gameObject.layer;
            if (layer != 10 && layer != 6 && layer != 11)
                col.enabled = false;
        }

        foreach (var rb in _limbsRigidbodies)
        {
            if (rb.gameObject.layer != 6 && rb.gameObject.layer != 11)
                rb.isKinematic = true;
        }
    }

    private void ApplyForce()
    {
        Vector3 attackDirection = new Vector3(0f, 0f, 1f);
        float forceMagnitude = 10f;

        foreach (var rb in _limbsRigidbodies)
            rb.AddForce(attackDirection * forceMagnitude, ForceMode.Impulse);
    }

    private IEnumerator Test()
    {
        yield return new WaitForSeconds(1f);
    }
}