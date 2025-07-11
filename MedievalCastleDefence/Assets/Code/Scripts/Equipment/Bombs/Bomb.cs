using System.Collections;
using UnityEngine;
using Fusion;


public class Bomb : NetworkBehaviour, IThrowable
{
    [SerializeField] protected ThrowableProperties BombProperties;
    public bool IsBombThrown { get; set; }
    [SerializeField] protected ParticleSystem _bombEffect;
    [Networked(OnChanged = nameof(OnBombStateChange))] public NetworkBool IsBombReadyToExplode { get; set; }

    [Networked]
    public NetworkBool IsObjectCollided { get; set; }
    public PlayerInfo OwnerProperties { get; set; }
    public Rigidbody Rigidbody;
    [SerializeField] protected WeaponStats _weapon;
    [SerializeField] private GameObject _interpolationTarget;
    [SerializeField] protected Collider _collison;

    private static void OnBombStateChange(Changed<Bomb> changed)
    {
        changed.Behaviour._bombEffect.Play();
        changed.Behaviour._interpolationTarget.SetActive(false);
        
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetEffectPosition(Vector3 pos)
    {
        _bombEffect.transform.position = pos;
    }

    public void SetOwner(PlayerInfo playerInfo)
    {
        OwnerProperties = playerInfo;
       // Debug.Log("Owner: " + OwnerProperties.PlayerNickName + "OwnerID: " + OwnerProperties.PlayerID + "OwnerTeam: " + OwnerProperties.PlayerTeam);
    }

    public IEnumerator DestroyObject(float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);

        if (Runner != null && Object != null && Object.IsValid)
            Runner.Despawn(Object);
    }
}
