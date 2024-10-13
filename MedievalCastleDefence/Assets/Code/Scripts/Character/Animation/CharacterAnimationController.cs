using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class CharacterAnimationController : BehaviourRegistry
{
    [SerializeField] protected Animator _animationController;
    [SerializeField] protected string[] _characterDirections;
    public virtual void UpdateSwingAnimationState(bool state) { }
    public virtual void UpdateJumpAnimationState(bool state) { }
    public virtual void UpdateDamageAnimationState() { }
    public virtual void UpdateStunAnimationState() { }
    public virtual void PlayBlockAnimation() { }
}
