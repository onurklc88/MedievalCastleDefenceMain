using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using CartoonFX;
using Cysharp.Threading.Tasks;

public class IronheartVFXController : PlayerVFXSytem
{
    [SerializeField] private GameObject[] _swordLocalTrails;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        _swordLocalTrails[0].SetActive(true);
        _swordLocalTrails[1].SetActive(true);
    }



}
