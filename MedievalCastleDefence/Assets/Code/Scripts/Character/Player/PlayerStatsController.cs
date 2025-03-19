using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static BehaviourRegistry;
public class PlayerStatsController : CharacterRegistry, IRPCListener
{
    [Networked] public PlayerInfo PlayerNetworkStats { get; set; }
    private PlayerInfo _playerLocalStats;
    public CharacterStats.CharacterType SelectedCharacter;
    [Networked(OnChanged = nameof(OnNetworkStatsChanged))] public PlayerInfo PlayerLocalStats { get; set; }
    [Networked(OnChanged = nameof(OnNetworkPlayerTeamChange))] public TeamManager.Teams PlayerTeam { get; set; }
    [Networked(OnChanged = nameof(Test))] public LevelManager.GamePhase CurrentGamePhase { get; set; }
   

    [SerializeField] private Material[] _teamMaterials;
    [SerializeField] private SkinnedMeshRenderer _playerMeshrenderer;
    private PlayerHUD _playerHUD;
    private TeamManager.Teams _playerTeam;
   

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameStateRpc);
    }


    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameStateRpc);
       
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
        _playerTeam = PlayerTeam;
        //Debug.Log("PlayerTeam: " + PlayerTeam);
        //Debug.Log("Nickname: " + PlayerLocalStats.PlayerNickName + " PlayerWarrior: " + PlayerLocalStats.PlayerWarrior);
    }
   
    public void UpdatePlayerKillCountRpc()
    {
        if (!Object.HasStateAuthority || CurrentGamePhase == LevelManager.GamePhase.Warmup) return;
       
        _playerLocalStats.PlayerKillCount += 1;
        PlayerLocalStats = _playerLocalStats;
    }
    
    public void UpdatePlayerDieCountRpc()
    {
        if (!Object.HasStateAuthority || CurrentGamePhase == LevelManager.GamePhase.Warmup) return;
        _playerLocalStats.PlayerDieCount += 1;
        PlayerLocalStats = _playerLocalStats;
    }

    public List<PlayerRef> GetAliveTeamPlayers()
    {
        List<PlayerRef> team = new List<PlayerRef>();
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            var playerStats = playerObject.GetComponentInParent<PlayerStatsController>();
            var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();

            if (playerStats != null && playerStats.PlayerNetworkStats.PlayerTeam == PlayerNetworkStats.PlayerTeam && characterHealth.NetworkedHealth > 0)
            {
                team.Add(player);
            }
        }

        return team;
    }



    private static void OnNetworkStatsChanged(Changed<PlayerStatsController> changed)
    {
        changed.Behaviour.PlayerNetworkStats = changed.Behaviour.PlayerLocalStats;
    }

    private static void Test(Changed<PlayerStatsController> changed)
    {
     
    }

    private static void OnNetworkPlayerTeamChange(Changed<PlayerStatsController> changed)
    {
        if (changed.Behaviour.PlayerTeam == TeamManager.Teams.None) return;
       // Debug.Log("Oyuncunun rengi deðiþtir: " +changed.Behaviour.PlayerTeam+ " PlayerID: " +changed.Behaviour.transform.GetComponentInParent<NetworkObject>().Id);
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
   
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
    }

    void OnApplicationQuit()
    {
       EventLibrary.test.Invoke(_playerTeam, false);
    }
}
