using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
public class ArcheryAttack : CharacterAttackBehaviour, IReadInput
{
    [SerializeField] private GameObject _standartArrow;
    [SerializeField] private GameObject _bombArrow;
    [SerializeField] private GameObject _stunArrow;
    [SerializeField] private GameObject _lookAtTarget;
    [SerializeField] private GameObject _arrowFirePoint;
    [SerializeField] private GameObject _angle;
    private PlayerHUD _playerHUD;
    private CharacterMovement _characterMovement;
    private CharacterCameraController _camController;
    private RangerAnimation _rangerAnimation;
    private RangerAnimationRigging _rangerAnimationRigging;
    private PlayerStatsController _playerStats;
    private bool _previousAimingInput;
    private ActiveRagdoll _activeRagdoll;
    private Vector3 _defaultAnglePosition;
    private Vector3 _aimingAnglePosition = new Vector3(0.032f, 0.289f, 0.322f);
    private bool _canDrawArrow = true;
    private float _drawDuration;
    private enum ArrowType
    {
        None,
        Standart,
        Bomb,
        Stun
    }
    private ArrowType _selectedArrowType;
    private const int DEFAULT_BOMB_ARROW_AMOUNT = 2;
    private const int DEFAULT_STUN_ARROW_AMOUNT = 2;
    private int _currentBombArrowAmount;
    private int _currentStunArrowAmount;
   
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.Ranger;
        InitScript(this);
        _drawDuration = .3f;
        _defaultAnglePosition = _angle.transform.localPosition;
        _currentBombArrowAmount = DEFAULT_BOMB_ARROW_AMOUNT;
        _currentStunArrowAmount = DEFAULT_STUN_ARROW_AMOUNT;
        _selectedArrowType = ArrowType.Standart;
    }
    private void Start()
    {
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
        _rangerAnimationRigging = GetScript<RangerAnimationRigging>();
        _rangerAnimation = GetScript<RangerAnimation>();
        _characterMovement = GetScript<CharacterMovement>();
        _camController = GetScript<CharacterCameraController>();
        _activeRagdoll = GetScript<ActiveRagdoll>();
        _characterCollision = GetScript<CharacterCollision>();
        _playerStats = GetScript<PlayerStatsController>();
        _characterHealth = GetScript<CharacterHealth>();
    }
    public override void FixedUpdateNetwork()
    {

        if (!Object.HasStateAuthority) return;
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
            PreviousButton = input.NetworkButtons;
        }
    }

    public override void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
        if (_characterMovement == null || _camController == null) return;
        var isPlayerAiming = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        
        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill))
        {
            SwitchArrowType();
        }

        if (isPlayerAiming != _previousAimingInput && _characterCollision.IsPlayerGrounded && _canDrawArrow)
        {
            _rangerAnimationRigging.IsPlayerAiming = isPlayerAiming;
        }
       
        if (_characterCollision.IsPlayerGrounded && _canDrawArrow && !_characterHealth.IsPlayerDead && _characterStamina.CurrentAttackStamina > 30)
        {
            if (_characterMovement.IsPlayerSlowed)
            {
                if (_rangerAnimation.GetCurrentAnimationState("UpperBody") == "Slowed1") return;
            }
            UpdateTargetPosition();
            _camController.UpdateCameraPriority(isPlayerAiming);
            // UpdateTargetPosition();
            _angle.transform.localPosition = isPlayerAiming ? _aimingAnglePosition : _defaultAnglePosition;
            _playerHUD.UpdateAimTargetState(isPlayerAiming);
            CalculateDrawDuration(isPlayerAiming);
        }
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Reload))
        {
            _activeRagdoll.RPCActivateRagdoll();
        }
       
        _previousAimingInput = isPlayerAiming;
      
    }

    private void UpdateTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * 10f;
        _lookAtTarget.transform.position = Vector3.Lerp(_lookAtTarget.transform.position, endPoint, Time.deltaTime * 50f);
    }

    private void SwitchArrowType()
    {

        if (_selectedArrowType == ArrowType.Standart)
        {
            if (_currentBombArrowAmount > 0)
            {
                _selectedArrowType = ArrowType.Bomb;
            }
            else if (_currentStunArrowAmount > 0)
            {
                _selectedArrowType = ArrowType.Stun;
            }
        }
        else if (_selectedArrowType == ArrowType.Bomb)
        {
            if (_currentStunArrowAmount > 0)
            {
                _selectedArrowType = ArrowType.Stun;
            }
            else
            {
                _selectedArrowType = ArrowType.Standart;
            }
        }
        else if (_selectedArrowType == ArrowType.Stun)
        {
            _selectedArrowType = ArrowType.Standart;
        }

        Debug.Log("SelectedArrowType: " + _selectedArrowType);

    }

    private void RelaseArrow()
    {
        StartDrawCooldown().Forget();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * 10f;
        Arrow arrowScript = null;
       

        if(_currentBombArrowAmount > 0 && _selectedArrowType == ArrowType.Bomb)
        {
            _currentBombArrowAmount -= 1;
            var arrow = Runner.Spawn(_bombArrow, _arrowFirePoint.transform.position, _lookAtTarget.transform.rotation, Runner.LocalPlayer);
            arrowScript = arrow.GetComponent<BombArrow>();
        }
        else if(_currentStunArrowAmount > 0 && _selectedArrowType == ArrowType.Stun)
        {
            _currentStunArrowAmount -= 1;
            var arrow = Runner.Spawn(_stunArrow, _arrowFirePoint.transform.position, _lookAtTarget.transform.rotation, Runner.LocalPlayer);
            arrowScript = arrow.GetComponent<StunArrow>();
        }
        else
        {
            var arrow = Runner.Spawn(_standartArrow, _arrowFirePoint.transform.position, _lookAtTarget.transform.rotation, Runner.LocalPlayer);
            arrowScript = arrow.GetComponent<StandartArrow>();
        }
        if (arrowScript == null)
        {
            Debug.LogError("StandartArrow scripti bulunamadý!");
            return;
        }
        arrowScript.SetOwner(_playerStats.PlayerNetworkStats); 
        arrowScript.ExecuteShot(ray.direction);
       
    }

   
    private void CalculateDrawDuration(bool condition)
    {
        if (!Object.HasStateAuthority) return;
        if (condition)
        {
            _rangerAnimation.UpdateDrawAnimState(true);
            if (_drawDuration > 0)
            {
                _drawDuration -= Time.deltaTime;
              
            }
        }
        else
        {

            if (_drawDuration <= 0)
            {
                _characterStamina.DecreaseCharacterAttackStamina(_weaponStats.StaminaWaste);
                RelaseArrow();
            }
            else
            {
                _rangerAnimation.UpdateDrawAnimState(false);
                _rangerAnimation.UpdateIdleAnimationState();
            }
            _rangerAnimation.UpdateDrawAnimState(false);
         
            _drawDuration = 0.3f;
        }

       
    }

    private async UniTaskVoid StartDrawCooldown()
    {
        _canDrawArrow = false;

        await UniTask.WaitUntil(
            () => _rangerAnimation.GetCurrentPlayingAnimationClipName() == "Idle-RangerV2",
            cancellationToken: this.GetCancellationTokenOnDestroy()
        ); 

        _canDrawArrow = true;
    }
    void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * 10f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ray.origin, endPoint);
        Gizmos.DrawSphere(endPoint, 0.2f);
    }

}
