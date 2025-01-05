using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Arrow : NetworkBehaviour
{
    public bool IsArrowReleased { get; set; }
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private WeaponStats _weapon;
    private bool _isArrowCollided = false;
    private Transform _parentTransform;
    [SerializeField] private Transform _interpolationTarget;
    [SerializeField] private BoxCollider _collison;
    public override void FixedUpdateNetwork()
    {
        if (!IsArrowReleased) return;

        if (_isArrowCollided)
        {
           
            //return;
        }
        if (_rigidbody.velocity.magnitude > 0.1f) 
        {
            transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
        }
    }
  


    public void ExecuteShot(Vector3 pos)
    {
        IsArrowReleased = true;
        Vector3 initialForce = pos * 50f + transform.forward + Vector3.up * 1.75f + Vector3.right * 0.5f;
        transform.rotation = Quaternion.LookRotation(initialForce);
        _rigidbody.AddForce(initialForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider collidedObject)
    {

        if (_isArrowCollided) return;
        _isArrowCollided = true;
        if (Runner.IsClient)
        {
            if (collidedObject.transform.GetComponentInParent<IDamageable>() != null && collidedObject.gameObject.layer != 11)
            {
                var opponentHealth = collidedObject.transform.GetComponentInParent<CharacterHealth>();
                //opponentHealth.DealDamageRPC(25f);
                _parentTransform = collidedObject.transform;
              
                Transform closestBone = GetClosestBone(collidedObject.transform, transform.position);
                if (closestBone != null)
                {
                    transform.SetParent(closestBone);
                    
                    _rigidbody.isKinematic = true;
                    _collison.enabled = false;
                    var networkRigidbody = transform.GetComponent<NetworkRigidbody>();
                    networkRigidbody.enabled = false;
                   
                }
              
            }
            else
            {
                _rigidbody.isKinematic = true;
                var networkRigidbody = transform.GetComponent<NetworkRigidbody>();
                networkRigidbody.enabled = false;
                if (collidedObject.gameObject.layer != 11)
                    StartCoroutine(DestroyArrow());
            }
        }
           
        
    }
    private Transform GetClosestBone(Transform root, Vector3 hitPosition)
    {
        Transform closestBone = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform bone in root.GetComponentsInChildren<Transform>())
        {
            float distance = Vector3.Distance(hitPosition, bone.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestBone = bone;
            }
        }
        return closestBone;
    }

    private IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(2f);
        Runner.Despawn(transform.GetComponent<NetworkObject>());

    }

}
