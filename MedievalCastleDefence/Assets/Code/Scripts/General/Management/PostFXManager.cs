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
    [SerializeField] private DoubleVisionFeature _doubleVisionFeature;
    [SerializeField] private float _doubleVisionDuration = 3f;
    [SerializeField] private float _doubleVisionMaxOffset = 0.05f;
    [SerializeField] private float _doubleVisionMaxIntensity = 0.8f;
    private bool _isEffectRunning;

    private void OnEnable()
    {
        EventLibrary.OnPlayerDash.AddListener(ActivateMotionBlur);
        EventLibrary.OnPlayerTakeDamage.AddListener(EnableOnScreenDamageFX);
        EventLibrary.OnplayerStunned.AddListener(() => ActivateEffectAsync().Forget());
    }

    private void OnDisable()
    {
        EventLibrary.OnPlayerDash.RemoveListener(ActivateMotionBlur);
        EventLibrary.OnPlayerTakeDamage.RemoveListener(EnableOnScreenDamageFX);
        EventLibrary.OnplayerStunned.RemoveListener(() => ActivateEffectAsync().Forget());
    }

    private void Start()
    {
        if (_generalPostProcessVolume.profile.TryGet(out _moBlur))
        {
            _moBlur.active = false;

        }
    }
    private async void Update()
    {
     
        if (Input.GetKeyDown(KeyCode.P))
        {
            await ActivateEffectAsync();
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

   
    public async UniTask ActivateEffectAsync()
    {
        if (_doubleVisionFeature == null || _isEffectRunning)
            return;

        _isEffectRunning = true;

       
        _doubleVisionFeature.settings.intensity = _doubleVisionMaxIntensity;
        _doubleVisionFeature.settings.offset = _doubleVisionMaxOffset;

        float timer = 0f;

        while (timer < _doubleVisionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / _doubleVisionDuration);

          
            _doubleVisionFeature.settings.intensity = Mathf.Lerp(_doubleVisionMaxIntensity, 0, progress);
            _doubleVisionFeature.settings.offset = Mathf.Lerp(_doubleVisionMaxOffset, 0, progress);

            await UniTask.Yield(); 
        }

        _doubleVisionFeature.settings.intensity = 0;
        _doubleVisionFeature.settings.offset = 0;
        _isEffectRunning = false;
    }


}

