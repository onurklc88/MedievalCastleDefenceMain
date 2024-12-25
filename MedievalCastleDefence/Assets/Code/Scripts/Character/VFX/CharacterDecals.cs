using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;   

public class CharacterDecals : BehaviourRegistry
{
    [SerializeField] private GameObject[] _bloodDecals;
    [SerializeField] private GameObject _swordDecal;
    [Networked(OnChanged = nameof(OnNetworkBloodDecalStateChange))] public int BloodDecalIndex { get; set; }
    private List<int> _usedIndices = new List<int>();
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

        Debug.Log("BloodDecalIndex: " + BloodDecalIndex);
    }

    public static void OnNetworkBloodDecalStateChange(Changed<CharacterDecals> changed)
    {
        changed.Behaviour._bloodDecals[changed.Behaviour.BloodDecalIndex].gameObject.SetActive(true);
    }


}
