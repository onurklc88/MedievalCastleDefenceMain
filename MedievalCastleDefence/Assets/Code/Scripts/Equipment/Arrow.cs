using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Arrow : NetworkBehaviour
{
    public bool IsArrowReleased { get; set; }
    [SerializeField] private Rigidbody _rigidbody;
    private bool _isArrowCollided = false;

    public override void FixedUpdateNetwork()
    {
        if (!IsArrowReleased) return;

        if (_isArrowCollided)
        {
            _rigidbody.isKinematic = true;
            return;
        }
        if (_rigidbody.velocity.magnitude > 0.1f) 
        {
            transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
        }
    }

 

    public void ExecuteShot(Vector3 pos)
    {
        IsArrowReleased = true;
        Vector3 initialForce = pos * 3f + transform.forward;
        transform.rotation = Quaternion.LookRotation(initialForce);
        _rigidbody.AddForce(initialForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        _isArrowCollided = true;
        Debug.Log("Name: " + other.gameObject.name);
    }

}
