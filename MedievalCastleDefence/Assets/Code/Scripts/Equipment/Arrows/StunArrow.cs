using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StunArrow : Arrow
{
    public override void Spawned()
    {
        StartCoroutine(DestroyObject(13f));
    }


    public override void FixedUpdateNetwork()
    {
        if (!IsArrowReleased) return;
        UpdateArrowRotation();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (Object == null || !Object.HasStateAuthority)
            return;
        TriggerExplosiveArrow(transform.position);
        StartCoroutine(DestroyObject(1f));
    }

    private void TriggerExplosiveArrow(Vector3 explosionPosition)
    {
       
    }
    
    private void DrawExplosionDebug(Vector3 position, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = position + new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Debug.DrawLine(prevPoint, nextPoint, Color.red, 0.5f); // 0.5 saniye sahnede kalýr
            prevPoint = nextPoint;
        }
    }


}
