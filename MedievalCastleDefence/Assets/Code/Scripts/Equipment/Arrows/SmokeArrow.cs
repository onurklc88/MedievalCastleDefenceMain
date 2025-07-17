using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeArrow : Arrow
{
  
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponentInParent<IDamageable>() == null)
        {
           
            TriggerSmokeArrow();
            StartCoroutine(DestroyObject(30));
        }
    }

    private void TriggerSmokeArrow()
    {
        if (IsObjectCollided) return;

        IsObjectCollided = true;
        _rigidbody.isKinematic = true;
        transform.rotation = Quaternion.identity;

        RPC_SetEffectPosition(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        
    }
}
