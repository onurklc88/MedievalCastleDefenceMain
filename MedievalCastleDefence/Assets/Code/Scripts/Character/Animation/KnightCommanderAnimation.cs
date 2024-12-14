using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class KnightCommanderAnimation : CharacterAnimationController, IReadInput
{
    [Networked(OnChanged = nameof(NetworkVerticalAnimationStateChanged))] public float PlayerVerticalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkHorizontalWalkAnimationStateChanged))] public float PlayerHorizontalDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkedUpperbodyWalkAnimationStateChange))] public NetworkBool OnPlayerWalk { get; set; }
    [Networked(OnChanged = nameof(NetworkParryAnimationStateChange))] public NetworkBool IsPlayerParry { get; set; }

    [Networked(OnChanged = nameof(NetworkedUpperbodyBlockAnimationStateChange))] public int BlockIndex { get; set; }
    [Networked(OnChanged = nameof(NetworkJumpAnimationChanged))] public NetworkBool IsPlayerJumping { get; set; }
    [Networked(OnChanged = nameof(NetworkedDamageAnimationStateChange))] public NetworkBool IsPlayerGetDamage { get; set; }
    [Networked(OnChanged = nameof(NetworkedStunnedAnimationStateChange))] public NetworkBool IsPlayerStunned { get; set; }
    
    [Networked(OnChanged = nameof(NetworkAttackAnimationStateChange))] public int SwingIndex { get; set; }
    public NetworkButtons PreviousButton { get; set; }
    private CharacterMovement _characterMovement;
    //Bunu networked yapmak lazým
    private CharacterAttackBehaviour.AttackDirection _opponentAttackDirection;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        
    }

    private void Start()
    {
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
        if (!Object.HasStateAuthority || _characterMovement == null) return;
        if (_characterMovement.IsPlayerStunned) return;

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
    private static void NetworkHorizontalWalkAnimationStateChanged(Changed<KnightCommanderAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[1], changed.Behaviour.PlayerHorizontalDirection);
        changed.Behaviour._animationController.SetBool("OnPlayerWalk", true);
        if (changed.Behaviour.PlayerHorizontalDirection == -1)
            changed.Behaviour._animationController.speed = 0.9f;
        else
            changed.Behaviour._animationController.speed = 1f;

    }
    private static void NetworkVerticalAnimationStateChanged(Changed<KnightCommanderAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[0], changed.Behaviour.PlayerVerticalDirection);
        if(changed.Behaviour.PlayerVerticalDirection == -1)
            changed.Behaviour._animationController.speed = 0.9f;
        else
            changed.Behaviour._animationController.speed = 1f;


    }
    private static void NetworkJumpAnimationChanged(Changed<KnightCommanderAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerJumping == false) return;
            changed.Behaviour.PlayJumpAnimation();
    }
    private static void NetworkedUpperbodyWalkAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("OnPlayerWalk", changed.Behaviour.OnPlayerWalk);
    }
    private static void NetworkedUpperbodyBlockAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        //changed.Behaviour._animationController.SetBool("OnPlayerParry", changed.Behaviour.IsPlayerParry);
        changed.Behaviour._animationController.SetInteger("BlockIndex", changed.Behaviour.BlockIndex);
    }
    private static void NetworkAttackAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {

        //  changed.Behaviour._animationController.SetBool("IsRightSwing", changed.Behaviour.IsPlayerSwing);
       // if (changed.Behaviour.SwingIndex == 0) return;

        changed.Behaviour._animationController.SetInteger("SwingIndex", changed.Behaviour.SwingIndex);
    }

    private static void NetworkedDamageAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerGetDamage == true) return;
        {
            changed.Behaviour._animationController.Play("KnightCommander-Damage");
        }
    }

    private static void NetworkedStunnedAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerStunned == false) return;

        
        changed.Behaviour._animationController.Play("KnightCommander-StunUpperBody", 1);

        #region problem On network
        switch (changed.Behaviour._opponentAttackDirection)
        {
            case CharacterAttackBehaviour.AttackDirection.Forward:
                changed.Behaviour._animationController.Play("KnightCommander-StunBackwards", 0);

                break;
            case CharacterAttackBehaviour.AttackDirection.FromLeft:
                changed.Behaviour._animationController.Play("KnightCommander-StunLeft", 0);

                break;
            case CharacterAttackBehaviour.AttackDirection.FromRight:
                changed.Behaviour._animationController.Play("KnightCommander-StunRight", 0);

                break;
            case CharacterAttackBehaviour.AttackDirection.Backward:
                changed.Behaviour._animationController.Play("KnightCommander-StunForward", 0);
                break;

        }
        #endregion
    }


    private static void NetworkParryAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerParry == false) return;
        if (changed.Behaviour.BlockIndex == 1)
            changed.Behaviour._animationController.Play("KnightCommander-RightParry");
        else
            changed.Behaviour._animationController.Play("KnightCommander-LeftParry");
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
   
    public override void UpdateStunAnimationState(CharacterAttackBehaviour.AttackDirection attackDirection)
    {
        IsPlayerStunned = true;
        _opponentAttackDirection = attackDirection;
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
    private void PlayJumpAnimation()
    {
        _animationController.Play("KnightCommander-Jump");
    }
    private IEnumerator WaitJumpAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        IsPlayerJumping = false;
    }
    private IEnumerator WaitParryAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        IsPlayerParry = false;
    }
    public void UpdateAttackAnimState(int swingIndex)
    {
        SwingIndex = swingIndex;
        StartCoroutine(WaitAttack(0.5f));
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
        yield return new WaitForSeconds(0.1f);
        IsPlayerStunned = false;
    }
   
}
