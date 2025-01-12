using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using static BehaviourRegistry;
public class TimeManager : ManagerRegistry, IGameStateListener
{
    [Networked] private TickTimer _matchTimer { get; set; }
    public LevelManager.GamePhase CurrentGameState { get; set; }

    private const float WARMUP_MATCH_TIME = 300f;
   
    private float _currentTimeAmount;
    private UIManager _uiManager;


    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
    }

    private void OnDisable()
    {
        
    }
    private void Awake()
    {
        InitScript(this);
    }

    private void Start()
    {
        _uiManager = GetScript<UIManager>();
    }



    public override void FixedUpdateNetwork()
    {
        if (_matchTimer.Expired(Runner) == false && _matchTimer.RemainingTime(Runner).HasValue)
        {
            var timeSpan = TimeSpan.FromSeconds(_matchTimer.RemainingTime(Runner).Value);
            _uiManager.UpdateTimer($"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");
          
        }
        else if (_matchTimer.Expired(Runner))
        {
            _matchTimer = TickTimer.None;
            /*
            if (CurrentGameState == GameManager.GameState.Warmup)
            {
                CurrentGameState = GameManager.GameState.MatchStart;
                EventLibrary.OnGameStateChange?.Invoke(CurrentGameState);
                StartCoroutine(DelayEvents());
            }
            */
        }
    }


    public void UpdateMatchTimer()
    {

    }

    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
        Debug.Log("Current: " + currentGameState);
        CurrentGameState = currentGameState;

        switch (currentGameState)
        {
            case LevelManager.GamePhase.Warmup:
                _currentTimeAmount = WARMUP_MATCH_TIME;
                break;
            case LevelManager.GamePhase.RoundStart:
               
                break;
            case LevelManager.GamePhase.RoundEnd:

                break;
        }

        _matchTimer = TickTimer.CreateFromSeconds(Runner, _currentTimeAmount);
    }
}
