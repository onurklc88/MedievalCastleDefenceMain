using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EarthShatterVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem _earthShatter;
    [SerializeField] private ParticleSystem[] _stoneParticles;


    private async void Start()
    {
        Debug.Log("Hav");
        _earthShatter.Play();

        for (int i = 0; i < _stoneParticles.Length; i++)
        {
            await UniTask.Delay(100);
            _stoneParticles[i].Play();
        }

        await UniTask.Delay(1000);
        
    }

}
