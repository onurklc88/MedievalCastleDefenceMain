using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
public class ArcheryAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private CharacterMovement _characterMovement;
    private CharacterCameraController _camController;
    private RangerAnimation _rangerAnimation;
    private RangerAnimationRigging _rangerAnimationRigging;
   
    [SerializeField] private GameObject _lookAtTarget;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _arrowFirePoint;
    [SerializeField] private GameObject _angle;
    private bool _previousAimingInput;
    private ActiveRagdoll _activeRagdoll;
    private Vector3 _defaultAnglePosition;
    private Vector3 _aimingAnglePosition = new Vector3(0.032f, 0.289f, 0.322f);
    private bool _canDrawArrow = true;
    private float _drawDuration;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.Ranger;
        InitScript(this);
        _drawDuration = .2f;
        _defaultAnglePosition = _angle.transform.localPosition;
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
    }
    public override void FixedUpdateNetwork()
    {

        if (!Object.HasStateAuthority) return;
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }
    }

    public override void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
        if (_characterMovement == null || _camController == null) return;
        var isPlayerAiming = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (isPlayerAiming != _previousAimingInput && _characterCollision.IsPlayerGrounded && _canDrawArrow)
        {
            
          
           
            _rangerAnimationRigging.IsPlayerAiming = isPlayerAiming;
          
            //_camController.UpdateCameraPriority(isPlayerAiming);
            UpdateTargetPosition();

        }
        Debug.Log("TEST: " + _rangerAnimation.GetCurrentPlayingAnimationClipName());
        if (_characterCollision.IsPlayerGrounded && _canDrawArrow)
        {
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

    private void RelaseArrow()
    {
       // Debug.Log("PAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        StartDrawCooldown().Forget();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * 10f;
        var arrow = Runner.Spawn(_arrow, _arrowFirePoint.transform.position, _lookAtTarget.transform.rotation, Runner.LocalPlayer);
        var arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.ExecuteShot(ray.direction);
    }

   
    private void CalculateDrawDuration(bool condition)
    {
        Debug.Log("TEST: " + _rangerAnimation.GetCurrentPlayingAnimationClipName());
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
                
                RelaseArrow();
            }
            else
            {
                _rangerAnimation.UpdateDrawAnimState(false);
                _rangerAnimation.UpdateIdleAnimationState();
            }
            _rangerAnimation.UpdateDrawAnimState(false);
           // _rangerAnimation.DisableDummyArrows();
            _drawDuration = 0.2f;
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
