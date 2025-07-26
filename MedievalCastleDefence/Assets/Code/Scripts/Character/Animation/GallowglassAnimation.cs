using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;


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
    [Networked(OnChanged = nameof(NetworkedAbilityAnimationStateChange))] public NetworkBool IsPlayerUseAbility { get; set; }
    [Networked(OnChanged = nameof(NetworkAttackAnimationStateChange))] public int SwingIndex { get; set; }
    [Networked(OnChanged = nameof(NetworkStunExitTransitionChange))] public NetworkBool CanStunExit { get; set; }
    [Networked(OnChanged = nameof(NetworkUpperbodyLeaningStateChange))] public float LeanDirection { get; set; }
    [Networked(OnChanged = nameof(NetworkedThrowingBombAnimationStateChange))] public NetworkBool IsPlayerHoldingBomb { get; set; }
    [Networked(OnChanged = nameof(NetworkUpperbodyVerticalStateChange))] public float VerticalDirection { get; set; }
    public NetworkButtons PreviousButton { get; set; }
    private CharacterMovement _characterMovement;
    private GallowglassAttack _gallowAttack;
    [SerializeField] private float _leanSmoothTime = 0.5f;
    private float _currentLeanVelocity;

    private float _verticalDirectionVelocity;
    public Animator BloodHandAnimator { get; private set; }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        BloodHandAnimator = _animationController;
        InitScript(this);
        
    }
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _characterMovement = GetScript<CharacterMovement>();
        _gallowAttack = GetScript<GallowglassAttack>();
        _animationController.SetLayerWeight(4, 0.35f);
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
        if (_characterMovement.IsInputDisabled) return;

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
        
        VerticalDirection = Mathf.SmoothDamp(
           current: VerticalDirection,
           target: input.VerticalInput,
           currentVelocity: ref _verticalDirectionVelocity,
           smoothTime: 0.2f
       );
        if (input.HorizontalInput != 0)
        {
            PlayerHorizontalDirection = input.HorizontalInput;
        }
        else
        {
            PlayerHorizontalDirection = 0;
        }
        UpperBodyLeaning();

        PreviousButton = input.NetworkButtons;
    }
    private void UpperBodyLeaning()
    {
        if (_gallowAttack.IsPlayerBlockingLocal == true || _characterMovement.IsInputDisabled) return;
        float target = (_gallowAttack.PlayerSwordPositionLocal == CharacterAttackBehaviour.SwordPosition.Right) ? 1f : -1f;
        float current = _animationController.GetFloat("LeanDirection");
        float smoothed = Mathf.SmoothDamp(current, target, ref _currentLeanVelocity, _leanSmoothTime);
        LeanDirection = smoothed;
        //_animationController.SetFloat("LeanDirection", smoothed);
    }
    private static void NetworkUpperbodyVerticalStateChange(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat("VerticalDirection", changed.Behaviour.VerticalDirection);
    }
    private static void NetworkUpperbodyLeaningStateChange(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetFloat("LeanDirection", changed.Behaviour.LeanDirection);
        /*
        if(changed.Behaviour.LeanDirection < -0.05f)
        {
           
            changed.Behaviour._animationController.CrossFade("GG_LS", 0.3f);
        }
        */
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
        //changed.Behaviour._animationController.SetBool("OnPlayerWalk", changed.Behaviour.OnPlayerWalk);
    }
    private static void NetworkedUpperbodyBlockAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
       changed.Behaviour._animationController.SetInteger("BlockIndex", changed.Behaviour.BlockIndex);
    }

    private static void NetworkedThrowingBombAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
       
        changed.Behaviour._animationController.SetBool("IsPlayerHoldingBomb", changed.Behaviour.IsPlayerHoldingBomb);
    }
    private static void NetworkAttackAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetInteger("SwingIndex", changed.Behaviour.SwingIndex);
        
        if (changed.Behaviour.SwingIndex == 1)
        {
            changed.Behaviour._animationController.CrossFade("Gallowglass-RightSwing", 0.1f);
        }
        else if (changed.Behaviour.SwingIndex == 2)
        {
            changed.Behaviour._animationController.SetLayerWeight(5, 0f);
            changed.Behaviour._animationController.CrossFade("Gallowglass-LeftSwing", 0f);
        }
        else
        {
          
            if (changed.Behaviour.HasStateAuthority)
            {
                changed.Behaviour.StartCoroutine(DelayedWeightChange(changed.Behaviour, 0.5f));
            }
        }
    }
    private static IEnumerator DelayedWeightChange(GallowglassAnimation behaviour, float duration)
    {
        yield return new WaitForSeconds(duration); 
        behaviour._animationController.SetLayerWeight(5, 0.47f);
        behaviour._animationController.Play("CleanState", 5);
        behaviour.RPC_SyncLayerWeight(5, 0.47f); 
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncLayerWeight(int layerIndex, float weight) // ! async YOK
    {
        _animationController.SetLayerWeight(layerIndex, weight);
    }
    /*
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private async void RPC_SyncLayerWeight(int layerIndex, float weight)
    {
        await UniTask.Delay(500);
        _animationController.SetLayerWeight(layerIndex, weight);

    }
    */
    private static void NetworkedDamageAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerGetDamage == true && changed.Behaviour.IsPlayerHoldingBomb) return;
        {
            //changed.Behaviour._animationController.Play("GallowGlass-Ultimate", 2);
            changed.Behaviour._animationController.Play("Gallowglass-Damage");
        }
    }

    private static void NetworkStunExitTransitionChange(Changed<GallowglassAnimation> changed)
    {
        changed.Behaviour._animationController.SetBool("CanStunExit", changed.Behaviour.CanStunExit);
    }
    private static void NetworkedAbilityAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        if(changed.Behaviour.IsPlayerUseAbility == true)
        {
            changed.Behaviour._animationController.SetLayerWeight(5, 0f);
            changed.Behaviour._animationController.Play("GallowGlass-Ultimate", 0);
            changed.Behaviour._animationController.Play("GallowGlass-Ultimate", 1);
            changed.Behaviour._animationController.Play("GallowGlass-Ultimate", 2);
            if (changed.Behaviour.HasStateAuthority)
            {
                changed.Behaviour.StartCoroutine(DelayedWeightChange(changed.Behaviour, 1.2f));
            }
        }
    }

    private static void NetworkedStunnedAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerStunned == false) return;
        
        changed.Behaviour._animationController.Play("Gallowglass-Stun", 1);
        changed.Behaviour._animationController.Play("Gallowglass_Stun_Backwards");
    }
  

    private static void NetworkParryAnimationStateChange(Changed<GallowglassAnimation> changed)
    {
        if (changed.Behaviour.IsPlayerParry == false) return;
           changed.Behaviour._animationController.Play("GallowGlass-Ultimate", 2);
        if (changed.Behaviour.BlockIndex == 1)
            changed.Behaviour._animationController.Play("Gallowglass-RightParry");
        else
            changed.Behaviour._animationController.Play("Gallowglass-LeftParry");
    }
    public override void UpdateThrowingAnimation(bool state)
    {
        IsPlayerHoldingBomb = state;
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
        StartCoroutine(WaitAttack(0.1f));
    }
    public void UpdateUltimateAnimState(bool condition)
    {
        IsPlayerUseAbility = condition;
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
