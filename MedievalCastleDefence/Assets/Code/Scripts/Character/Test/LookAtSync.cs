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
            // Sadece owner oyuncu kendi hedef rotasyonunu g�nderir
            SyncedRotation = lookAtTarget.rotation;
        }
        else
        {
            // Di�er oyuncular i�in yumu�ak ge�i�le kafa y�n�n� uygula
            lookAtTarget.rotation = Quaternion.Slerp(
                lookAtTarget.rotation,
                SyncedRotation,
                Runner.DeltaTime * rotationSmoothSpeed
            );
        }
    }
}