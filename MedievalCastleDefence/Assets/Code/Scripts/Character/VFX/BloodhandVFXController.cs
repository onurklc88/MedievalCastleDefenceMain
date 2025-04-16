using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using CartoonFX;
using  Tiny;
using static BehaviourRegistry;
using Cysharp.Threading.Tasks;
public class BloodhandVFXController : PlayerVFXSytem
{
    [Networked(OnChanged = nameof(OnNetworkTrailStateChange))] public NetworkBool IsPlayerSwing { get; set; }
    [SerializeField] private ParticleSystem _earthShatterVFX;
    [SerializeField] private Trail _axeTrail;
    [SerializeField] private ParticleSystem[] _stoneParticles;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void PlayEarthShatterVFXRpc()
    {
        _earthShatterVFX.Play();
        
        for(int i = 0; i < _stoneParticles.Length; i++)
        {
            await UniTask.Delay(150);
            _stoneParticles[i].Play();
        }
        

       //&& await UniTask.Delay(200);
        _earthShatterVFX.Stop();



    }

    private static void OnNetworkTrailStateChange(Changed<BloodhandVFXController> changed)
    {
        changed.Behaviour._axeTrail.enabled = changed.Behaviour.IsPlayerSwing;
    }

}
