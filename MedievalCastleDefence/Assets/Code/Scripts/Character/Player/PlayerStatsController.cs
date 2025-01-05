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
    private PlayerHUD _playerHUD;

    public override void Spawned()
    {
        _playerLocalStats.PlayerWarrior = SelectedCharacter;
        PlayerLocalStats = _playerLocalStats;
        //SelectedCharacter = _testStats.PlayerWarrior;
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        
    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _playerHUD = GetScript<PlayerHUD>();
        _playerHUD.UpdatePlayerNickname(_playerLocalStats.PlayerNickName.ToString());
    }

    public void SetPlayerInfo(PlayerInfo playerInfo)
    {
        _playerLocalStats = playerInfo;
        PlayerLocalStats = _playerLocalStats;
        Debug.Log("Nickname: " + PlayerLocalStats.PlayerNickName + " PlayerWarrior: " + PlayerLocalStats.PlayerWarrior);

    }

    public void UpdatePlayerKillCount()
    {
        _playerLocalStats.PlayerKillCount += 1;
        PlayerLocalStats = _playerLocalStats;
    }

    public void UpdatePlayerDieCount()
    {
        _playerLocalStats.PlayerDieCount += 1;
        PlayerLocalStats = _playerLocalStats;
    }



    private static void OnNetworkStatsChanged(Changed<PlayerStatsController> changed)
    {
        changed.Behaviour.PlayerNetworkStats = changed.Behaviour.PlayerLocalStats;
    }
    

}
