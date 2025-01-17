using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

public class RoundManager : ManagerRegistry, IGameStateListener
{
    [Networked(OnChanged = nameof(OnRoundCounterChange))] public int RoundIndex { get; set; }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }

    private UIManager _uiManager;
    private LevelManager _levelManager;
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
         EventLibrary.OnPlayerKillRegistryUpdated.AddListener(CheckRoundEndByDefeatRpc);
    }

    private void Start()
    {
        _uiManager = GetScript<UIManager>();
        _levelManager = GetScript<LevelManager>();
    }
    public void StartNewRound()
    {
        RoundIndex += 1;
    }

    public void EndRound()
    {
        if(RoundIndex < _levelManager.TotalLevelRound)
        {
            _redTeamDeadCount = 0;
            _blueTeamDeadCount = 0;
        }
        else
        {
            Debug.Log("EndMatch");
        }
       
    }
  
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void CheckRoundEndByDefeatRpc(TeamManager.Teams playerTeam)
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

        if (_redTeamDeadCount == _levelManager.TeamsPlayerCount)
        {
            _blueTeamScore += 1;
            _uiManager.UpdateTeamScoreRpc(TeamManager.Teams.Blue, _blueTeamScore);
           // ResetRoundStats();
        }

        if (_blueTeamDeadCount == _levelManager.TeamsPlayerCount)
        {
            _redTeamScore += 1;
            _uiManager.UpdateTeamScoreRpc(TeamManager.Teams.Red, _redTeamScore);
           // ResetRoundStats();
        }
    }

   

    private static void OnRoundCounterChange(Changed<RoundManager> changed)
    {
        changed.Behaviour._uiManager.UpdateRoundCounterText(changed.Behaviour.RoundIndex.ToString());
    }

    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
        switch (currentGameState)
        {
            case LevelManager.GamePhase.GameStart:
                break;
            case LevelManager.GamePhase.Warmup:

                break;
            case LevelManager.GamePhase.Preparation:
                RoundIndex = 0;
                break;
            case LevelManager.GamePhase.RoundStart:
                StartNewRound();
                break;
            case LevelManager.GamePhase.RoundEnd:
                EndRound();
                break;
            case LevelManager.GamePhase.GameEnd:

                break;
        }
    }
}
