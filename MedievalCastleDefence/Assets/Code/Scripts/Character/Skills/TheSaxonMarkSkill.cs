using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;


public class TheSaxonMarkSkill : CharacterRegistry, IReadInput, IAbility
{
    public NetworkButtons PreviousButton { get; set; }
    [Networked(OnChanged = nameof(OnNetworkDashStateChange))] public NetworkBool IsAbilityInUseLocal { get; set; }
    [Networked] public NetworkBool IsAbilityInUse { get; set; }

    public int SlideCharges { get; set; }
    private CharacterStamina _characterStamina;
    private PlayerHUD _playerHUD;
    private PlayerVFXSytem _characterVFX;
    private CharacterController _characterController;
    private Rigidbody _rigidbody;
    private CharacterMovement _characterMovement;
    private int _slideChargeCount;
    private float _duration = 0.2f;
    private float _distance = 0.35f;

    private Vector3 _slideDirection;
    private const int MAX_SLIDE_CHARGE_COUNT = 1;
    private bool _isRefilling = false;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _rigidbody = GetComponent<Rigidbody>();
        InitScript(this);
        if (!Runner.IsSharedModeMasterClient)
            _distance = 0.17f;

    }

    private void Start()
    {
        _characterMovement = GetScript<CharacterMovement>();
        _characterStamina = GetScript<CharacterStamina>();
        _playerHUD = GetScript<PlayerHUD>();
        _characterVFX = GetScript<PlayerVFXSytem>();
        _slideChargeCount = MAX_SLIDE_CHARGE_COUNT;
     
        if (_playerHUD != null)
            _playerHUD.UpdateSlideChargeCount(_slideChargeCount);

    }


    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || _characterMovement == null) return;

        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input) && !_characterMovement.IsInputDisabled)
        {
            ReadPlayerInputs(input);
            //Debug.Log("IsplayerDashLocal: " + IsPlayerUseAbility);
        }

    }
    private static void OnNetworkDashStateChange(Changed<TheSaxonMarkSkill> changed)
    {
        changed.Behaviour.IsAbilityInUse = changed.Behaviour.IsAbilityInUseLocal;
    }

    public void ReadPlayerInputs(PlayerInputData input)
    {
        #region legacy
        /*
        if (input.ForwardDoubleTab)
            _slideDirection = transform.forward;
        else if (input.RightDoubleTab)
            _slideDirection = transform.right;
        else if (input.LeftDoubleTab)
            _slideDirection = -transform.right;
        else if (input.BacwardsDoubleTab)
            _slideDirection = -transform.forward;
        else
            return;

        

        ActivateUtilitySkill();
        PreviousButton = input.NetworkButtons;
        */
        #endregion
        var utilityButton = input.NetworkButtons.GetPressed(PreviousButton);
      
        if (utilityButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UtilitySkill))
        {
            if (_characterMovement.IsInputDisabled) return;
           Vector3 direction = new Vector3(input.HorizontalInput, 0f, input.VerticalInput);

            if (direction.sqrMagnitude < 0.1f)
                return;

            _slideDirection = transform.TransformDirection(direction.normalized);
            ActivateUtilitySkill();

        }
        PreviousButton = input.NetworkButtons;

    }

    private void ActivateUtilitySkill()
    {
        if (_slideChargeCount > 0 && _characterStamina.CurrentAttackStamina > 10)
        {
            _slideChargeCount -= 1;
            _playerHUD.UpdateSlideChargeCount(_slideChargeCount);
            _characterStamina.DecreaseCharacterAttackStamina(10f);
            SlideCharacter(_slideDirection).Forget();

            if (_slideChargeCount < 1 && !_isRefilling)
            {
                RechargeSlideAsync().Forget();
            }
        }
        else
        {
            // Debug.Log("No slide charges left! Waiting for recharge...");
        }
    }

    private async UniTaskVoid RechargeSlideAsync()
    {
        _isRefilling = true;

        while (_slideChargeCount < 1)
        {
            await UniTask.Delay(10000);
            _slideChargeCount += 1;
            _playerHUD.UpdateSlideChargeCount(_slideChargeCount);
        }

        _isRefilling = false;
    }

    private async UniTaskVoid SlideCharacter(Vector3 direction)
    {
        if (IsAbilityInUseLocal) return;

        if (_slideChargeCount < 0) return;
        IsAbilityInUseLocal = true;
        _characterVFX.ActivateSwordTrail(IsAbilityInUseLocal);
        EventLibrary.OnPlayerDash.Invoke(IsAbilityInUseLocal);

        float elapsedTime = 0f;
        float forceMultiplier = 1700f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            // Kuvveti kademeli azalt (elapsedTime ile ters orantýlý)
            float currentForce = forceMultiplier * (1 - (elapsedTime / duration));
            _rigidbody.AddForce(direction * currentForce, ForceMode.Acceleration);

            elapsedTime += Runner.DeltaTime;
            await UniTask.Yield();
        }
        _characterVFX.ActivateSwordTrail(false);
        await UniTask.Delay(200);
        IsAbilityInUseLocal = false;
        EventLibrary.OnPlayerDash.Invoke(IsAbilityInUseLocal);
        await UniTask.Delay(500);

      

    }
}
