using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class PlayerVFXSytem : BehaviourRegistry
{
    [SerializeField] private GameObject _weaponParticles;
    [SerializeField] private ParticleSystem[] _bloodVFX;
    [Networked(OnChanged = nameof(NetworkBloodVFXOnChange))] public NetworkBool IsBloodVFXReady { get; set; }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }


    public void EnableWeaponParticles()
    {
        if (!Object.HasStateAuthority) return;
        _weaponParticles.SetActive(true);
        StartCoroutine(DisableWeaponParticles());
    }

    public void PlayBloodVFX()
    {
        IsBloodVFXReady = true;
        StartCoroutine(DisableBloodVFX());
    }

    private static void NetworkBloodVFXOnChange(Changed<PlayerVFXSytem> changed)
    {
        if (!changed.Behaviour.IsBloodVFXReady) return;
         changed.Behaviour._bloodVFX[0].Play();
        changed.Behaviour._bloodVFX[1].Play();

    }

    private IEnumerator DisableBloodVFX()
    {
        yield return new WaitForSeconds(0.3f);
        IsBloodVFXReady = false;
    }

    private IEnumerator DisableWeaponParticles()
    {
        yield return new WaitForSeconds(0.9f);
        _weaponParticles.SetActive(false);
    }

   

}
