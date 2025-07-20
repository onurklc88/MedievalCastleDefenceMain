using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class ExplosiveBomb : Bomb
{
    public override void Spawned()
    {
        Invoke("EnableCollision", 0.2f);
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


        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, BombProperties.AEOWidth);
        DrawSphere(explosionPosition, 3f, Color.red, 2f);
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
                    BombProperties.Damage,
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
            EventLibrary.OnKillFeedReady.Invoke(OwnerProperties.PlayerWarrior, OwnerProperties.PlayerNickName.ToString(), opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            Debug.Log("KillerName: " + OwnerProperties.PlayerNickName.ToString() + " PlayerWarrior: " + OwnerProperties.PlayerWarrior + " OppnentName: " + opponent.transform.GetComponentInParent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString());
            EventLibrary.OnPlayerKillRegistryUpdated.Invoke(OwnerProperties.PlayerTeam);
            NetworkObject obj = Runner.FindObject(OwnerProperties.PlayerID);
            if (opponent.transform.GetComponentInParent<NetworkObject>() != OwnerProperties.PlayerID)
            {
                obj.gameObject.GetComponentInParent<PlayerStatsController>().UpdatePlayerKillCountRpc();
            }
        }
    }

    public static void DrawSphere(Vector3 center, float radius, Color color, float duration = 0.1f)
    {
        // Unity'de built-in Debug.DrawSphere yok, o yüzden kendimiz çiziyoruz
        Vector3 prevPos = center + new Vector3(radius, 0, 0);
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)(i + 1) / 30f * Mathf.PI * 2f;
            Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPos, newPos, color, duration);
            prevPos = newPos;
        }

        prevPos = center + new Vector3(0, radius, 0);
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)(i + 1) / 30f * Mathf.PI * 2f;
            Vector3 newPos = center + new Vector3(0, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPos, newPos, color, duration);
            prevPos = newPos;
        }

        prevPos = center + new Vector3(radius, 0, 0);
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)(i + 1) / 30f * Mathf.PI * 2f;
            Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Debug.DrawLine(prevPos, newPos, color, duration);
            prevPos = newPos;
        }
    }
}
