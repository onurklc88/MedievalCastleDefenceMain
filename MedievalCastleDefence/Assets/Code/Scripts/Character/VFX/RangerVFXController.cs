using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerVFXController : PlayerVFXSytem
{
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);

    }
}
