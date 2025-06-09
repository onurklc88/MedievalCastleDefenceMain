using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Fusion;
using static BehaviourRegistry;

public class RangerAnimationRigging : CharacterRegistry
{
    [SerializeField] private MultiAimConstraint _leftHandConstrait;
    [SerializeField] private MultiAimConstraint _rightHandConstrait;
    [Networked(OnChanged = nameof(OnNetworkDrawStateChange))] public NetworkBool IsPlayerDrawing { get; set; }
    [SerializeField] private NetworkRigidbody _networkLeftArmRigidbody;
    [SerializeField] private NetworkRigidbody _networkHeadRigidbody;
    [SerializeField] private NetworkRigidbody _networkRightHandRigidbody;
    [SerializeField] private NetworkRigidbody _networkSpineRigidbody;
    [SerializeField] private Transform _leftArm;
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _spine;
    
    private Vector3 SyncedLookTarget { get; set; }

    [SerializeField] private Transform lookAtTarget;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    public void UpdateConstraits(bool condition)
    {
        
        IsPlayerDrawing = condition;
        Debug.Log("IsplayerDrawing: " + condition);
        if (condition)
        {
          
            _leftHandConstrait.weight = 1f;
        }
        else
            _leftHandConstrait.weight = 0.47f;
        
    }

    private void OnTargetChanged()
    {
        lookAtTarget.position = SyncedLookTarget;
    }

    public override void FixedUpdateNetwork()
    {
       
          
    }
    private static void OnNetworkDrawStateChange(Changed<RangerAnimationRigging> changed)
    {
        if (changed.Behaviour.IsPlayerDrawing)
        {
           // changed.Behaviour._networkLeftArmRigidbody.InterpolationTarget = changed.Behaviour._leftArm;
           // changed.Behaviour._networkHeadRigidbody.InterpolationTarget = changed.Behaviour._head;
          //  changed.Behaviour._networkRightHandRigidbody.InterpolationTarget = changed.Behaviour._rightHand;
            //changed.Behaviour._networkSpineRigidbody.InterpolationTarget = changed.Behaviour._spine;
        }
        else
        {
           // changed.Behaviour._networkHeadRigidbody.InterpolationTarget = null;
           // changed.Behaviour._networkLeftArmRigidbody.InterpolationTarget = null;
           // changed.Behaviour._networkRightHandRigidbody.InterpolationTarget = null;
            //changed.Behaviour._networkSpineRigidbody.InterpolationTarget = null;
        }
           
    }
        
}

