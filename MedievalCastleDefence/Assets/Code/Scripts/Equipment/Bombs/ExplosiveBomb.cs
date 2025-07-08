using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class ExplosiveBomb : Bomb
{
   public override void Spawned()
    {
        StartCoroutine(DestroyObject(13f));
    }
    public override void FixedUpdateNetwork()
    {
        
        if (!IsObjectCollided)
        {
            transform.Rotate(Vector3.forward * 300f * Runner.DeltaTime * 2f);
            
        }
            
        
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if (Object == null || !Object.HasStateAuthority)
            return;
        TriggerExplosiveBomb(transform.position);
        StartCoroutine(DestroyObject(2f));
    }

    private void TriggerExplosiveBomb(Vector3 explosionPosition)
    {
        if (IsObjectCollided) return;

        IsObjectCollided = true;
        Rigidbody.isKinematic = true;

      
        _bombEffect.transform.position = explosionPosition + Vector3.up * 1.2f;
        RPC_SetEffectPosition(explosionPosition + Vector3.up * 1.2f);
        IsBombReadyToExplode = true;

       
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, 3f);
        HashSet<NetworkId> alreadyDamaged = new HashSet<NetworkId>();

        foreach (var hitCollider in hitColliders)
        {
            NetworkObject netObj = hitCollider.transform.GetComponentInParent<NetworkObject>();
            if (netObj == null || !netObj.IsValid)
                continue;

          
            NetworkId damageableId = netObj.Id;
            if (alreadyDamaged.Contains(damageableId))
                continue;

            alreadyDamaged.Add(damageableId);

           
            var damageable = hitCollider.transform.GetComponentInParent<IDamageable>();
            if (damageable == null)
                continue;

            
            if (hitCollider.transform.GetComponentInParent<CharacterHealth>() == null)
                continue;

           
            if (netObj.Id == OwnerProperties.PlayerID ||
                hitCollider.transform.GetComponentInParent<PlayerStatsController>()?.PlayerTeam !=
                OwnerProperties.PlayerTeam)
            {
                damageable.DealDamageRPC(
                    50f,
                    OwnerProperties.PlayerNickName.ToString(),
                    CharacterStats.CharacterType.Ranger
                );

                StartCoroutine(VerifyOpponentDeath(hitCollider.transform.gameObject));
            }
        }
    }
 
    private IEnumerator VerifyOpponentDeath(GameObject opponent)
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("OpponentHealth: " + opponent.GetComponentInParent<CharacterHealth>().NetworkedHealth);


        if (opponent.GetComponentInParent<CharacterHealth>().NetworkedHealth <= 0)
        {

            EventLibrary.OnPlayerGotKill.Invoke();
            EventLibrary.OnKillFeedReady.Invoke(OwnerProperties.PlayerWarrior, OwnerProperties.PlayerNickName.ToString(), opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            Debug.Log("KillerName: " + OwnerProperties.PlayerNickName.ToString() + " PlayerWarrior: " + OwnerProperties.PlayerWarrior + " OppnentName: " + opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            EventLibrary.OnPlayerKillRegistryUpdated.Invoke(OwnerProperties.PlayerTeam);

            if (!Object.HasStateAuthority)
            {
                EventLibrary.OnPlayerGotKill.Invoke();
            }

        }

        
    }
}
