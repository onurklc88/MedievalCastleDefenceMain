using System;
using UnityEngine;
using Fusion;
public static class EventLibrary
{
    //public static readonly GameEvent<PlayerRef> OnRespawnRequested = new GameEvent<PlayerRef>();
    public static readonly GameEvent<PlayerRef, CharacterStats.CharacterType> OnRespawnRequested = new GameEvent<PlayerRef, CharacterStats.CharacterType>();
    public static readonly GameEvent<PlayerStats> OnPlayerStatsUpdated = new GameEvent<PlayerStats>();


}
