using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerStatsController : BehaviourRegistry
{
    [Networked] public PlayerInfo PlayerNetworkStats { get; set; }
    private PlayerInfo _playerLocalStats;
    public CharacterStats.CharacterType SelectedCharacter;
    [Networked(OnChanged = nameof(OnNetworkStatsChanged))] public PlayerInfo PlayerLocalStats { get; set; }
 

    public override void Spawned()
    {
        //test area

        _playerLocalStats.PlayerWarrior = SelectedCharacter;
        PlayerLocalStats = _playerLocalStats;
        //SelectedCharacter = _testStats.PlayerWarrior;
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        
    }

    public void SetPlayerInfo(PlayerInfo playerInfo)
    {
        _playerLocalStats = playerInfo;
        PlayerLocalStats = _playerLocalStats;

    }

    private static void OnNetworkStatsChanged(Changed<PlayerStatsController> changed)
    {
        changed.Behaviour.PlayerNetworkStats = changed.Behaviour.PlayerLocalStats;
    }
    

}
