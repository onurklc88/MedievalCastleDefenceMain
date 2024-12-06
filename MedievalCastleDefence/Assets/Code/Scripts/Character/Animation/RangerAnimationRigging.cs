using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RangerAnimationRigging : BehaviourRegistry
{
    [SerializeField] private MultiAimConstraint _leftHandConstrait;
    [SerializeField] private MultiAimConstraint _rightHandConstrait;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    public void UpdateConstraits(bool condition)
    {
        if (condition)
            _leftHandConstrait.weight = 0.6f;
        else
            _leftHandConstrait.weight = 0f;
    }
        
}

