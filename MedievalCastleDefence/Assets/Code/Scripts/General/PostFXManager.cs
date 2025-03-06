using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostFXManager : MonoBehaviour
{
    [SerializeField] private Volume _postProcessVolume;
    private MotionBlur _moBlur;



    private void OnEnable()
    {
        EventLibrary.OnPlayerDash.AddListener(ActivateMotionBlur);
    }

    private void OnDisable()
    {
        EventLibrary.OnPlayerDash.RemoveListener(ActivateMotionBlur);
    }

    private void Start()
    {
        if (_postProcessVolume.profile.TryGet(out _moBlur))
        {
            _moBlur.active = false;

        }
    }

    private void ActivateMotionBlur(bool enable)
    {
        _moBlur.active = enable;
    }
}
