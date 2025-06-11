using Fusion;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimRig : NetworkBehaviour
{
    [Networked] private Quaternion NetworkedSpineRotation { get; set; }
    [SerializeField] private Transform _spineBone;
    [SerializeField] private RigBuilder _rigBuilder; 
    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            Destroy(GetComponent<RigBuilder>()); 
        }
    }
    public override void FixedUpdateNetwork()
    {
       
        if (Object.HasStateAuthority)
        {
            NetworkedSpineRotation = _spineBone.localRotation;
            return;
        }

       
        _spineBone.localRotation = NetworkedSpineRotation;
    }
    private void LateUpdate()
    {
        
        if (!Object.HasStateAuthority)
        {
            _spineBone.localRotation = NetworkedSpineRotation;
        }
    }


}
