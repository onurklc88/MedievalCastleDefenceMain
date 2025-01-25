using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using static BehaviourRegistry;
public class TimeManager : ManagerRegistry
{
    [Networked] private TickTimer _matchTimer { get; set; }
    public LevelManager.GamePhase CurrentGameState { get; set; }

    private const float WARMUP_MATCH_TIME = 10f;
    private const float MATCH_PREPARATION_TIME = 3f;

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

    private void HandlePhaseTransition()
    {
        RemainingTime = 0;

        switch (CurrentGameState)
        {
            case LevelManager.GamePhase.Warmup:
                ChangeGamePhase(LevelManager.GamePhase.Preparation, MATCH_PREPARATION_TIME);
                break;
            case LevelManager.GamePhase.Preparation:
                ChangeGamePhase(LevelManager.GamePhase.RoundStart, 0);
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