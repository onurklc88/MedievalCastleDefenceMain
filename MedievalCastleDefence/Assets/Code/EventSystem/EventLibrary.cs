using System;
using UnityEngine;
using Fusion;
public static class EventLibrary
{
    //ServerEvents
    public static readonly GameEvent<PlayerRef> OnPlayerJoiningServer = new GameEvent<PlayerRef>();
  

    
    
    
    
    
    //Level Evets

    public static readonly GameEvent<PlayerRef, CharacterStats.CharacterType> OnRespawnRequested = new GameEvent<PlayerRef, CharacterStats.CharacterType>();
    public static readonly GameEvent<int, int> OnRoundCompleted = new GameEvent<int, int>();
    public static readonly GameEvent OnPlayerSelectWarrior = new GameEvent();
    public static readonly GameEvent<LevelManager.GamePhase> OnGamePhaseChange = new GameEvent<LevelManager.GamePhase>();
    public static readonly GameEvent<PlayerRef, CharacterStats.CharacterType, TeamManager.Teams> OnPlayerSelectTeam = new GameEvent<PlayerRef, CharacterStats.CharacterType, TeamManager.Teams>();
    
    
    
    
    
    
    
    
    
    
    //Stats Events
    public static readonly GameEvent<PlayerInfo> OnPlayerStatsUpdated = new GameEvent<PlayerInfo>();
    public static readonly GameEvent<CharacterStats.CharacterType, string, string> OnPlayerKill = new GameEvent<CharacterStats.CharacterType, string, string>();
    public static readonly GameEvent<TeamManager.Teams> OnPlayerKillRegistryUpdated = new GameEvent<TeamManager.Teams>();

    //Debug
    public static readonly GameEvent<String> DebugMessage = new GameEvent<String>();
}
