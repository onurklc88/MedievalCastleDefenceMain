using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using UnityEngine.Animations;
public class RangerAnimation : CharacterAnimationController, IReadInput
{

    [Networked(OnChanged = nameof(NetworkVerticalAnimationStateChanged))] public float PlayerVerticalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkHorizontalWalkAnimationStateChanged))] public float PlayerHorizontalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkedUpperbodyWalkAnimationStateChange))] public NetworkBool OnPlayerWalk { get; set; }
    [Networked(OnChanged = nameof(NetworkJumpAnimationChanged))] public NetworkBool IsPlayerJumping { get; set; }
    [Networked(OnChanged = nameof(NetworkedDamageAnimationStateChange))] public NetworkBool IsPlayerGetDamage { get; set; }
    [Networked(OnChanged = nameof(NetworkedStunnedAnimationStateChange))] public NetworkBool IsPlayerStunned { get; set; }
    [Networked(OnChanged = nameof(NetworkDrawAnimationStateChange))] public NetworkBool IsPlayerDrawingBow { get; set; }
  
    
    public NetworkButtons PreviousButton { get; set; }
    private CharacterMovement _characterMovement;
   
   
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        InitScript(this);
    }
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _characterMovement = GetScript<CharacterMovement>();
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
        if (_characterMovement.IsInputDisabled) return;

      
        if (input.VerticalInput != 0)
        {
            PlayerVerticalDirection = input.VerticalInput;
            //OnPlayerWalk = true;
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
    private static void NetworkHorizontalWalkAnimationStateChanged(Changed<RangerAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[1], changed.Behaviour.PlayerHorizontalDirection);
        //changed.Behaviour._animationController.SetBool("OnPlayerWalk", true);
     }
    private static void NetworkVerticalAnimationStateChanged(Changed<RangerAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[0], changed.Behaviour.PlayerVerticalDirection);
    }
    private static void NetworkJumpAnimationChanged(Changed<RangerAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerJumping == false) return;
        changed.Behaviour.PlayJumpAnimation();
    }
    private static void NetworkedUpperbodyWalkAnimationStateChange(Changed<RangerAnimation> changed)
    {
        //changed.Behaviour._animationController.SetBool("OnPlayerWalk", changed.Behaviour.OnPlayerWalk);
        if (changed.Behaviour.OnPlayerWalk == false) return;
        changed.Behaviour._animationController.Play("Idle-Ranger", 1);
    }
 
    private static void NetworkDrawAnimationStateChange(Changed<RangerAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("IsPlayerDrawing", changed.Behaviour.IsPlayerDrawingBow);
      
    }

    private static void NetworkedDamageAnimationStateChange(Changed<RangerAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerGetDamage == true) return;
        {
            changed.Behaviour._animationController.Play("mixamo_com");
        }
    }

    private static void NetworkedStunnedAnimationStateChange(Changed<RangerAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerStunned == false) return;


        changed.Behaviour._animationController.Play("StunV2_Saxon", 1);
    }


    public string GetCurrentPlayingAnimationClipName()
    {
        AnimatorClipInfo[] clipInfo = _animationController.GetCurrentAnimatorClipInfo(1);

        if (clipInfo.Length > 0)
        {
            AnimationClip clip = clipInfo[0].clip;
            return clip.name;
        }

        return "none";
    }

    public async void UpdateIdleAnimationState()
    {
        if (!Object.HasStateAuthority) return;
        if (!Object || !Object.IsValid) return;
        OnPlayerWalk = true;
        await UniTask.Delay(200);
        OnPlayerWalk = false;
    }
    public override void UpdateJumpAnimationState(bool state)
    {
        IsPlayerJumping = state;
        StartCoroutine(WaitJumpAnimation(0.3f));
    }
    public override void UpdateDamageAnimationState()
    {
        StartCoroutine(WaitDamageAnimation());
    }

    public override void UpdateStunAnimationState(int stunDuration)
    {
        IsPlayerStunned = true;
        //_opponentAttackDirection = attackDirection;
        StartCoroutine(WaitStunnedAnimation());
    }

    public void UpdateDrawAnimState(bool isDraw)
    {
        IsPlayerDrawingBow = isDraw;
    }
    public void UpdateParryAnimation()
    {
        //IsPlayerParry = true;
        //StartCoroutine(WaitParryAnimation());

    }
  
    private void PlayJumpAnimation()
    {
        _animationController.Play("Ranger_Jump");
    }
    private IEnumerator WaitJumpAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        IsPlayerJumping = false;
    }
  
    private IEnumerator WaitDamageAnimation()
    {
        //yield return new WaitForSeconds(0.3f);
        IsPlayerGetDamage = true;
        yield return new WaitForSeconds(0.2f);
        IsPlayerGetDamage = false;
    }
  
    private IEnumerator WaitStunnedAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        IsPlayerStunned = false;
    }

}
