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
    
    
    
    public enum GamePhase
    {
        None,
        Warmup,
        Prepertaion,
        RoundStart,
        RoundEnd,
        GameEnd
    }
    public GamePhase CurrentGamePhase;
    public GameManager.GameModes GameMode;

    public override void Spawned()
    {
        //InitScript(this);
        EventLibrary.OnPlayerSelectWarrior.AddListener(UpdatePlayerCountRpc);
        
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void UpdatePlayerCountRpc()
    {
        if (!Runner.IsSharedModeMasterClient) return;
        CurrentPlayerCount += 1;
        //Debug.Log("CurrentPlayerCount: " + CurrentPlayerCount);
        if(CurrentPlayerCount == MaxPlayerCount)
        {
            Debug.LogWarning("MaxPlayerCount reached.");
            CurrentGamePhase = GamePhase.RoundStart;
            EventLibrary.OnGamePhaseChange.Invoke(CurrentGamePhase);
        }
    }

  

    private static void OnPlayerCountChange(Changed<LevelManager> changed)
    {
        if (!changed.Behaviour.Object.HasStateAuthority) return;
        Debug.LogWarning($"<color=yellow>Player count updated: {changed.Behaviour.CurrentPlayerCount}</color>");
        if(changed.Behaviour.CurrentPlayerCount == changed.Behaviour.MaxPlayerCount)
        {
            changed.Behaviour.CurrentGamePhase = GamePhase.RoundStart;
            
        }
    }

 


}
