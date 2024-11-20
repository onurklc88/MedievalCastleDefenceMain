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
        Vector3 initialForce = pos * 40f + transform.forward + Vector3.up * 4f + Vector3.right * 0.5f;
        transform.rotation = Quaternion.LookRotation(initialForce);
        _rigidbody.AddForce(initialForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider collidedObject)
    {

        if (_isArrowCollided) return;
        _isArrowCollided = true;
        if (Runner.IsClient)
        {
            if (collidedObject.transform.GetComponentInParent<IDamageable>() != null)
            {
                var opponentHealth = collidedObject.transform.GetComponentInParent<CharacterHealth>();
                opponentHealth.DealDamageRPC(25f);
                _parentTransform = collidedObject.transform;
                //gameObject.transform.SetParent(_parentTransform);
                Transform closestBone = GetClosestBone(collidedObject.transform, transform.position);
                if (closestBone != null)
                {
                    transform.SetParent(closestBone);
                    _rigidbody.velocity = Vector3.zero;
                    _rigidbody.angularVelocity = Vector3.zero;
                    _rigidbody.useGravity = false;
                    _rigidbody.isKinematic = true;
                    var networkRigidbody = transform.GetComponent<NetworkRigidbody>();
                    networkRigidbody.enabled = false;
                   var _networkTransform = _interpolationTarget.gameObject.AddComponent<NetworkTransform>();
                    _networkTransform.InterpolationTarget = _interpolationTarget;
                }
                Debug.Log("þimdi Sex");
            }
            else
            {
                _rigidbody.isKinematic = true;
                gameObject.transform.SetParent(collidedObject.transform);
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
       // Runner.Despawn(transform.GetComponent<NetworkObject>());

    }

}
