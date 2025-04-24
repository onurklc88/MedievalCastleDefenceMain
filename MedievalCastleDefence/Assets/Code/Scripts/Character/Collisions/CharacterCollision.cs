using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
public class CharacterCollision : CharacterRegistry
{
    public bool IsPlayerGrounded { get; private set; }
    private CharacterMovement _characterMovement;
    public override void Spawned()
    {
        InitScript(this);

    }

    private void Start()
    {
        _characterMovement = GetScript<CharacterMovement>();
    }
    public override void FixedUpdateNetwork()
    {
        CheckGroundLayers();
    }
    private void ShowOpponentWorldUI()
    {
        if (!Object.HasStateAuthority) return;
        int layerMask = ~LayerMask.GetMask("Ragdoll");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 50f, layerMask);
        for(int i = 0; i < hitColliders.Length; i++)
        {
            if(hitColliders[i].transform.GetComponentInParent<PlayerStatsController>() != null)
            {
                hitColliders[i].transform.GetComponentInParent<PlayerHUD>().ShowNickName(true);
            }
        }
    }

    private void CheckGroundLayers()
    {

        IsPlayerGrounded = Physics.Raycast(
          transform.position + Vector3.up * 0.1f,
          Vector3.down,
          out var _groundHit,
          0.3f
       );

        Debug.Log("IsplayerGorunded: " + IsPlayerGrounded);
        if (!IsPlayerGrounded) return;

        switch (_groundHit.transform.gameObject.layer)
        {
            case 8:
                _characterMovement.ThrowCharacter();
                break;
          
            
        }
        
    }


}
