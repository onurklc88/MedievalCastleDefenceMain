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
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        

    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _playerHUD = GetScript<PlayerHUD>();
        _playerHUD.UpdatePlayerNickname(_playerLocalStats.PlayerNickName.ToString());
    }
   // [Rpc(RpcSources.All, RpcTargets.All)]
    public void SetPlayerInfo(PlayerInfo playerInfo)
    {
        _playerLocalStats = playerInfo;
        PlayerLocalStats = _playerLocalStats;
        //Debug.Log("Nickname: " + PlayerLocalStats.PlayerNickName + " PlayerWarrior: " + PlayerLocalStats.PlayerWarrior);

    }
   
    public void UpdatePlayerKillCountRpc()
    {
        if (!Object.HasStateAuthority) return;
        _playerLocalStats.PlayerKillCount += 1;
        PlayerLocalStats = _playerLocalStats;
        Debug.Log("Nickname: " + PlayerLocalStats.PlayerNickName + " PlayerKillCount: " + PlayerLocalStats.PlayerKillCount+ "PlayerDieCount: " +PlayerLocalStats.PlayerDieCount);
    }
    
    public void UpdatePlayerDieCountRpc()
    {
        if (!Object.HasStateAuthority) return;
        _playerLocalStats.PlayerDieCount += 1;
        PlayerLocalStats = _playerLocalStats;
        Debug.Log("Nickname: " + PlayerLocalStats.PlayerNickName + " PlayerKillCount: " + PlayerLocalStats.PlayerKillCount + "PlayerDieCount: " + PlayerLocalStats.PlayerDieCount);
    }



    private static void OnNetworkStatsChanged(Changed<PlayerStatsController> changed)
    {
        changed.Behaviour.PlayerNetworkStats = changed.Behaviour.PlayerLocalStats;
    }
    

}
