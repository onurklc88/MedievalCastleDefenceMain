using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BehaviourRegistry : NetworkBehaviour
{
    private static List<BehaviourRegistry> _scriptList = new List<BehaviourRegistry>();
    [SerializeField] protected CharacterStats _characterStats;
   
    protected void InitScript(BehaviourRegistry type)
    {

        foreach (var script in _scriptList)
        {
            if (script.GetType() == type.GetType())
            {
                Debug.LogWarning($"{type.GetType().Name} is already registered.");
                return;
            }
        }

        _scriptList.Add(type);
        Debug.Log($"{this.GetType().Name} registered with type: {type.GetType().Name}.");
    }
    protected T GetScript<T>() where T : BehaviourRegistry
    {
        foreach (var script in _scriptList)
        {
            if (script is T)
            {
                return script as T;
            }
        }

        Debug.LogWarning($"No script of type {typeof(T).Name} found.");
        return null;
    }

    protected void OnObjectDestroy()
    {
        _scriptList.Clear();
    }
}




