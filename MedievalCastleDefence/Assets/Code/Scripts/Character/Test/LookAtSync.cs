using Fusion;
using UnityEngine;

public class LookAtSync : NetworkBehaviour
{
    [SerializeField] private Transform lookAtTarget; // Rig'deki kafa hedef objesi

    [Networked] private Quaternion SyncedRotation { get; set; }

    public float rotationSmoothSpeed = 10f;

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            // Sadece owner oyuncu kendi hedef rotasyonunu gönderir
            SyncedRotation = lookAtTarget.rotation;
        }
        else
        {
            // Diðer oyuncular için yumuþak geçiþle kafa yönünü uygula
            lookAtTarget.rotation = Quaternion.Slerp(
                lookAtTarget.rotation,
                SyncedRotation,
                Runner.DeltaTime * rotationSmoothSpeed
            );
        }
    }
}