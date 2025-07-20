using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunBomb : Bomb
{
    

    public override void Spawned()
    {
        Invoke("EnableCollision", 0.2f);
        StartCoroutine(DestroyObject(13f));
    }
    public override void FixedUpdateNetwork()
    {

        if (!IsObjectCollided)
        {
            transform.Rotate(Vector3.forward * 300f * Runner.DeltaTime * 2f);
        }
     }
   
    private void OnTriggerEnter(Collider other)
    {
        if (Object == null || !Object.HasStateAuthority)
            return;
        TriggerStunBomb(transform.position);
        StartCoroutine(DestroyObject(2f));
    }

    private void TriggerStunBomb(Vector3 explosionPosition)
    {
        if (IsObjectCollided) return;

        IsObjectCollided = true;
        Rigidbody.isKinematic = true;
        RPC_SetEffectPosition(explosionPosition + Vector3.up * 1.2f);
        IsBombReadyToExplode = true;


        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, BombProperties.AEOWidth);
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
   
}
