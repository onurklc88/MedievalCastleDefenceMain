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
  
    [SerializeField] private ParticleSystem _spritualVFX;
    [SerializeField] private GameObject _earthShatterTest;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

    public void ActivateAxeTrail(bool enable)
    {
        if (!Object.HasStateAuthority) return;
        IsPlayerSwing = enable;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void PlayEarthShatterVFXRpc()
    {
        var earthVFX = Runner.Spawn(_earthShatterTest, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), transform.rotation, Runner.LocalPlayer);
        await UniTask.Delay(4000);
        Runner.Despawn(earthVFX);

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void PlaySpritualVFXRpc()
    {
        _spritualVFX.Play();

    }

    private static void OnNetworkTrailStateChange(Changed<BloodhandVFXController> changed)
    {
        changed.Behaviour._axeTrail.enabled = changed.Behaviour.IsPlayerSwing;
    }

}
