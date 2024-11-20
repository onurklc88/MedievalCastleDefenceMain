using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ArcheryAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private CharacterMovement _characterMovement;
    private CharachterCameraController _camController;
    [SerializeField] private GameObject _lookAtTarget;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _arrowFirePoint;
 
  
    private float _drawDuration;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.Ranger;
        InitScript(this);
        _drawDuration = 0.1f;
    }
    private void Start()
    {
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
        //_knightCommanderAnimation = GetScript<KnightCommanderAnimation>();
        _characterMovement = GetScript<CharacterMovement>();
        _camController = GetScript<CharachterCameraController>();
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
        var _isPlayerAiming = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        _characterMovement.IsInputDisabled = _isPlayerAiming;
        _camController.UpdateCameraPriority(_isPlayerAiming);
        _playerHUD.UpdateAimTargetState(_isPlayerAiming);
        UpdateTargetPosition();
        CalculateDrawDuration(_isPlayerAiming);
    }

    private void UpdateTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * 10f;
        _lookAtTarget.transform.position = Vector3.Lerp(_lookAtTarget.transform.position, endPoint, Time.deltaTime * 50f);

    }

    private void RelaseArrow()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * 10f;
        var arrow = Runner.Spawn(_arrow, _arrowFirePoint.transform.position, _lookAtTarget.transform.rotation, Runner.LocalPlayer);
        var arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.ExecuteShot(ray.direction);
    }

   
    private void CalculateDrawDuration(bool condition)
    {
        if (condition)
        {
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
            _drawDuration = 0.1f;
        }
    }

    
    private void CheckAttackCollision(GameObject collidedObject)
    {
        if (collidedObject.transform.GetComponentInParent<NetworkObject>() == null) return;
        if (collidedObject.transform.GetComponentInParent<NetworkObject>().Id == transform.GetComponentInParent<NetworkObject>().Id) return;


        if (collidedObject.transform.GetComponentInParent<IDamageable>() != null)
        {
            var opponentType = base.GetCharacterType(collidedObject);
            switch (opponentType)
            {
                case CharacterStats.CharacterType.None:

                    break;
                case CharacterStats.CharacterType.FootKnight:
                    DamageToFootknight(collidedObject, _weaponStats.Damage);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    DamageToKnightCommander(collidedObject, _weaponStats.Damage);
                    break;

            }
        }
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
