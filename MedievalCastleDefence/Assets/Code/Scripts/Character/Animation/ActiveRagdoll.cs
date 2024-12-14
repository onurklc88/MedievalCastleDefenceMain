using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ActiveRagdoll : BehaviourRegistry
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Animator _animator;
 
    public Collider[] _ragdollColliders;
    public Rigidbody[] _limbsRigidbodies;
    

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        GetRagdollBits();
        StartCoroutine(Test());
    }

    private void GetRagdollBits()
    {
        if (!Object.HasStateAuthority) return;
       
        _ragdollColliders = GetComponentsInChildren<Collider>();
        _limbsRigidbodies = GetComponentsInChildren<Rigidbody>();
        EventLibrary.DebugMessage.Invoke("Girdi <3 " + _ragdollColliders.Length.ToString());
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCActivateRagdoll()
    {

        
        foreach (Collider col in _ragdollColliders)
        {
            if(col.gameObject.layer != 11)
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


    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
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

        EventLibrary.DebugMessage.Invoke(_ragdollColliders.Length.ToString());
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
        RPCDisableRagdoll();
    }
}
