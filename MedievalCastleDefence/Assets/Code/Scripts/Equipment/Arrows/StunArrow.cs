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
        if (IsObjectCollided) return;

        IsObjectCollided = true;
        _rigidbody.isKinematic = true;
        RPC_SetEffectPosition(explosionPosition + Vector3.up * 1.2f);
        IsObjectCollided = true;


        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, ArrowProperties.AOEWidth);
        HashSet<NetworkId> alreadyDamaged = new HashSet<NetworkId>();

        foreach (var hitCollider in hitColliders)
        {
            NetworkObject netObj = hitCollider.transform.GetComponentInParent<NetworkObject>();
            if (netObj == null || !netObj.IsValid)
                continue;


            NetworkId damageableId = netObj.Id;
            if (alreadyDamaged.Contains(damageableId))
                continue;

            alreadyDamaged.Add(damageableId);


            var damageable = hitCollider.transform.GetComponentInParent<IDamageable>();
            var playerInput = hitCollider.transform.GetComponentInParent<CharacterCameraController>();
            var characterMovement = hitCollider.transform.GetComponentInParent<CharacterMovement>();
            if (damageable == null)
                continue;

            if (netObj.Id == OwnerProperties.PlayerID ||
                hitCollider.transform.GetComponentInParent<PlayerStatsController>()?.PlayerTeam !=
                OwnerProperties.PlayerTeam)
            {
                playerInput.RPC_ReduceMouseSpeedTemporarily();
                characterMovement.RPC_SlowPlayerSpeed();
            }
        }
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
