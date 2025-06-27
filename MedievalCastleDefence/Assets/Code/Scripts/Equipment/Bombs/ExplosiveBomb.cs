using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class ExplosiveBomb : Bomb, IThrowable
{
    [SerializeField] private ParticleSystem _bombEffect;
    
    
    [Networked(OnChanged = nameof(OnArrowStateChange))] public NetworkBool IsBombReadyToExplode { get; set; }
    public override void Spawned()
    {
        StartCoroutine(DestroyObject(13f));
    
    }


    public void InitOwnerStats(PlayerStatsController ownerStats)
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
        /*
        if(!IsObjectCollided)
            _interpolationTarget.Rotate(Vector3.forward * 300f * Runner.DeltaTime);
        */
    }
    private static void OnArrowStateChange(Changed<ExplosiveBomb> changed)
    {
        changed.Behaviour._bombEffect.Play();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Object == null || !Object.HasStateAuthority)
            return;
        TriggerExplosiveArrow(transform.position);
        if (other.gameObject.GetComponentInParent<IDamageable>() != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            StartCoroutine(DestroyObject(2f));
        }


    }

    private void TriggerExplosiveArrow(Vector3 explosionPosition)
    {
        if (IsObjectCollided) return;
        IsObjectCollided = true;
        //DrawExplosionDebug(explosionPosition, 5f);
        Rigidbody.isKinematic = true;
        _bombEffect.transform.position = new Vector3(explosionPosition.x, explosionPosition.y + 1.2f, explosionPosition.z);
        IsBombReadyToExplode = true;
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, 5f);
        HashSet<NetworkId> alreadyDamaged = new HashSet<NetworkId>();
        /*
        for (int i = 0; i < hitColliders.Length; i++)
        {
            NetworkObject netObj = hitColliders[i].transform.GetComponentInParent<NetworkObject>();
            if (netObj == null || !netObj.IsValid)
                continue;
            NetworkId damageableId = netObj.Id;

            if (!alreadyDamaged.Contains(damageableId))
            {
               alreadyDamaged.Add(damageableId);
                //CheckAttackCollision(hitColliders[i].transform.gameObject);
                var collidedObject = hitColliders[i].transform.gameObject.GetComponentInParent<IDamageable>();
                if(collidedObject != null)
                    collidedObject.DealDamageRPC(50f, _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(), CharacterStats.CharacterType.Ranger);
            }
           
        }
        */
        foreach (var hitCollider in hitColliders)
        {
            NetworkObject netObj = hitCollider.transform.GetComponentInParent<NetworkObject>();
            if (netObj == null || !netObj.IsValid)
                continue;

            NetworkId damageableId = netObj.Id;

            if (alreadyDamaged.Contains(damageableId))
                continue;

            alreadyDamaged.Add(damageableId);
            /*
            var damageable = hitCollider.transform.GetComponentInParent<CharacterHealth>();
            if (damageable != null)
            {
                Debug.Log("A");
            }
            else
            {
                Debug.Log("B");
            }
           */
           // DamageToArea(hitCollider.transform.gameObject);
            
            
            var damageable = hitCollider.transform.GetComponentInParent<IDamageable>();
            if (damageable == null)
                continue;
            if (hitCollider.transform.gameObject.GetComponentInParent<CharacterHealth>() == null)
                continue;
            if(hitCollider.transform.GetComponentInParent<NetworkObject>().Id == )
           
            damageable.DealDamageRPC(
                50f,
                _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(),
                CharacterStats.CharacterType.Ranger
            );
            
            
        }
            
        
    }
    private void DamageToArea(GameObject opponent)
    {
        var opponentNetObj = opponent.GetComponentInParent<NetworkObject>();
        var selfNetObj = GetComponentInParent<NetworkObject>();


        if (opponentNetObj.Id == selfNetObj.Id)
        {
            Debug.Log("ID'Ler eþleþti");
            var opponentHealth = opponent.transform.GetComponentInParent<CharacterHealth>();
            if (opponentHealth == null)
            {
                Debug.Log("Opponent health  bulunmadý");
                return;
            }
            else
            {
                Debug.Log("Opponent health  bulundu");
            }

            opponentHealth.DealDamageRPC(
                _weaponStats.Damage,
                _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(),
                _playerStatsController.PlayerLocalStats.PlayerWarrior
            );
            return;
        }
        else
        {
            Debug.Log("Test");
        }


        var opponentStats = opponent.GetComponentInParent<PlayerStatsController>();
        if (opponentStats == null) return;

        var opponentTeam = opponentStats.PlayerTeam;
        if (opponentTeam == _playerStatsController.PlayerTeam)
            return;


        var targetHealth = opponent.GetComponentInParent<CharacterHealth>();
        Debug.Log("Name: " + opponent.gameObject.name);
        targetHealth.DealDamageRPC(
            _weaponStats.Damage,
            _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(),
            _playerStatsController.PlayerLocalStats.PlayerWarrior
        );
    }
    private IEnumerator DestroyObject(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);

        if (Runner != null && Object != null && Object.IsValid)
            Runner.Despawn(Object);

    }
  
}
