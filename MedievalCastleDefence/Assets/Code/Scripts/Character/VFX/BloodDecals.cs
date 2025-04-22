using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

public class BloodDecals : CharacterRegistry
{
    [SerializeField] private GameObject[] _bloodDecals;
    [SerializeField] private GameObject _swordDecal;
    [Networked(OnChanged = nameof(OnNetworkBloodDecalStateChange))] public int BloodDecalIndex { get; set; }
    private List<int> _usedIndices = new List<int>();
    private bool _condition;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        BloodDecalIndex = -1;

    }
  
    public void EnableRandomBloodDecal()
    {
        if (!Object.HasStateAuthority) return;

        int newIndex = _usedIndices.Count;

        if (newIndex >= _bloodDecals.Length)
        {
            _usedIndices.Clear();
            newIndex = 0;
        }

        _usedIndices.Add(newIndex);
     
        BloodDecalIndex = newIndex;
        Debug.Log("BloodDecalIndex: " + BloodDecalIndex);
    }

    public void DisableBloodDecals()
    {
        if (!Object.HasStateAuthority) return;

        BloodDecalIndex = -1; 
        _usedIndices.Clear();
    }

    public static void OnNetworkBloodDecalStateChange(Changed<BloodDecals> changed)
    {
        
        if (changed.Behaviour.BloodDecalIndex >= 0)
        {
            changed.Behaviour._bloodDecals[changed.Behaviour.BloodDecalIndex].gameObject.SetActive(true);
        }
        else
        {
            foreach (var decal in changed.Behaviour._bloodDecals)
            {
                decal.gameObject.SetActive(false);
            }
        }
    }


}
