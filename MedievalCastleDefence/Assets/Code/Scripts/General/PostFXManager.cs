using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PostFXManager : MonoBehaviour
{
    [SerializeField] private Volume _generalPostProcessVolume;
    //[SerializeField] private Volume _onScreenDamagePostProcessVolume;
    private MotionBlur _moBlur;



    private void OnEnable()
    {
        EventLibrary.OnPlayerDash.AddListener(ActivateMotionBlur);
        EventLibrary.OnPlayerTakeDamage.AddListener(EnableOnScreenDamageFX);
    }

    private void OnDisable()
    {
        EventLibrary.OnPlayerDash.RemoveListener(ActivateMotionBlur);
        EventLibrary.OnPlayerTakeDamage.RemoveListener(EnableOnScreenDamageFX);
    }

    private void Start()
    {
        if (_generalPostProcessVolume.profile.TryGet(out _moBlur))
        {
            _moBlur.active = false;

        }
    }

    private void ActivateMotionBlur(bool enable)
    {
        _moBlur.active = enable;
    }

    [SerializeField] private Volume _onScreenDamagePostProcessVolume;
    private CancellationTokenSource _cts;

    private async void EnableOnScreenDamageFX()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _onScreenDamagePostProcessVolume.weight = 1;
        await UniTask.Delay(1000, cancellationToken: _cts.Token);

        await FadeOutWeight(0.5f, _cts.Token);
    }

    private async UniTask FadeOutWeight(float duration, CancellationToken token)
    {
        float startTime = Time.time;
        float startWeight = _onScreenDamagePostProcessVolume.weight;

        while (Time.time - startTime < duration)
        {
            if (token.IsCancellationRequested) return;
            float t = (Time.time - startTime) / duration;
            _onScreenDamagePostProcessVolume.weight = Mathf.Lerp(startWeight, 0, t);
            await UniTask.Yield();
        }

        _onScreenDamagePostProcessVolume.weight = 0;
    }
}

