using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class FootknightAnimation : CharacterAnimationController, IReadInput
{
    [Networked(OnChanged = nameof(NetworkVerticalAnimationStateChanged))] public float PlayerVerticalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkHorizontalWalkAnimationStateChanged))] public float PlayerHorizontalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkedUpperbodyWalkAnimationStateChange))] public NetworkBool OnPlayerWalk { get; set; }

    [Networked(OnChanged = nameof(NetworkedUpperbodyParryAnimationStateChange))] public NetworkBool IsPlayerParry { get; set; }
    [Networked(OnChanged = nameof(NetworkJumpAnimationChanged))] public NetworkBool IsPlayerJumping { get; set; }
    [Networked(OnChanged = nameof(NetworkAttackAnimationStateChange))] public NetworkBool IsPlayerSwing { get; set; }
    [Networked(OnChanged = nameof(NetworkedDamageAnimationStateChange))] public NetworkBool IsPlayerGetDamage { get; set; }
    [Networked(OnChanged = nameof(NetworkedStunnedAnimationStateChange))] public NetworkBool IsPlayerStunned { get; set; }
    public NetworkButtons PreviousButton { get; set; }
    [SerializeField] private string[] _triggerAnimations;
   // [SerializeField] private string[] _characterDirections;  
    [SerializeField] private string[] _attackStates;  
    //[SerializeField] private Animator _animationController;
    

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
    
    private static void NetworkHorizontalWalkAnimationStateChanged(Changed<FootknightAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[1], changed.Behaviour.PlayerHorizontalDirection);
        changed.Behaviour._animationController.SetBool("OnPlayerWalk", true);
       
    }
    private static void NetworkVerticalAnimationStateChanged(Changed<FootknightAnimation> changed)
    {
         changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[0], changed.Behaviour.PlayerVerticalDirection);
         
    }
    private static void NetworkJumpAnimationChanged(Changed<FootknightAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerJumping == false) return;
        changed.Behaviour.PlayJumpAnimation();
    }
    private static void NetworkedUpperbodyWalkAnimationStateChange(Changed<FootknightAnimation> changed)
    {
       changed.Behaviour._animationController.SetBool("OnPlayerWalk", changed.Behaviour.OnPlayerWalk);
    }
    private static void NetworkedUpperbodyParryAnimationStateChange(Changed<FootknightAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("OnPlayerParry", changed.Behaviour.IsPlayerParry);
    }
    private static void NetworkAttackAnimationStateChange(Changed<FootknightAnimation> changed)
    {
      if(changed.Behaviour.IsPlayerSwing == true)
          changed.Behaviour.PlaySwingAnimation();
    }

    private static void NetworkedDamageAnimationStateChange(Changed<FootknightAnimation> changed)
    {
       if(changed.Behaviour.IsPlayerGetDamage == true)
       {
            changed.Behaviour.PlayDamageAnimation();
       }
    }

    private static void NetworkedStunnedAnimationStateChange(Changed<FootknightAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerStunned == true)
        {
            changed.Behaviour.PlayStunAnimation();
        }
    }
    public override void UpdateJumpAnimationState(bool state)
    {
        IsPlayerJumping = state;
        StartCoroutine(WaitJumpAnimation(0.3f));
    }
    public override void UpdateDamageAnimationState()
    {
        //IsPlayerGetDamage = true;
        StartCoroutine(WaitDamageAnimation());
    }
    public override void UpdateSwingAnimationState(bool state)
    {
       IsPlayerSwing = state;
       StartCoroutine(WaitAttackAnimation(0.9f));
    }

    public override void UpdateStunAnimationState()
    {
        IsPlayerStunned = true;
        StartCoroutine(WaitStunnedAnimation());
    }

    private void PlaySwingAnimation()
    {
      _animationController.SetTrigger(_attackStates[0]);
    }

    private void PlayJumpAnimation()
    {
        _animationController.Play("foot-knight-jump");
    }

   private void PlayDamageAnimation()
   {
        _animationController.Play("foot-kinght-Damage");
   }

    private void PlayStunAnimation()
    {
       _animationController.Play("Stun-Stormsheildlowerbody");
       _animationController.Play("Stun_StormshieldUpperBody");

    }
    private IEnumerator WaitDamageAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        IsPlayerGetDamage = true;
        yield return new WaitForSeconds(0.2f);
        IsPlayerGetDamage = false;
    }
    private IEnumerator WaitJumpAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        IsPlayerJumping = false;
    }
    private IEnumerator WaitAttackAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        IsPlayerSwing = false;
    }

    private IEnumerator WaitStunnedAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        IsPlayerStunned = false;
    }

    
}
