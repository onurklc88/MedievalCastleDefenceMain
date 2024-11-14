using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ArcheryAttack : CharacterAttackBehaviour
{
    private PlayerHUD _playerHUD;
    private KnightCommanderAnimation _knightCommanderAnimation;
    private CharacterMovement _characterMovement;
    private CharachterCameraController _camController;
    [SerializeField] private GameObject _aimTarget;
    [SerializeField] private GameObject _lookAtTarget;
    [SerializeField] private GameObject _arrow;
    private bool _isPlayerAiming;
    private bool _isReadyToThrow;
    private float _drawDuration;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.FootKnight;
        InitScript(this);
        _drawDuration = 2f;
    }
    private void Start()
    {
        _playerHUD = GetScript<PlayerHUD>();
        _characterStamina = GetScript<CharacterStamina>();
        _knightCommanderAnimation = GetScript<KnightCommanderAnimation>();
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
        if (_characterMovement == null || _camController == null || _isReadyToThrow) return;
        _isPlayerAiming = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        _camController.UpdateCameraPriority(_isPlayerAiming);
        _aimTarget.SetActive(_isPlayerAiming);
        UpdateTargetPosition();
        if (_isPlayerAiming)
        {
           if(_drawDuration > 0)
           {
                _drawDuration -= Time.deltaTime;
                Debug.Log("Test: " + _drawDuration);
           }
        }

        if (!_isPlayerAiming)
        {
            if(_drawDuration <= 0)
            {
                RelaseArrow();
            }
            _drawDuration = 2f;
        }
      
      
    }

    private void UpdateTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = _lookAtTarget.transform.position;
        float calculatedY = ray.origin.y + ray.direction.y * 10f;
        targetPosition.y = Mathf.Min(calculatedY, 2f);
       _lookAtTarget.transform.position = Vector3.Lerp(_lookAtTarget.transform.position, targetPosition, Time.deltaTime * 100f);
  
    }

    private void RelaseArrow()
    {
        _drawDuration = 2f;
        Debug.Log("relaseArrow");
    }

   
    private void CalculateDrawDuration()
    {
        
        if(_drawDuration > 0 && _isReadyToThrow)
        {
            _drawDuration -= Time.deltaTime;
        }
        else
        {
            _isReadyToThrow = true;
          
            StartCoroutine(UpdateReadyToAým());
            _drawDuration = 2f;
        }
    }

    private IEnumerator UpdateReadyToAým()
    {
        yield return new WaitForSeconds(1f);
        _isReadyToThrow = false;
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
