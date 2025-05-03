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
    [SerializeField] private NetworkRigidbody _networkRigidbody;
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
          
            _leftHandConstrait.weight = 0.6f;
        }
        else
            _leftHandConstrait.weight = 0.47f;
    }

    private static void OnNetworkDrawStateChange(Changed<RangerAnimationRigging> changed)
    {
        changed.Behaviour._networkRigidbody.enabled = changed.Behaviour.IsPlayerDrawing;
    }
        
}

