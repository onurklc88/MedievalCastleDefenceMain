using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public abstract class BehaviourRegistry : NetworkBehaviour
{
    protected abstract List<BehaviourRegistry> ScriptList { get; }

    protected void InitScript(BehaviourRegistry type)
    {

        foreach (var script in ScriptList)
        {
            if (script.GetType() == type.GetType())
            {
                Debug.LogWarning($"{type.GetType().Name} is already registered.");
                return;
            }
        }

        ScriptList.Add(type);
        Debug.Log($"{this.GetType().Name} registered with type: {type.GetType().Name}.");
    }
    protected T GetScript<T>() where T : BehaviourRegistry
    {
        foreach (var script in ScriptList)
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
        ScriptList.Clear();
    }

    public class CharacterRegistry : BehaviourRegistry
    {
        private static List<BehaviourRegistry> _characterScripts = new List<BehaviourRegistry>();
        protected override List<BehaviourRegistry> ScriptList => _characterScripts;
        [SerializeField] protected CharacterStats _characterStats;
    }

    public class ManagerRegistry : BehaviourRegistry
    {
        private static List<BehaviourRegistry> _managerScripts = new List<BehaviourRegistry>();
        protected override List<BehaviourRegistry> ScriptList => _managerScripts;
    }
}




