using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
public class BombArrow : Arrow
{

    [SerializeField] private ParticleSystem _bombEffect;
  
    [Networked(OnChanged = nameof(OnArrowStateChange))] public NetworkBool IsBombReadyToExplode { get; set; }
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
        if (IsObjectCollided) return;
        IsObjectCollided = true;
        DrawExplosionDebug(explosionPosition, 5f);
        _rigidbody.isKinematic = true;
        //_bombEffect.transform.position = new Vector3(explosionPosition.x, explosionPosition.y + 1.2f, explosionPosition.z);
        IsBombReadyToExplode = true;
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, 5f);
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
            if (damageable == null)
                continue;

            damageable.DealDamageRPC(
                50f,
                _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(),
                CharacterStats.CharacterType.Ranger
            );
        }
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
    private static void OnArrowStateChange(Changed<BombArrow> changed)
    {
       
        changed.Behaviour._bombEffect.Play();
    }


}
