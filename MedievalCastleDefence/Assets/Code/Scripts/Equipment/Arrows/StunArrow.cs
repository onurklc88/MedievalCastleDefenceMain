using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class StunArrow : Arrow
{
    public override void Spawned()
    {
        StartCoroutine(DestroyArrow(13f));
    }


    public override void InitOwnerStats(PlayerStatsController ownerStats)
    {
        if (ownerStats == null)
        {
            Debug.LogError("OwnerStats null verildi!");
            return;
        }
        _playerStatsController = ownerStats;
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
        if (other.gameObject.GetComponentInParent<IDamageable>() != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            StartCoroutine(DestroyArrow(1f));
        }


    }

    private void TriggerExplosiveArrow(Vector3 explosionPosition)
    {
       
    }
    private IEnumerator DestroyArrow(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);

        if (Runner != null && Object != null && Object.IsValid)
            Runner.Despawn(Object);

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
