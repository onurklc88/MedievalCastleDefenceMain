using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
using static BehaviourRegistry;

public class LevelManager : ManagerRegistry, IGameStateListener
{
   
  
    [Networked(OnChanged = nameof(OnPlayerCountChange))]  public int CurrentPlayerCount { get; set; }
   

    [Networked(OnChanged = nameof(OnRoundCounterChange))] public int RoundIndex { get; set; }
    public GamePhase CurrentGamePhase { get; set; }

    public enum GamePhase
    {
        None,
        GameStart,
        Warmup,
        Preparation,
        RoundStart,
        RoundEnd,
        GameEnd
    }
    public int MaxPlayerCount;
    public GameManager.GameModes GameMode;
    protected int CurrentLevelIndex;
    protected int TotalLevelRound;
    private int _localPlayerCount;
    private UIManager _uiManager;
    public int TeamsPlayerCount;
    private int _redTeamDeadCount;
    private int _blueTeamDeadCount;
    private int _redTeamScore;
    private int _blueTeamScore;

    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
       // EventLibrary.OnPlayerKillRegistryUpdated.AddListener(CheckRoundEndByDefeatRpc);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameState);
        EventLibrary.OnPlayerKillRegistryUpdated.RemoveListener(CheckRoundEndByDefeatRpc);
    }
    public override void Spawned()
    {
        //InitScript(this);
        EventLibrary.OnPlayerSelectWarrior.AddListener(UpdatePlayerCountRpc);
        EventLibrary.OnPlayerKillRegistryUpdated.AddListener(CheckRoundEndByDefeatRpc);
        CurrentGamePhase = GamePhase.Warmup;
       EventLibrary.OnGamePhaseChange.Invoke(CurrentGamePhase);
    }

    private void Awake()
    {
        SetGameMode();
        InitScript(this);
    }

    private void Start()
    {
        _uiManager = GetScript<UIManager>();
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

        TeamsPlayerCount = MaxPlayerCount / 2;
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
            CurrentGamePhase = GamePhase.Preparation;
            EventLibrary.OnGamePhaseChange.Invoke(CurrentGamePhase);
        }
    }

    public void UpdateGameState(GamePhase currentGameState)
    {
       
        switch (currentGameState)
        {
            case GamePhase.GameStart:
                break;
            case GamePhase.Warmup:
               
                break;
            case GamePhase.Preparation:
                RoundIndex = 0;
                break;
            case GamePhase.RoundStart:
                
                RoundIndex += 1;
                break;
            case GamePhase.RoundEnd:
                
                break;
            case GamePhase.GameEnd:

                break;
        } 
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void CheckRoundEndByDefeatRpc(TeamManager.Teams playerTeam)
    {
        if (!Runner.IsSharedModeMasterClient) return;
        if (playerTeam == TeamManager.Teams.Red)
        {
            _blueTeamDeadCount += 1;
          
        }
        else
        {
            _redTeamDeadCount += 1;
        }


        
       
        if (_redTeamDeadCount == TeamsPlayerCount)
        {
            _blueTeamScore += 1;
            _uiManager.UpdateTeamScoreRpc(TeamManager.Teams.Blue, _blueTeamScore);
        }
        if (_blueTeamDeadCount == TeamsPlayerCount)
        {
            _redTeamScore += 1;
            _uiManager.UpdateTeamScoreRpc(TeamManager.Teams.Red, _redTeamScore);
        }
        _blueTeamScore = 0;
        _redTeamScore = 0;
    }


    private static void OnPlayerCountChange(Changed<LevelManager> changed)
    {
       Debug.LogWarning($"<color=yellow>Player count updated: {changed.Behaviour.CurrentPlayerCount}</color>");
     
    }

    private static void OnRoundCounterChange(Changed<LevelManager> changed)
    {
        changed.Behaviour._uiManager.UpdateRoundCounterText(changed.Behaviour.RoundIndex.ToString());
    }

  
}
