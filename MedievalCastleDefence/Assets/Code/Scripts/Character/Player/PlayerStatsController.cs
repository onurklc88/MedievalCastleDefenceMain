using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerStatsController : BehaviourRegistry
{
    [Networked] public PlayerStats PlayerStats { get; set; }
    private PlayerStats _testStats;
    public CharacterStats.CharacterType SelectedCharacter { get; set; }
  
    public override void Spawned()
    {
        //test area
        
        _testStats.PlayerWarrior = CharacterStats.CharacterType.KnightCommander;
        PlayerStats = _testStats;
        SelectedCharacter = _testStats.PlayerWarrior;
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        
    }

    /*
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerInfo(PlayerStats playerInfo)
    {
        _testStats = playerInfo;
        PlayerStats = _testStats;

    }
    */
}
