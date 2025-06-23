using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public interface IThrowable 
{
    public bool IsObjectCollided { get; set; }
    public void InitOwnerStats(PlayerStatsController ownerInfo);


}
