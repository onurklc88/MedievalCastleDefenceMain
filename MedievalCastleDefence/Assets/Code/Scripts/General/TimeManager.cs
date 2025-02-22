using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;
public class TimeManager : ManagerRegistry
{
    [Networked] private TickTimer _matchTimer { get; set; }
    public LevelManager.GamePhase CurrentGameState { get; set; }

    private const float WARMUP_MATCH_TIME = 10f;
    private const float MATCH_PREPARATION_TIME = 4f;

    private float _currentTimeAmount;
    private UIManager _uiManager;
    [Networked(OnChanged = nameof(OnTimerChanged))] public float RemainingTime { get; set; }
    private float _matchDuration;

    private LevelManager _levelManager;


    private void Awake()
    {
        InitScript(this);
    }

    private void Start()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameStateRpc);
        _levelManager = GetScript<LevelManager>();
        _uiManager = GetScript<UIManager>();
    }



    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsSharedModeMasterClient) return;

        if (RemainingTime > 0)
        {
            RemainingTime -= Runner.DeltaTime;
        }
        else
        {
            HandlePhaseTransition();
        }
    }

    private async void HandlePhaseTransition()
    {
        RemainingTime = 0;

        switch (CurrentGameState)
        {
            case LevelManager.GamePhase.Warmup:
               
                /*
                _levelManager.BalanceTeamsRpc();
                await UniTask.Delay(1000);
                _levelManager.ChangeGamePhaseRpc(LevelManager.GamePhase.Preparation);
                */

                _levelManager.ForcePlayersSpawnRpc();
                await UniTask.Delay(500);
                //_levelManager.BalanceTeamsRpc();
                await UniTask.Delay(500);
                
                _levelManager.ChangeGamePhaseRpc(LevelManager.GamePhase.Preparation);
                RemainingTime = MATCH_PREPARATION_TIME;
                break;
            case LevelManager.GamePhase.Preparation:
                RemainingTime = 0;
                //_levelManager.UpdateTeamPlayerCounts();
                _levelManager.ChangeGamePhaseRpc(LevelManager.GamePhase.RoundStart);
                break;
        }
    }

    private void ChangeGamePhase(LevelManager.GamePhase newPhase, float newTime)
    {
        CurrentGameState = newPhase;
        RemainingTime = newTime;
        EventLibrary.OnGamePhaseChange.Invoke(newPhase);
    }
    private static void OnTimerChanged(Changed<TimeManager> changed)
    {
        var time = TimeSpan.FromSeconds(changed.Behaviour.RemainingTime);
        changed.Behaviour._uiManager.UpdateTimer($"{time.Minutes:D2}:{time.Seconds:D2}");
    }

    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {

        CurrentGameState = currentGameState;

        switch (currentGameState)
        {
            case LevelManager.GamePhase.Warmup:
               _matchDuration = WARMUP_MATCH_TIME;
                break;
            case LevelManager.GamePhase.Preparation:
                _matchDuration = MATCH_PREPARATION_TIME;
                break;
            case LevelManager.GamePhase.RoundStart:
                RemainingTime = 0;
                break;
            case LevelManager.GamePhase.RoundEnd:
               
                break;
        }
      
        if (Runner.IsSharedModeMasterClient)
        {
           RemainingTime = _matchDuration;
        }

    }
}