using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
using static BehaviourRegistry;

public class LevelManager : ManagerRegistry
{
    public int MaxPlayerCount;
    protected int MaxTeamPlayerCount;
    
    protected int CurrentLevelIndex;
    protected int TotalLevelRound;
    [Networked(OnChanged = nameof(OnPlayerCountChange))]
    public int CurrentPlayerCount { get; set; }
    private int _localPlayerCount;
    private int _redTeamPlayerCount;
    private int _blueTeamPlayerCount;
    
    
    public enum GamePhase
    {
        None,
        Warmup,
        RoundStart,
        RoundEnd,
        GameEnd
    }
    public GamePhase CurrentGamePhase;
    public GameManager.GameModes GameMode;

    public override void Spawned()
    {
        //InitScript(this);
        EventLibrary.OnPlayerSelectWarrior.AddListener(UpdatePlayerCount);
        
       CurrentGamePhase = GamePhase.Warmup;
       EventLibrary.OnGamePhaseChange.Invoke(CurrentGamePhase);
        Debug.Log("PlayerMax: " + MaxPlayerCount);
    }

    private void Awake()
    {
        SetGameMode();
        InitScript(this);
    }

    private void CheckCurrentPlayerCount()
    {

    }

    private void SetGameMode()
    {
        switch (GameMode)
        {
            case GameManager.GameModes.OneVsOne:
                MaxPlayerCount = 2;
                break;
            case GameManager.GameModes.TwoVsTwo:
                MaxPlayerCount = 4;
                break;
            case GameManager.GameModes.ThreeVsThree:
                MaxPlayerCount = 6;
                break;
          
        }
    }

    
    private void UpdatePlayerCount()
    {
         CurrentPlayerCount += 1;
        if(CurrentPlayerCount == MaxPlayerCount)
        {
            CurrentGamePhase = GamePhase.RoundStart;
        }
    }

  

    private static void OnPlayerCountChange(Changed<LevelManager> changed)
    {
        Debug.LogWarning($"<color=yellow>Player count updated: {changed.Behaviour.CurrentPlayerCount}</color>");
    }

 


}
