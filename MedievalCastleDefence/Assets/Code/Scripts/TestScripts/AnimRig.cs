using Fusion;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimRig : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnSpineDataChanged))]
    private Quaternion NetworkedSpineRotation { get; set; }

    [Networked]
    private float NetworkedRigWeight { get; set; }

    // Referanslar
    public Transform spineBone; // Kontrol etmek istediðiniz spine kemiði
    public MultiAimConstraint spineConstraint; // Spine için rig constraint
    public float interpolationSpeed = 8f;

  
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // Rigging sonuçlarýný networke gönder
            NetworkedSpineRotation = spineBone.localRotation;
            NetworkedRigWeight = spineConstraint.weight;
           
        }
        else
        {
            // Remote player'lar için yumuþak geçiþ
            spineBone.localRotation = Quaternion.Lerp(
                spineBone.localRotation,
                NetworkedSpineRotation,
                Runner.DeltaTime * interpolationSpeed
            );

            spineConstraint.weight = Mathf.Lerp(
                spineConstraint.weight,
                NetworkedRigWeight,
                Runner.DeltaTime * interpolationSpeed
            );
        }
    }

    private static void OnSpineDataChanged(Changed<AnimRig> changed)
    {
        // Veri deðiþtiðinde hemen güncelle (lerp Update'de yapýlýyor)
    }
}
