using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
public class PlayerStatsController : CharacterRegistry
{
    [Networked] public PlayerInfo PlayerNetworkStats { get; set; }
    private PlayerInfo _playerLocalStats;
    public CharacterStats.CharacterType SelectedCharacter;
    [Networked(OnChanged = nameof(OnNetworkStatsChanged))] public PlayerInfo PlayerLocalStats { get; set; }
    [Networked(OnChanged = nameof(OnNetworkPlayerTeamChange))] public TeamManager.Teams PlayerTeam { get; set; }
    [SerializeField] private Material[] _teamMaterials;
    [SerializeField] private SkinnedMeshRenderer _playerMeshrenderer;
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
  
    public void SetPlayerInfo(PlayerInfo playerInfo)
    {
        _playerLocalStats = playerInfo;
        PlayerLocalStats = _playerLocalStats;
        PlayerTeam = playerInfo.PlayerTeam;
        Debug.Log("PlayerTeam: " + PlayerTeam);
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

    private static void OnNetworkPlayerTeamChange(Changed<PlayerStatsController> changed)
    {
        if (changed.Behaviour.PlayerTeam == TeamManager.Teams.None) return;

        var renderer = changed.Behaviour._playerMeshrenderer;
        if (renderer != null)
        {
           
            var materials = renderer.materials;

            
            if (changed.Behaviour.PlayerTeam == TeamManager.Teams.Red)
            {
                materials[0] = changed.Behaviour._teamMaterials[0];
            }
            else
            {
                materials[0] = changed.Behaviour._teamMaterials[1];
            }

           
            renderer.materials = materials;
        }
    }
    

}
