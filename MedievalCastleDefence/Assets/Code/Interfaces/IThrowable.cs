using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public interface IThrowable 
{
    [Networked]
    public NetworkBool IsObjectCollided { get; set; }
    public void InitOwnerStats(PlayerStatsController ownerInfo, NetworkId ownerID);


}
