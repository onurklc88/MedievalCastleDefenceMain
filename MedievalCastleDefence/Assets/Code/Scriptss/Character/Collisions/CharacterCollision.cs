using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterCollision : NetworkBehaviour
{
    private void Start()
    {

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


   

}
