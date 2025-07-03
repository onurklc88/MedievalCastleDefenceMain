using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
public class CharacterAnimationController : CharacterRegistry
{
    [Networked(OnChanged = nameof(NetworkSlowedAnimationStateChange))] public NetworkBool IsPlayerSlowed { get; set; }
    [SerializeField] protected Animator _animationController;
    [SerializeField] protected string[] _characterDirections;
    public void ChangeAnimationSpeed(bool state)
    {
        IsPlayerSlowed = state;
    }
    private static void NetworkSlowedAnimationStateChange(Changed<CharacterAnimationController> changed)
    {
        if (changed.Behaviour.IsPlayerSlowed)
        {
            changed.Behaviour._animationController.Play("Slowed1");
            changed.Behaviour._animationController.SetFloat("LowerBodySpeedMultiplier", 0.5f);
        }
        else
        {
            changed.Behaviour._animationController.SetFloat("LowerBodySpeedMultiplier", 1f);
        }
        
    }

    public virtual void UpdateSwingAnimationState(bool state) { }
    public virtual void UpdateJumpAnimationState(bool state) { }
    public virtual void UpdateDamageAnimationState() { }
    public virtual void UpdateStunAnimationState(int stunDuration) { }
    public virtual void PlayBlockAnimation() { }
    public virtual void UpdateThrowingAnimation(bool state) { }
    public virtual void TestUpdateLowerBodyStunAnimation() { }


    public string GetCurrentAnimationState(string targetLayer)
    {
        int layerIndex = _animationController.GetLayerIndex(targetLayer);
        if (layerIndex < 0) return null;

        AnimatorStateInfo state = _animationController.GetCurrentAnimatorStateInfo(layerIndex);
        return state.IsName("Slowed1") ? "Slowed1" : "UnknownState";
    }
}
