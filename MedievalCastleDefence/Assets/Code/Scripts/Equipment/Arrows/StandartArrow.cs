using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StandartArrow : Arrow
{
   
    public override void Spawned()
    {
        StartCoroutine(DestroyObject(13f));
    }
   

    public override void FixedUpdateNetwork()
    {
        if (!IsArrowReleased) return;
        UpdateArrowRotation();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Object == null || !Object.HasStateAuthority)
            return;
        ArrowCollision(other);
        StartCoroutine(DestroyObject(1f));
        if (other.gameObject.GetComponentInParent<IDamageable>() != null)
        {
            //Runner.Despawn(Object);
        }
        else
        {
            
        }
       
        
    }

    private void ArrowCollision(Collider collidedObject)
    {
       
        if (IsObjectCollided)
        {
            return;
        }
        IsObjectCollided = true;
        _rigidbody.isKinematic = true;

        if (!Runner.IsClient) return;

        //if (collidedObject.gameObject.layer == 11) return;

        var damageable = collidedObject.transform.GetComponentInParent<IDamageable>();
        if (damageable == null) return;
        
        if (collidedObject.transform.GetComponentInParent<NetworkObject>() == null) return;
        if (collidedObject.transform.GetComponentInParent<NetworkObject>().Id == OwnerProperties.PlayerID) return;
        var opponentTeam = collidedObject.transform.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;
        Debug.Log("OpponentTeam: " + collidedObject.transform.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam);
       
        if (opponentTeam == OwnerProperties.PlayerTeam || CurrentGamePhase == LevelManager.GamePhase.Preparation) return;
        var opponentHealth = collidedObject.transform.GetComponentInParent<CharacterHealth>();
        var opponentStamina = collidedObject.transform.GetComponentInParent<CharacterStamina>();
        var isOpponentBlocking = collidedObject.transform.GetComponentInParent<CharacterAttackBehaviour>().IsPlayerBlocking;
      
        if ((collidedObject.gameObject.layer == 11 || collidedObject.gameObject.layer == 10) && isOpponentBlocking)
        {
            collidedObject.transform.GetComponentInParent<PlayerVFXSytem>().UpdateParryVFXRpc();
            opponentStamina.DecreaseDefenceStaminaRPC(10);
        }
        else
        {
            opponentHealth.DealDamageRPC(ArrowProperties.Damage, OwnerProperties.PlayerNickName.ToString(), OwnerProperties.PlayerWarrior);
            StartCoroutine(VerifyOpponentDeath(collidedObject.gameObject));
            
        }
       // CheckAttackCollision(collidedObject.gameObject);
    }

    private IEnumerator VerifyOpponentDeath(GameObject opponent)
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("OpponentHealth: " + opponent.GetComponentInParent<CharacterHealth>().NetworkedHealth);


        if (opponent.GetComponentInParent<CharacterHealth>().NetworkedHealth <= 0)
        {
           
            EventLibrary.OnKillFeedReady.Invoke(OwnerProperties.PlayerWarrior, OwnerProperties.PlayerNickName.ToString(), opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            Debug.Log("KillerName: " + OwnerProperties.PlayerNickName.ToString() + " PlayerWarrior: " + OwnerProperties.PlayerWarrior + " OppnentName: " + opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            EventLibrary.OnPlayerKillRegistryUpdated.Invoke(OwnerProperties.PlayerTeam);
        }


    }


}
