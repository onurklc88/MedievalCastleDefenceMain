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
    public Transform spineBone; // Kontrol etmek istedi�iniz spine kemi�i
    public MultiAimConstraint spineConstraint; // Spine i�in rig constraint
    public float interpolationSpeed = 8f;

  
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // Rigging sonu�lar�n� networke g�nder
            NetworkedSpineRotation = spineBone.localRotation;
            NetworkedRigWeight = spineConstraint.weight;
           
        }
        else
        {
            // Remote player'lar i�in yumu�ak ge�i�
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
        // Veri de�i�ti�inde hemen g�ncelle (lerp Update'de yap�l�yor)
    }
}
