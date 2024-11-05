using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GallowglassAnimation : CharacterAnimationController, IReadInput
{
    [Networked] public CharacterAttackBehaviour.AttackDirection AttackDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkVerticalAnimationStateChanged))] public float PlayerVerticalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkHorizontalWalkAnimationStateChanged))] public float PlayerHorizontalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkedUpperbodyWalkAnimationStateChange))] public NetworkBool OnPlayerWalk { get; set; }
    [Networked(OnChanged = nameof(NetworkParryAnimationStateChange))] public NetworkBool IsPlayerParry { get; set; }

    [Networked(OnChanged = nameof(NetworkedUpperbodyBlockAnimationStateChange))] public int BlockIndex { get; set; }
    [Networked(OnChanged = nameof(NetworkJumpAnimationChanged))] public NetworkBool IsPlayerKick { get; set; }
    [Networked(OnChanged = nameof(NetworkedDamageAnimationStateChange))] public NetworkBool IsPlayerGetDamage { get; set; }
    [Networked(OnChanged = nameof(NetworkedStunnedAnimationStateChange))] public NetworkBool IsPlayerStunned { get; set; }
    //[Networked(OnChanged = nameof(NetworkedStunnedAnimationStateChange))] public NetworkBool IsPlayerStunned { get; set; }
    [Networked(OnChanged = nameof(NetworkAttackAnimationStateChange))] public int SwingIndex { get; set; }
    public NetworkButtons PreviousButton { get; set; }
   
    
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }
    }

    public void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;


        if (input.VerticalInput != 0 || input.HorizontalInput != 0)
        {
            OnPlayerWalk = true;
        }
        else
        {
            OnPlayerWalk = false;
        }

        if (input.VerticalInput != 0)
        {
            PlayerVerticalDirection = input.VerticalInput;
            OnPlayerWalk = true;
        }
        else
        {
            PlayerVerticalDirection = 0;
        }

        if (input.HorizontalInput != 0)
        {
            PlayerHorizontalDirection = input.HorizontalInput;
        }
        else
        {
            PlayerHorizontalDirection = 0;
        }


        PreviousButton = input.NetworkButtons;
    }
    private static void NetworkHorizontalWalkAnimationStateChanged(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[1], changed.Behaviour.PlayerHorizontalDirection);
        changed.Behaviour._animationController.SetBool("OnPlayerWalk", true);
        if (changed.Behaviour.PlayerHorizontalDirection == -1)
            changed.Behaviour._animationController.speed = 0.9f;
        else
            changed.Behaviour._animationController.speed = 1f;

    }
    private static void NetworkVerticalAnimationStateChanged(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[0], changed.Behaviour.PlayerVerticalDirection);
        if (changed.Behaviour.PlayerVerticalDirection == -1)
            changed.Behaviour._animationController.speed = 0.9f;
        else
            changed.Behaviour._animationController.speed = 1f;


    }
    private static void NetworkJumpAnimationChanged(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("IsPlayerKick", changed.Behaviour.IsPlayerKick);
        changed.Behaviour._animationController.Play("Gallowglass-Kick", 0);
    }
    private static void NetworkedUpperbodyWalkAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("OnPlayerWalk", changed.Behaviour.OnPlayerWalk);
    }
    private static void NetworkedUpperbodyBlockAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetInteger("BlockIndex", changed.Behaviour.BlockIndex);
    }
    private static void NetworkAttackAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
         changed.Behaviour._animationController.SetInteger("SwingIndex", changed.Behaviour.SwingIndex);
        float animduration = changed.Behaviour._animationController.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log("anim time: " + animduration);
    }

    private static void NetworkedDamageAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerGetDamage == true) return;
        {
            changed.Behaviour._animationController.Play("Gallowglass-Damage");
        }
    }

    private static void NetworkedStunnedAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerStunned == false) return;
        
        changed.Behaviour._animationController.Play("Gallowglass-Stun", 1);
        switch (changed.Behaviour.AttackDirection)
        {
            case CharacterAttackBehaviour.AttackDirection.Forward:
                changed.Behaviour._animationController.Play("Gallowglass_Stun_Backwards");
                
                break;
            case CharacterAttackBehaviour.AttackDirection.FromLeft:
                changed.Behaviour._animationController.Play("Gallowglass_Stun_Left");
               
                break;
            case CharacterAttackBehaviour.AttackDirection.FromRight:
                changed.Behaviour._animationController.Play("Gallowglass_Stun_Right");
                
                break;
            case CharacterAttackBehaviour.AttackDirection.Backward:
                changed.Behaviour._animationController.Play("Gallowglass_Stun_Forward");
              break;

        }
      
    }
  

    private static void NetworkParryAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerParry == false) return;
        if (changed.Behaviour.BlockIndex == 1)
            changed.Behaviour._animationController.Play("Gallowglass-RightParry");
        else
            changed.Behaviour._animationController.Play("Gallowglass-LeftParry");
    }

    public override void UpdateJumpAnimationState(bool state)
    {
        IsPlayerKick = state;
        StartCoroutine(WaitJumpAnimation(0.1f));
    }
    public override void UpdateDamageAnimationState()
    {
        StartCoroutine(WaitDamageAnimation());
    }

    public override void UpdateStunAnimationState(CharacterAttackBehaviour.AttackDirection attackDirection)
    {
        AttackDirection = attackDirection;
      
        IsPlayerStunned = true;
        StartCoroutine(WaitStunnedAnimation());
    }
    public void UpdateParryAnimation()
    {
        //IsPlayerParry = true;
        //StartCoroutine(WaitParryAnimation());

    }
    public override void PlayBlockAnimation()
    {
        IsPlayerParry = true;
        StartCoroutine(WaitParryAnimation());
    }
    private IEnumerator WaitJumpAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        IsPlayerKick = false;
    }
    private IEnumerator WaitParryAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        IsPlayerParry = false;
    }
    public void UpdateAttackAnimState(int swingIndex)
    {
        SwingIndex = swingIndex;
        StartCoroutine(WaitAttack(1f));
    }
    private IEnumerator WaitDamageAnimation()
    {
        //yield return new WaitForSeconds(0.3f);
        IsPlayerGetDamage = true;
        yield return new WaitForSeconds(0.2f);
        IsPlayerGetDamage = false;
    }
    public void UpdateBlockAnimState(int blockPositionIndex)
    {
        BlockIndex = blockPositionIndex;
    }
    private IEnumerator WaitAttack(float time)
    {
        yield return new WaitForSeconds(time);
        SwingIndex = 0;
    }

    private IEnumerator WaitStunnedAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        IsPlayerStunned = false;
    }

}
