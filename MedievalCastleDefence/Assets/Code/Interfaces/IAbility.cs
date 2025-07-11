using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public interface IAbility
{
    [Networked] public NetworkBool IsAbilityInUse { get; set; }
    public NetworkBool IsAbilityInUseLocal { get; set; }
}
