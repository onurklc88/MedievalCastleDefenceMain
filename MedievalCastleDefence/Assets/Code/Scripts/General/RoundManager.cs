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

    public void Initialize(LevelManager levelManager, UIManager uiManager)
    {
        _levelManager = levelManager;
        _uiManager = uiManager;
    }

    public void StartNewRound()
    {
        RoundIndex += 1;
        ResetRoundStats();
        
    }

    public void EndRound()
    {
        Debug.Log("<color=red>Round Ended</color>");
        // Add additional end-round logic here.
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
            EndRound();
        }

        if (_blueTeamDeadCount == _levelManager.TeamsPlayerCount)
        {
            _redTeamScore += 1;
            _uiManager.UpdateTeamScoreRpc(TeamManager.Teams.Red, _redTeamScore);
            EndRound();
        }
    }

    private void ResetRoundStats()
    {
        _redTeamDeadCount = 0;
        _blueTeamDeadCount = 0;
    }

    private static void OnRoundCounterChange(Changed<RoundManager> changed)
    {
        changed.Behaviour._uiManager.UpdateRoundCounterText(changed.Behaviour.RoundIndex.ToString());
    }

    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
        throw new System.NotImplementedException();
    }
}
