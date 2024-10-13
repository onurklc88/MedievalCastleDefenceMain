using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
namespace test
{
    public class CharacterRegistry : NetworkBehaviour
    {
        protected static List<CharacterRegistry> _scriptList = new List<CharacterRegistry>();
        [SerializeField] protected CharacterStats _characterStats;
        protected void InitScript(CharacterRegistry type)
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
        protected T GetScript<T>() where T : CharacterRegistry
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

       
    }
}

