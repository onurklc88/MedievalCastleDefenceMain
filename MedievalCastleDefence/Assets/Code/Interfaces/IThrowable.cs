using System.Collections;
using UnityEngine;
using Fusion;

public interface IThrowable 
{
    [Networked]
    public NetworkBool IsObjectCollided { get; set; }
    public PlayerInfo OwnerProperties { get; set; }
    public void SetOwner(PlayerInfo playerInfo);


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetEffectPosition(Vector3 pos);
    public IEnumerator DestroyObject(float destroyTime);

}
