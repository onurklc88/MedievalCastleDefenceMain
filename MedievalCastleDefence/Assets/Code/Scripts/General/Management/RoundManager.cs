using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;

public class RoundManager : ManagerRegistry, IGameStateListener
{
    [Networked(OnChanged = nameof(OnRoundCounterChange))] public int RoundIndex { get; set; }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }
    private TeamManager.Teams _roundWinnerTeam;
    private UIManager _uiManager;
    private LevelManager _levelManager;
    private int _redTeamDeadCount;
    private int _blueTeamDeadCount;
    private int _redTeamScore;
    private int _blueTeamScore;
    

    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
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
        if(_roundWinnerTeam != TeamManager.Teams.None)
        {
            _uiManager.UpdateTeamScoreRpc(_roundWinnerTeam, _roundWinnerTeam == TeamManager.Teams.Red ? _redTeamScore : _blueTeamScore);
        }
    }

    public void EndRound()
    {
       
        if(RoundIndex < _levelManager.TotalLevelRound)
        {
            _redTeamDeadCount = 0;
            _blueTeamDeadCount = 0;
             _levelManager.ChangeGamePhase(LevelManager.GamePhase.Preparation);
        }
        else
        {
            _uiManager.UpdateTeamScoreRpc(_roundWinnerTeam, _roundWinnerTeam == TeamManager.Teams.Red ? _redTeamScore : _blueTeamScore);
            _levelManager.ChangeGamePhase(LevelManager.GamePhase.GameEnd);
        }
       
    }
  
    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void CheckRoundEndByDefeatRpc(TeamManager.Teams playerTeam)
    {
        if (!Runner.IsSharedModeMasterClient || CurrentGamePhase == LevelManager.GamePhase.Preparation || CurrentGamePhase == LevelManager.GamePhase.Warmup) return;
        
        if (playerTeam == TeamManager.Teams.Red)
        {
            _blueTeamDeadCount += 1;
        }
        else
        {
            _redTeamDeadCount += 1;
        }
       

        if (_redTeamDeadCount == _levelManager.RedTeamPlayerCount)
        {
            _blueTeamScore += 1;
            _roundWinnerTeam = TeamManager.Teams.Blue;
            EventLibrary.OnLevelFinish.Invoke(_roundWinnerTeam);
            await UniTask.Delay(3000);
          
            _levelManager.ChangeGamePhase(LevelManager.GamePhase.RoundEnd);
        }
        else if (_blueTeamDeadCount == _levelManager.BlueTeamPlayerCount)
        {
            _redTeamScore += 1;
            _roundWinnerTeam = TeamManager.Teams.Red;
            EventLibrary.OnLevelFinish.Invoke(_roundWinnerTeam);
            await UniTask.Delay(3000);
            _levelManager.ChangeGamePhase(LevelManager.GamePhase.RoundEnd);
        }

        //Debug.Log("RedPTeamdeadCount: " + _redTeamDeadCount + " BlueTeamDeadCoutn: " + _blueTeamDeadCount);
    }

   

    private static void OnRoundCounterChange(Changed<RoundManager> changed)
    {
        changed.Behaviour._uiManager.UpdateRoundCounterText(changed.Behaviour.RoundIndex.ToString());
    }

    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
        switch (currentGameState)
        {
            case LevelManager.GamePhase.GameStart:
                break;
            case LevelManager.GamePhase.Warmup:

                break;
            case LevelManager.GamePhase.Preparation:
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
