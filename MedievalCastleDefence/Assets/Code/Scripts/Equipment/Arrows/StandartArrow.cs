using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StandartArrow : Arrow
{
    
  

    public override void Spawned()
    {
        StartCoroutine(DestroyArrow(13f));
    }


    public override void InitOwnerStats(PlayerStatsController ownerStats)
    {
        if (ownerStats == null)
        {
            Debug.LogError("OwnerStats null verildi!");
            return;
        }
        _playerStatsController = ownerStats;
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
        if(other.gameObject.GetComponentInParent<IDamageable>() != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            StartCoroutine(DestroyArrow(1f));
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

        if (collidedObject.gameObject.layer == 11) return;

        var damageable = collidedObject.transform.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        var opponentHealth = damageable as CharacterHealth;
        if (opponentHealth == null)
        {
            Debug.LogError("IDamageable is not CharacterHealth");
            return;
        }

        CurrentGamePhase = LevelManager.GamePhase.RoundStart;
        CheckAttackCollision(collidedObject.gameObject);
    }

    private IEnumerator DestroyArrow(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);

        if (Runner != null && Object != null && Object.IsValid)
            Runner.Despawn(Object);

    }
}
