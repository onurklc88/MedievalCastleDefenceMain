using Fusion;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimRig : NetworkBehaviour
{
    [Networked] private Quaternion NetworkedSpineRotation { get; set; }
    public Transform spineBone; // Rigging'in kontrol ettiği kemik
    public RigBuilder rigBuilder; // RigBuilder referansı

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            Destroy(GetComponent<RigBuilder>()); // Remote player'da Rigging'i TAMAMEN KAPAT
        }
    }
    public override void FixedUpdateNetwork()
    {
        // 1. AUTHORITY: Rigging çalışsın ve sonucu networke göndersin
        if (Object.HasStateAuthority)
        {
            NetworkedSpineRotation = spineBone.localRotation;
            return;
        }

        // 2. REMOTE: Networkten gelen rotasyonu direkt uygula
        spineBone.localRotation = NetworkedSpineRotation;
    }
    private void LateUpdate()
    {
        // Remote player'da animasyonun düzgün görünmesi için
        if (!Object.HasStateAuthority)
        {
            spineBone.localRotation = NetworkedSpineRotation;
        }
    }


}
