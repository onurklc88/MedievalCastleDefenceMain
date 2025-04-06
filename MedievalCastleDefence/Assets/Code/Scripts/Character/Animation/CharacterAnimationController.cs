using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
public class CharacterAnimationController : CharacterRegistry
{
    [SerializeField] protected Animator _animationController;
    [SerializeField] protected string[] _characterDirections;
    public virtual void UpdateSwingAnimationState(bool state) { }
    public virtual void UpdateJumpAnimationState(bool state) { }
    public virtual void UpdateDamageAnimationState() { }
    public virtual void UpdateStunAnimationState(int stunDuration) { }
    public virtual void PlayBlockAnimation() { }
    public virtual void TestUpdateLowerBodyStunAnimation() { }
}
