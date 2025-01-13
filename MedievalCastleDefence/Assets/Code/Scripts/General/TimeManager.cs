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

    private const float WARMUP_MATCH_TIME = 20f;
    private const float MATCH_PREPERATION_TIME = 10f;

    private float _currentTimeAmount;
    private UIManager _uiManager;
    [Networked(OnChanged = nameof(OnTimerChanged))] public float RemainingTime { get; set; }
    private float _matchDuration;



    private void Awake()
    {
        InitScript(this);
    }

    private void Start()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameStateRpc);
        _uiManager = GetScript<UIManager>();
    }



    public override void FixedUpdateNetwork()
    {
      
        if (Runner.IsSharedModeMasterClient)
        {
            if (RemainingTime > 0)
            {
                RemainingTime -= Runner.DeltaTime;
            }
            else
            {
                RemainingTime = 0;
                //CurrentGameState = LevelManager.GamePhase.RoundStart;
                EventLibrary.OnGamePhaseChange.Invoke(LevelManager.GamePhase.RoundStart);
            }
        }
    }
    private static void OnTimerChanged(Changed<TimeManager> changed)
    {
        var time = TimeSpan.FromSeconds(changed.Behaviour.RemainingTime);
        changed.Behaviour._uiManager.UpdateTimer($"{time.Minutes:D2}:{time.Seconds:D2}");
    }

    public void UpdateMatchTimer()
    {

    }

  
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {

        CurrentGameState = currentGameState;

        switch (currentGameState)
        {
            case LevelManager.GamePhase.Warmup:
               _matchDuration = WARMUP_MATCH_TIME;
                break;
            case LevelManager.GamePhase.Prepertaion:
                _matchDuration = MATCH_PREPERATION_TIME;
                break;
            case LevelManager.GamePhase.RoundStart:
                
                break;
            case LevelManager.GamePhase.RoundEnd:
                _matchDuration = 0;
                break;
        }
      
        if (Runner.IsSharedModeMasterClient)
        {
           RemainingTime = _matchDuration;
        }

    }
}