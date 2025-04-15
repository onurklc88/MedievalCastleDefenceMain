using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using CartoonFX;
using static BehaviourRegistry;
using Cysharp.Threading.Tasks;
public class PlayerVFXSytem : CharacterRegistry
{
    [SerializeField] protected ParticleSystem[] _bloodVFX;
    [SerializeField] protected ParticleSystem _parryVFX;
    [SerializeField] protected ParticleSystem _ultimateSkillVFX;
    [SerializeField] protected ParticleSystem _healingVFX;
    [SerializeField] private ParticleSystem _swordTrail;
    [SerializeField] private CFXR_Effect _cfxrEffect;
    [SerializeField] private ParticleSystem _stunnedEffect;
    [SerializeField] protected ParticleSystem _stunnedText;
   
    protected PlayerStatsController _playerStatsController;
 
   
    [Networked(OnChanged = nameof(OnSwordTrailStateChange))] public NetworkBool IsTrailActive { get; set; }
    [Networked(OnChanged = nameof(NetworkBloodVFXOnChange))] public NetworkBool IsBloodVFXReady { get; set; }
    [Networked(OnChanged = nameof(NetworkUltimateVFXOnChange))] public NetworkBool IsPlayerUseUltimate { get; set; }

    private void Start()
    {
        /*
         _playerStatsController = GetScript<PlayerStatsController>();
        

        if (_playerStatsController == null)
        {
            Debug.Log("Bulundu PlayerVFX");
        }
        else
        {
            Debug.Log("Bulunamadý PlayerVFX");
        }
        */
    }
    public void PlayBloodVFX()
    {
        IsBloodVFXReady = true;
        StartCoroutine(DisableBloodVFX());
    }

    public async void PlayUltimateVFX()
    {
        IsPlayerUseUltimate = true;
        await UniTask.Delay(300);
        IsPlayerUseUltimate = false;

    }

    private static void NetworkBloodVFXOnChange(Changed<PlayerVFXSytem> changed)
    {
        if (!changed.Behaviour.IsBloodVFXReady) return;
        changed.Behaviour._bloodVFX[0].Play();
        changed.Behaviour._bloodVFX[1].Play();

    }

    private static void NetworkUltimateVFXOnChange(Changed<PlayerVFXSytem> changed)
    {
        if (!changed.Behaviour.IsPlayerUseUltimate) return;
       changed.Behaviour._ultimateSkillVFX.Play();

    }

    private IEnumerator DisableBloodVFX()
    {
        yield return new WaitForSeconds(0.3f);
        IsBloodVFXReady = false;
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void UpdateParryVFXRpc()
    {

        await UniTask.Delay(300);
        _parryVFX.Play();
        if (_playerStatsController == null) return;
        if(_playerStatsController.PlayerNetworkStats.PlayerWarrior == CharacterStats.CharacterType.KnightCommander)
        {
            _cfxrEffect.PlayLightAnimation();
        } 
       
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void PlayHealingRpc()
    {
        await UniTask.Delay(300);
        _healingVFX.Play();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void PlayDisabledVFXRpc()
    {
        _stunnedEffect.Play();
        _stunnedText.Play();
        await UniTask.Delay(2000);
        _stunnedEffect.Stop();
        _stunnedText.Stop();


    }
}
