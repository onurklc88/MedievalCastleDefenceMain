using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

public class CharacterDecals : CharacterRegistry
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
    }

    public void EnableRandomBloodDecal()
    {

        if (_usedIndices.Count >= _bloodDecals.Length)
        {
            Debug.LogWarning("All indices have been used. Resetting the index pool.");
            _usedIndices.Clear();
        }
      
        int newIndex;
        do
        {
            newIndex = UnityEngine.Random.Range(0, _bloodDecals.Length);
        } while (_usedIndices.Contains(newIndex));

        _usedIndices.Add(newIndex);
        BloodDecalIndex = newIndex;
    }

    public void DisableBloodDecals()
    {
        if (!Object.HasStateAuthority) return;

        BloodDecalIndex = -1; 
        _usedIndices.Clear();
    }

    public static void OnNetworkBloodDecalStateChange(Changed<CharacterDecals> changed)
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
