using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class ImpulseManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource[] _impulse;

    private void OnEnable()
    {
        EventLibrary.OnImpulseRequested.AddListener(PlayImpulseAtIndex);
    }

    private void OnDisable()
    {
        EventLibrary.OnImpulseRequested.RemoveListener(PlayImpulseAtIndex);
    }
    private void PlayImpulseAtIndex(int index, float force)
    {
        _impulse[index].GenerateImpulseWithForce(force);
    }
}
