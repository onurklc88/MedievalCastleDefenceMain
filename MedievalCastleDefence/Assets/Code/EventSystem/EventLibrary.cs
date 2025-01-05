using System;
using UnityEngine;
using Fusion;
public static class EventLibrary
{
    //public static readonly GameEvent<PlayerRef> OnRespawnRequested = new GameEvent<PlayerRef>();
    public static readonly GameEvent<PlayerRef, CharacterStats.CharacterType> OnRespawnRequested = new GameEvent<PlayerRef, CharacterStats.CharacterType>();
    public static readonly GameEvent<PlayerInfo> OnPlayerStatsUpdated = new GameEvent<PlayerInfo>();
    public static readonly GameEvent<String> DebugMessage = new GameEvent<String>();
    public static readonly GameEvent<CharacterStats.CharacterType, string, string> OnPlayerKill = new GameEvent<CharacterStats.CharacterType, string, string>();


}
