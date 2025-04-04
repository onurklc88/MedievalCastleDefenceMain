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
    [SerializeField] private float _earthShatterRadius = 7f;
    [SerializeField] private float _earthShatterAngle = 90f;
    private GallowglassAnimation _gallowAnimation;
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
            CastGroundShatterSkill();
            //StartBloodhandCooldown().Forget();
            await UniTask.Delay(1000);
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
        Gizmos.color = Color.blue;

        foreach (Collider target in targetsInRadius)
        {
            var targetObject = target.transform.GetComponentInParent<Transform>();
            if (targetObject.position.y > 1) continue;
            var opponentStamina = targetObject.GetComponentInParent<CharacterStamina>();
            /*
            var opponentID = targetObject.GetComponentInParent<NetworkObject>().Id;
            if (targetObject.GetComponentInParent<IDamageable>() == null || opponentStamina == null || opponentID == transform.GetComponent<NetworkObject>().Id) continue;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            if (angleToTarget > _earthShatterAngle / 2) continue;
           
           
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            bool hasObstacle = Physics.Raycast(transform.position, dirToTarget, distanceToTarget, _obstacleLayer);

            if (!hasObstacle)
            {
                opponentStamina.StunPlayerRpc(5);
            }

           Debug.Log("Detected: " + target.transform.gameObject.name);
            */
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
