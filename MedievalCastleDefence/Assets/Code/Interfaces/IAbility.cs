using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public interface IAbility
{
    [Networked] public NetworkBool IsPlayerUseAbility { get; set; }
    public NetworkBool IsPlayerUseAbilityLocal { get; set; }
}
