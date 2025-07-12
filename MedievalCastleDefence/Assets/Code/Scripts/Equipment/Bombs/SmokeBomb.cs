using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeBomb : Bomb
{
    public override void FixedUpdateNetwork()
    {

        if (!IsObjectCollided)
        {
            transform.Rotate(Vector3.forward * 300f * Runner.DeltaTime * 2f);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.GetComponentInParent<IDamageable>() == null)
        {
            TriggerSmokeBomb();
            StartCoroutine(DestroyObject(30));
        }
    }

    private void TriggerSmokeBomb()
    {
        if (IsObjectCollided) return;

        IsObjectCollided = true;
        Rigidbody.isKinematic = true;
        transform.rotation = Quaternion.identity;
       
        RPC_SetEffectPosition(transform.position);
        IsBombReadyToExplode = true;
    }
}
