using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;


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
    [Networked(OnChanged = nameof(NetworkStunExitTransitionChange))] public NetworkBool CanStunExit { get; set; }
    [Networked(OnChanged = nameof(NetworkedThrowingBombAnimationStateChange))] public NetworkBool IsPlayerHoldingBomb { get; set; }
    [Networked(OnChanged = nameof(NetworkUpperbodyLeaningStateChange))] public float LeanDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkUpperbodyVerticalStateChange))] public float VerticalDirection { get; set; }
    
    public NetworkButtons PreviousButton { get; set; }
    private CharacterMovement _characterMovement;
    private CharacterAttackBehaviour _kcAttack;
    //Bunu networked yapmak laz�m
    private CharacterAttackBehaviour.AttackDirection _opponentAttackDirection;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        
    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _characterMovement = GetScript<CharacterMovement>();
        _kcAttack = GetScript<KnightCommanderAttack>();
        _animationController.SetLayerWeight(3, 0.25f);
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }
    }
    [SerializeField] private float _leanSmoothTime = 0.1f; // Inspector'dan ayarla (0.05-0.2 aras�)
    private float _currentLeanVelocity;
    [SerializeField] private float _smoothTime = 0.05f; // Ge�i� s�resi (sn)
    private float _verticalDirectionVelocity; // Ref velocity for SmoothDamp
    public void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority || _characterMovement == null) return;
        if (_characterMovement.IsInputDisabled) return;

        OnPlayerWalk = input.VerticalInput != 0 || input.HorizontalInput != 0;
       
        if (input.VerticalInput != 0)
            PlayerVerticalDirection = input.VerticalInput;
        else
            PlayerVerticalDirection = 0;

        
        float targetDirection = input.VerticalInput; 

      
        VerticalDirection = Mathf.SmoothDamp(
            current: VerticalDirection,
            target: targetDirection,
            currentVelocity: ref _verticalDirectionVelocity,
            smoothTime: _smoothTime
        );
      

        if (Mathf.Abs(input.HorizontalInput) < 0.05f)
            PlayerHorizontalDirection = 0;
        else
            PlayerHorizontalDirection = input.HorizontalInput;

        UpperBodyLeaning();

        PreviousButton = input.NetworkButtons;
    }

    private void UpperBodyLeaning()
    {
        if (_kcAttack.IsPlayerBlockingLocal == true || _characterMovement.IsInputDisabled) return;
        float target = (_kcAttack.PlayerSwordPositionLocal == CharacterAttackBehaviour.SwordPosition.Right) ? 1f : -1f;
        float current = _animationController.GetFloat("LeanDirection");
        float smoothed = Mathf.SmoothDamp(current, target, ref _currentLeanVelocity, _leanSmoothTime);
        LeanDirection = smoothed;
        //_animationController.SetFloat("LeanDirection", smoothed);
    }
    private static void NetworkHorizontalWalkAnimationStateChanged(Changed<KnightCommanderAnimation> changed)
    {
       
        changed.Behaviour._animationController.SetFloat(changed.Behaviour._characterDirections[1], changed.Behaviour.PlayerHorizontalDirection);
        
        
        if (changed.Behaviour.PlayerHorizontalDirection == -1)
            changed.Behaviour._animationController.speed = 0.9f;
        else
            changed.Behaviour._animationController.speed = 1f;
        

    }
    private static void NetworkUpperbodyLeaningStateChange(Changed<KnightCommanderAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat("LeanDirection", changed.Behaviour.LeanDirection);
    }

    private static void NetworkUpperbodyVerticalStateChange(Changed<KnightCommanderAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat("VerticalDirection", changed.Behaviour.VerticalDirection);
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
        //changed.Behaviour._animationController.SetBool("OnPlayerWalk", changed.Behaviour.OnPlayerWalk);
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
        if (changed.Behaviour.IsPlayerGetDamage == true && changed.Behaviour.IsPlayerHoldingBomb) return;
        {
            changed.Behaviour._animationController.Play("KnightCommander-Damage");
        }
    }

    private static void NetworkedStunnedAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerStunned == false) return;

        
        changed.Behaviour._animationController.Play("KnightCommander-StunUpperBody", 1);
        changed.Behaviour._animationController.Play("KnightCommander-StunBackwards", 0);
    }
    private static void NetworkedThrowingBombAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("IsPlayerHoldingBomb", changed.Behaviour.IsPlayerHoldingBomb);
    }

    private static void NetworkParryAnimationStateChange(Changed<KnightCommanderAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerParry == false) return;
        if (changed.Behaviour.IsPlayerStunned == true) return;
        if (changed.Behaviour.BlockIndex == 1)
            changed.Behaviour._animationController.Play("KnightCommander-RightParry");
        else
            changed.Behaviour._animationController.Play("KnightCommander-LeftParry");
    }

   
    private static void NetworkStunExitTransitionChange(Changed<KnightCommanderAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("CanStunExit", changed.Behaviour.CanStunExit);
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
   
    public async override void UpdateStunAnimationState(int stunDuration)
    {
        UpdateBlockAnimState(0);
       IsPlayerStunned = true;
       await UniTask.Delay(stunDuration);
       CanStunExit = true;
       await UniTask.Delay(50);
        CanStunExit = false;
       StartCoroutine(WaitStunnedAnimation());
    }
    public override void UpdateThrowingAnimation(bool state)
    {
        IsPlayerHoldingBomb = state;
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
