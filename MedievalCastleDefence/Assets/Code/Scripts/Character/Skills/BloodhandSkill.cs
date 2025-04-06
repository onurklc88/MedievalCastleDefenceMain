using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;



public class BloodhandSkill : CharacterRegistry, IReadInput
{
    [SerializeField] private LayerMask _obstacleLayer;
    private CharacterMovement _characterMovement;
   private PlayerVFXSytem _playerVFX;
    [Networked] private TickTimer _kickCooldown { get; set; }
    public NetworkButtons PreviousButton { get; set; }
    private GallowglassAttack _bloodhandAttack;
    public bool CanUseAbility { get; private set; }
    [SerializeField] private GameObject _test;
    [SerializeField] private float _earthShatterRadius = 8f;
    [SerializeField] private float _earthShatterAngle = 90f;
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
        _playerVFX = GetScript<PlayerVFXSytem>();
        _gallowAnimation = GetScript<GallowglassAnimation>();
        _playerStatsController = GetScript<PlayerStatsController>();
    }

   


    public async void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
        if (_characterMovement != null && _characterMovement.IsInputDisabled)
        {
            //IsPlayerBlocking = false;
            //_gallowGlassAnimation.IsPlayerParry = IsPlayerBlocking;
            return;
        }

        var attackButton = input.NetworkButtons.GetPressed(PreviousButton);
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.UltimateSkill) && CanUseAbility)
        {
             CanUseAbility = false;
            _characterMovement.IsInputDisabled = true;
            
            _gallowAnimation.UpdateUltimateAnimState(true);
            await UniTask.Delay(500);
            CastGroundShatterSkill();
            await UniTask.Delay(500);
            _characterMovement.IsInputDisabled = false;
            CanUseAbility = true;
           
            _gallowAnimation.UpdateUltimateAnimState(false);
            //_playerVFX.PlayUltimateVFX();
        }
       
        PreviousButton = input.NetworkButtons;
    }

    private void CastGroundShatterSkill()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, _earthShatterRadius);
        HashSet<NetworkId> processedIds = new HashSet<NetworkId>();
        Gizmos.color = Color.blue;

        foreach (Collider collider in targetsInRadius)
        {
            // Debug.Log("A");
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
            //GameObject debug = Instantiate(_test);
            //debug.transform.position = new Vector3(detectedTransform.position.x, transform.position.y, transform.position.z);
            //GameObject debug2 = Instantiate(_test);
            //debug2.transform.position = new Vector3(detectedTransform.position.x, transform.position.y, detectedTransform.position.z);
            Debug.Log("Character: " + networkObject.Id + " distance: " + distance);

            var opponentStamina = detectedTransform.GetComponentInParent<CharacterStamina>();
            if (!hasObstacle)
            {
                if(distance < 2.36f)
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
        if (!Application.isPlaying) return;

        //Gizmos.color = new Color(0, 0, 1, 0.5f);
        //Gizmos.DrawWireSphere(transform.position, _earthShatterRadius);

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
