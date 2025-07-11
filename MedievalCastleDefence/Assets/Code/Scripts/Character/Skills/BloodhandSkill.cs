using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;

public class BloodhandSkill : CharacterRegistry, IReadInput, IAbility
{
    [Networked(OnChanged = nameof(OnNetworkAbilityStateChange))] public NetworkBool IsAbilityInUseLocal { get; set; }
    [Networked] public NetworkBool IsAbilityInUse { get; set; }
    [SerializeField] private LayerMask _obstacleLayer;
    private CharacterMovement _characterMovement;
    private BloodhandVFXController _playerVFX;
    public NetworkButtons PreviousButton { get; set; }
    private GallowglassAttack _bloodhandAttack;
    public bool CanUseAbility { get; private set; }
   

    [SerializeField] private float _earthShatterRadius = 8f;
    [SerializeField] private float _earthShatterAngle = 90f;
    [SerializeField] private float _abilityCooldown = 40f;
    private GallowglassAnimation _gallowAnimation;
    private PlayerStatsController _playerStatsController;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }
    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        CanUseAbility = true;
        _bloodhandAttack = GetScript<GallowglassAttack>();
        _characterMovement = GetScript<CharacterMovement>();
        _playerVFX = GetScript<BloodhandVFXController>();
        _gallowAnimation = GetScript<GallowglassAnimation>();
        _playerStatsController = GetScript<PlayerStatsController>();
    }

    private static void OnNetworkAbilityStateChange(Changed<BloodhandSkill> changed)
    {
        changed.Behaviour.IsAbilityInUse = changed.Behaviour.IsAbilityInUseLocal;
    }
    private void Update()
    {
        Debug.Log("IsplayerUseAbility: " + IsAbilityInUse);
    }
    public async void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
        if (_characterMovement != null && _characterMovement.IsInputDisabled)
        {
            return;
        }

        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && CanUseAbility)
        {
            CanUseAbility = false;
            _characterMovement.IsInputDisabled = true;
            IsAbilityInUseLocal = true;

            _gallowAnimation.UpdateUltimateAnimState(true);
            _playerVFX.PlaySpritualVFXRpc();
            await UniTask.Delay(500);

            CastGroundShatterSkill();
            await UniTask.Delay(100);

            _playerVFX.PlayEarthShatterVFXRpc();
          
            await UniTask.Delay(400);

            _characterMovement.IsInputDisabled = false;
            IsAbilityInUseLocal = false;
            _gallowAnimation.UpdateUltimateAnimState(false);

            // Cooldown baþlat
            await StartCooldown();
        }

        PreviousButton = input.NetworkButtons;
    }

    private async UniTask StartCooldown()
    {
        await UniTask.Delay((int)(_abilityCooldown * 1000)); 
        CanUseAbility = true;
    }

    private void CastGroundShatterSkill()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, _earthShatterRadius);
        HashSet<NetworkId> processedIds = new HashSet<NetworkId>();
        Gizmos.color = Color.blue;

        foreach (Collider collider in targetsInRadius)
        {
            var detectedTransform = collider.GetComponentInParent<Transform>();
            var networkObject = detectedTransform.GetComponentInParent<NetworkObject>();
            if (networkObject == null) continue;
            var damageable = detectedTransform.GetComponentInParent<IDamageable>();
            var stats = detectedTransform.GetComponentInParent<PlayerStatsController>();
            if (damageable == null || stats == null) continue;
            if (networkObject.Id == transform.GetComponentInParent<NetworkObject>().Id) continue;
            if (stats.PlayerTeam == _playerStatsController.PlayerTeam) continue;
            if (processedIds.Contains(networkObject.Id)) continue;
            processedIds.Add(networkObject.Id);
            if (detectedTransform.position.y > 3f) continue;
            Vector3 dirToTarget = (detectedTransform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            if (angleToTarget > _earthShatterAngle / 2) continue;
            float distanceToTarget = Vector3.Distance(transform.position, detectedTransform.position);
            bool hasObstacle = Physics.Raycast(transform.position, dirToTarget, distanceToTarget, _obstacleLayer);
            float distance = Vector3.Distance(new Vector3(detectedTransform.position.x, transform.position.y, transform.position.z), detectedTransform.position);

            var opponentStamina = detectedTransform.GetComponentInParent<CharacterStamina>();
            var opponentWarrior = detectedTransform.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerWarrior;

            if (!hasObstacle)
            {
                if (distance < 2.36f)
                {
                    Debug.Log("Diff bigger less 2");
                    opponentStamina.StunPlayerRpc(3);
                }
                else
                {
                    Debug.Log("Diff bigger than 3");
                    opponentStamina.StunPlayerRpc(2);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 forwardEdge = transform.forward * _earthShatterRadius;
        Vector3 leftEdge = Quaternion.Euler(0, -_earthShatterAngle / 2, 0) * forwardEdge;
        Vector3 rightEdge = Quaternion.Euler(0, _earthShatterAngle / 2, 0) * forwardEdge;

        Gizmos.DrawLine(transform.position, transform.position + leftEdge);
        Gizmos.DrawLine(transform.position, transform.position + rightEdge);

        int segments = 20;
        Vector3 prevPoint = transform.position + leftEdge;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -_earthShatterAngle / 2 + (i * _earthShatterAngle / segments);
            Vector3 newPoint = transform.position + Quaternion.Euler(0, angle, 0) * forwardEdge;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}