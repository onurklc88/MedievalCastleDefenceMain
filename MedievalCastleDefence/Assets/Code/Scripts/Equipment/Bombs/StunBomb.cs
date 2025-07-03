using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunBomb : Bomb, IThrowable
{
   
    public override void Spawned()
    {
        StartCoroutine(DestroyObject(13f));
    }


    public void InitOwnerStats(PlayerStatsController ownerStats, NetworkId ownerID)
    {
        if (ownerStats == null)
        {
            Debug.LogError("OwnerStats null verildi!");
            return;
        }
        _playerStatsController = ownerStats;
        OwnerID = ownerID;
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
        TriggerStunBomb(transform.position);
        /*
        if (other.gameObject.GetComponentInParent<IDamageable>() != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            StartCoroutine(DestroyObject(2f));
        }
        */
        StartCoroutine(DestroyObject(2f));
    }

    private void TriggerStunBomb(Vector3 explosionPosition)
    {
        if (IsObjectCollided) return;

        IsObjectCollided = true;
        Rigidbody.isKinematic = true;
        RPC_SetEffectPosition(explosionPosition + Vector3.up * 1.2f);
        IsBombReadyToExplode = true;


        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, 5f);
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
            var playerInput = hitCollider.transform.GetComponentInParent<CharacterCameraController>();
            var characterMovement = hitCollider.transform.GetComponentInParent<CharacterMovement>();
            if (damageable == null)
                continue;


            


            if (netObj.Id == OwnerID ||
                hitCollider.transform.GetComponentInParent<PlayerStatsController>()?.PlayerTeam !=
                _playerStatsController.PlayerNetworkStats.PlayerTeam)
            {
                playerInput.RPC_ReduceMouseSpeedTemporarily();
                characterMovement.RPC_SlowPlayerSpeed();
                /*
                damageable.DealDamageRPC(
                    50f,
                    _playerStatsController.PlayerLocalStats.PlayerNickName.ToString(),
                    CharacterStats.CharacterType.Ranger
                );
                */
            }
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
