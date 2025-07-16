using System;
using UnityEngine;
using Fusion;
public static class EventLibrary
{
    //ServerEvents
    public static readonly GameEvent<PlayerRef> OnPlayerJoiningServer = new GameEvent<PlayerRef>();
   
    public static readonly GameEvent<TeamManager.Teams, bool> test = new GameEvent<TeamManager.Teams, bool>();
    
    
    
    
    
    //Level Evets

    public static readonly GameEvent<PlayerRef, CharacterStats.CharacterType> OnRespawnRequested = new GameEvent<PlayerRef, CharacterStats.CharacterType>();
    public static readonly GameEvent OnPlayerSelectWarrior = new GameEvent();
    public static readonly GameEvent<LevelManager.GamePhase> OnGamePhaseChange = new GameEvent<LevelManager.GamePhase>();
    public static readonly GameEvent<PlayerRef, CharacterStats.CharacterType, TeamManager.Teams> OnPlayerSelectTeam = new GameEvent<PlayerRef, CharacterStats.CharacterType, TeamManager.Teams>();
    


    //UI
    public static readonly GameEvent<TeamManager.Teams> OnLevelFinish = new GameEvent<TeamManager.Teams>();
    public static readonly GameEvent<TeamManager.Teams> OnPlayerRespawn = new GameEvent<TeamManager.Teams>();
    public static readonly GameEvent OnPlayerTeamSwitchRequested = new GameEvent();


    //PostFX
    public static readonly GameEvent<bool> OnPlayerDash = new GameEvent<bool>();
    public static readonly GameEvent OnPlayerTakeDamage = new GameEvent();
  

    //Stats Events
    public static readonly GameEvent<PlayerInfo> OnPlayerStatsUpdated = new GameEvent<PlayerInfo>();
    public static readonly GameEvent<CharacterStats.CharacterType, string, string> OnKillFeedReady = new GameEvent<CharacterStats.CharacterType, string, string>();
    public static readonly GameEvent<TeamManager.Teams> OnPlayerKillRegistryUpdated = new GameEvent<TeamManager.Teams>();

    //Camera
    public static readonly GameEvent<int, float> OnImpulseRequested = new GameEvent<int, float>();

    //Debug
    public static readonly GameEvent<String> DebugMessage = new GameEvent<String>();
}
