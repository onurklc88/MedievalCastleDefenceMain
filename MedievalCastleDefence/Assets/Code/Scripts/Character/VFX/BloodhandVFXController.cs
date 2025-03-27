using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using CartoonFX;
using static BehaviourRegistry;
using Cysharp.Threading.Tasks;
public class BloodhandVFXController : PlayerVFXSytem
{
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }
}
