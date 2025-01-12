using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameStateListener
{
    public LevelManager.GamePhase CurrentGameState { get; set; }

    public void UpdateGameState(LevelManager.GamePhase currentGameState);



}
