using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public interface IRPCListener
{

    public LevelManager.GamePhase CurrentGamePhase { get; set; }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState);
}
