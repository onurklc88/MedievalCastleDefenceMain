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
    private float _earthShatterRadius = 7f;         // Pasta dilimi yarýçapý
    private float _earthShatterAngle = 90f;         // Pasta dilimi açýsý (örneðin 90° = 45° sað + 45° sol)
   
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
        if (attackButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Jump) && CanUseAbility && input.HorizontalInput == 0 && input.VerticalInput >= 0)
        {
             CanUseAbility = false;
            _characterMovement.IsInputDisabled = true;
           
            _bloodhandAttack.KickAction();
            StartBloodhandCooldown().Forget();
            await UniTask.Delay(600);
            _playerVFX.PlayUltimateVFX();
        }
       
        PreviousButton = input.NetworkButtons;
    }

    private void CastEarthshatterSkill()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, _earthShatterRadius);
        Gizmos.color = Color.blue;

        foreach (Collider target in targetsInRadius)
        {
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

           
            if (angleToTarget > _earthShatterAngle / 2) continue;

           
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            bool hasObstacle = Physics.Raycast(transform.position, dirToTarget, distanceToTarget, _obstacleLayer);

            if (!hasObstacle)
            {
                Gizmos.DrawSphere(target.transform.position, 0.5f); 
                Gizmos.DrawLine(transform.position, target.transform.position); 
            }
        }
    }
    
    private async UniTaskVoid StartBloodhandCooldown()
    {
        await UniTask.Delay(100);
        CanUseAbility = true;
    }

   
}
