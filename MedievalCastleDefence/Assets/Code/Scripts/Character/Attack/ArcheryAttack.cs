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
    private bool _isPlayerAiming;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _characterController = GetComponent<CharacterController>();
        _characterType = CharacterStats.CharacterType.FootKnight;
        InitScript(this);
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
        if (_characterMovement == null || _camController == null) return;
        _isPlayerAiming = input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Mouse1);
        _camController.UpdateCameraPriority(true);
        _aimTarget.SetActive(true);
        if (_isPlayerAiming)
            _camController.HandleArcheryCameraAction();
        //IsPlayerBlockingLocal = true;

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
}
