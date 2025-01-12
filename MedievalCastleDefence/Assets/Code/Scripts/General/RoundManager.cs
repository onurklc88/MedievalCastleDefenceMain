using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;

public class RoundManager : ManagerRegistry, IGameStateListener
{
    public LevelManager.GamePhase CurrentGameState { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int CurrentRound;
    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
        CurrentGameState = currentGameState;
        switch (currentGameState)
        {
            case LevelManager.GamePhase.RoundStart:
                CurrentRound += 1;
                break;
        }

    }
}
