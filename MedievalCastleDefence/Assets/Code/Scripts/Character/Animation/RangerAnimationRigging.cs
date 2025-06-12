using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Fusion;
using static BehaviourRegistry;

public class RangerAnimationRigging : CharacterRegistry
{
    
    [Networked] private Quaternion NetworkedSpineRotation { get; set; }
    
    public bool IsPlayerAiming { get; set; }
    private float _lastWeight = 0;
    [SerializeField] private Transform _spineBone;
    [SerializeField] private RigBuilder _rigBuilder;
    [SerializeField] private MultiAimConstraint _spineConstrait;
    public override void Spawned()
    {
        _spineConstrait.weight = 0;
        if (!Object.HasStateAuthority)
        {
         
            Destroy(GetComponent<RigBuilder>());
        }
        else
        {
            InitScript(this);
        }


    }
    public override void FixedUpdateNetwork()
    {
        
        //if (!IsPlayerAiming) { return; }
        float targetWeight = IsPlayerAiming ? 0.7f : 0f;
        if (_lastWeight != targetWeight)
        {
            _spineConstrait.weight = targetWeight;
            _lastWeight = targetWeight;
        }
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

