using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
public class PlayerVFXSytem : CharacterRegistry
{
    [SerializeField] private GameObject _weaponParticles;
    [SerializeField] private ParticleSystem[] _bloodVFX;
    [SerializeField] private GameObject[] _swordLocalTrails;
    [SerializeField] private ParticleSystem _swordTrail;
    [Networked(OnChanged = nameof(OnSwordTrailStateChange))] public NetworkBool IsTrailActive { get; set; }
    [Networked(OnChanged = nameof(NetworkBloodVFXOnChange))] public NetworkBool IsBloodVFXReady { get; set; }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        _swordLocalTrails[0].SetActive(true);
        _swordLocalTrails[1].SetActive(true);
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


    public void ActivateSwordTrail(bool enable)
    {
        if (!Object.HasStateAuthority) return;
       IsTrailActive = enable;
    }

    private static void OnSwordTrailStateChange(Changed<PlayerVFXSytem> changed)
    {

        if (changed.Behaviour.IsTrailActive)
        {
            changed.Behaviour._swordTrail.Play();
        }
        else
        {
            changed.Behaviour._swordTrail.Stop();
        }
      
    }

}
