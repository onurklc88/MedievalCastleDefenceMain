using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BehaviourRegistry;

public class KnightCommanderAnimationRigging : CharacterRegistry
{
    [SerializeField] private Transform _spine; // Spine bone'unu Inspector'dan ata
    [SerializeField] private float _leanAngle = 15f; // Yatma açýsý
    [SerializeField] private float _leanSpeed = 8f; // Yatma hýzý

    private KnightCommanderAttack _kcAttack;

    private Quaternion _targetRotation;

    public override void Spawned()
    {
        _kcAttack = GetScript<KnightCommanderAttack>();
    }

    [SerializeField] private float _smoothSpeed = 5f; // Yumuþak geçiþ hýzý (Inspector'dan ayarla)
    private float _currentLeanAngle = 0f; // Mevcut açýyý takip et

    void LateUpdate()
    {
        if (_spine == null || _kcAttack == null) return;

        // Hedef açýyý belirle
        float targetAngle = (_kcAttack.PlayerSwordPositionLocal == CharacterAttackBehaviour.SwordPosition.Right)
            ? -_leanAngle : _leanAngle;

        // Mevcut açýyý yumuþakça hedefe doðru ilerlet
        _currentLeanAngle = Mathf.Lerp(_currentLeanAngle, targetAngle, _smoothSpeed * Time.deltaTime);

        // Rotasyonu uygula
        _spine.localRotation = Quaternion.Euler(0, 0, _currentLeanAngle);
    }

}
