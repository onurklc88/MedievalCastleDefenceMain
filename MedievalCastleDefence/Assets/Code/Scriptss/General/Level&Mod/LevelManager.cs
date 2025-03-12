using UnityEngine;
using System.Linq;
using Fusion;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using static BehaviourRegistry;

public class LevelManager : ManagerRegistry, IGameStateListener
{
   [Networked(OnChanged = nameof(OnPlayerCountChange))]  public int CurrentPlayerCount { get; set; }
   [Networked(OnChanged = nameof(OnPlayerCountChange))]  public int RedTeamPlayerCount { get; set; }
   [Networked(OnChanged = nameof(OnPlayerCountChange))]  public int BlueTeamPlayerCount { get; set; }
   [Networked(OnChanged = nameof(OnGamePhaseChanged))] public GamePhase CurrentGamePhase { get; set; }
   public enum GamePhase
   {
        None,
        GameStart,
        Warmup,
        Preparation,
        RoundStart,
        RoundEnd,
        GameEnd
   }
   
    public int MaxPlayerCount;
    public int TeamsPlayerCount;
    public GameManager.GameModes GameMode;
    public int TotalLevelRound;
    private UIManager _uiManager;
    private List<PlayerRef> _spawnedPlayers = new List<PlayerRef>();
    [SerializeField] private Transform[] _redTeamPlayerSpawnPositions;
    [SerializeField] private Transform[] _blueTeamPlayerSpawnPositions;
    [SerializeField] private TestPlayerSpawner _testPlayerSpawner;
    private List<PlayerRef> _activePlayers;
    [Networked, Capacity(10)] 
    public NetworkLinkedList<PlayerRef> SpawnedPlayers { get; }

    public override void Spawned()
    {
        EventLibrary.test.AddListener(UpdatePlayerCountRpc);
       ChangeGamePhase(GamePhase.Warmup);
    }

    private void Awake()
    {
        SetGameMode();
        InitScript(this);
    }

    private void Start()
    {
        _uiManager = GetScript<UIManager>();
    }
  
    private void SetGameMode()
    {
        switch (GameMode)
        {
            case GameManager.GameModes.OneVsOne:
                MaxPlayerCount = 2;
                break;
            case GameManager.GameModes.TwoVsTwo:
                MaxPlayerCount = 4;
                break;
            case GameManager.GameModes.ThreeVsThree:
                MaxPlayerCount = 6;
                break;
        }

        TeamsPlayerCount = MaxPlayerCount / 2;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdatePlayerCountRpc(TeamManager.Teams playerTeam, bool isPlayerJoin)
    {
        if (!Runner.IsSharedModeMasterClient) return;

        CurrentPlayerCount += isPlayerJoin ? 1 : -1;

        if (playerTeam == TeamManager.Teams.None)
        {
            Debug.LogError("PlayerTeam Return None");
            return;
        }

        if (playerTeam == TeamManager.Teams.Red)
            RedTeamPlayerCount += isPlayerJoin ? 1 : -1;
        else if (playerTeam == TeamManager.Teams.Blue)
            BlueTeamPlayerCount += isPlayerJoin ? 1 : -1;


       
        if (CurrentPlayerCount == MaxPlayerCount)
        {
           ChangeGamePhase(GamePhase.Preparation);
        }
        
        Debug.LogError("PlayerCount: " + CurrentPlayerCount + " RedTeamPlayerCount  " + RedTeamPlayerCount + "BlueTeamPlayerCount: " + BlueTeamPlayerCount);
    }
   
    public async void UpdateGameState(GamePhase currentGameState)
    {
            
            switch (currentGameState)
            {
            case GamePhase.GameStart:
                break;
            case GamePhase.Warmup:
               
                break;
            case GamePhase.Preparation:
                if (!Runner.IsSharedModeMasterClient) return;
                RestorePlayerWarriorRpc();
                await UniTask.Delay(500);
                DisablePlayerInputsRpc(false);
                await UniTask.Delay(3000);
                ChangeGamePhase(GamePhase.RoundStart);
                break;

            case GamePhase.RoundStart:
                if (!Runner.IsSharedModeMasterClient) return;
                await ForcePlayersSpawnAsync();
                break;

            case GamePhase.RoundEnd:
               
                break;
            case GamePhase.GameEnd:
                await UniTask.Delay(200);
                _uiManager.ShowEndingLeaderboardRpc();
              
                await UniTask.Delay(4000);
                if (!Runner.IsSharedModeMasterClient) return;
                DespawnAllPlayersRpc();
                break;
            } 
    }

    public void ChangeGamePhase(GamePhase newPhase)
    {
        CurrentGamePhase = newPhase;
        UpdateGameState(CurrentGamePhase);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void BalanceTeamsRpc()
    {
        Debug.Log("B");
        for (int i = 0; i < 10; i++) 
        {
            int teamDifference = Math.Abs(RedTeamPlayerCount - BlueTeamPlayerCount);
            
            if (teamDifference < 2) break;
            CurrentPlayerCount -= 1;
            if (RedTeamPlayerCount > BlueTeamPlayerCount)
            {
                RedTeamPlayerCount -= 1;
                MovePlayerToTeamRpc(TeamManager.Teams.Red, TeamManager.Teams.Blue);
                UpdatePlayerCountRpc(TeamManager.Teams.Blue, true);
            }
            else if (BlueTeamPlayerCount > RedTeamPlayerCount)
            {
                BlueTeamPlayerCount -= 1;
                MovePlayerToTeamRpc(TeamManager.Teams.Blue, TeamManager.Teams.Red);
                UpdatePlayerCountRpc(TeamManager.Teams.Red, true);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void MovePlayerToTeamRpc(TeamManager.Teams fromTeam, TeamManager.Teams toTeam)
    {
      
        foreach (var player in Runner.ActivePlayers)
        {
            var playerNetworkObject = Runner.GetPlayerObject(player);
            if (playerNetworkObject != null)
            {
                var playerStats = playerNetworkObject.GetComponentInParent<PlayerStatsController>();
                if (playerStats != null)
                {
                    var playerTeam = playerStats.PlayerTeam;
                    if (playerTeam == fromTeam)
                    {
                        PlayerInfo updatedInfo = playerStats.PlayerNetworkStats;
                        updatedInfo.PlayerTeam = toTeam;
                        playerStats.SetPlayerInfo(updatedInfo);
                        break;
                    }
                }
            }
        }

       
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void CheckPlayerCountRpc()
    {
        CurrentPlayerCount -= 1;
        RedTeamPlayerCount = 0;
        BlueTeamPlayerCount = 0;
        foreach (var player in Runner.ActivePlayers)
        {
            var playerNetworkObject = Runner.GetPlayerObject(player);
            if (playerNetworkObject != null)
            {
                var playerStats = playerNetworkObject.GetComponentInParent<PlayerStatsController>();
                if (playerStats != null)
                {
                    var playerTeam = playerStats.PlayerTeam;
                    
                    if (playerTeam == TeamManager.Teams.Red)
                    {
                        RedTeamPlayerCount += 1;
                    }
                    else
                    {
                        BlueTeamPlayerCount += 1;
                    }
                }
            }
        }
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void NotifySpawnCompletedRpc(PlayerRef playerRef)
    {
        SpawnedPlayers.Add(playerRef);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RestorePlayerWarriorRpc()
    {
        var alivePlayerList = 0;
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            
            if (playerObject == null)
            {
                continue;
            }

            var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();
            if (characterHealth == null)
            {
                 continue;
            }

           
            if (playerObject.GetComponentInParent<CharacterHealth>()!= null)
            {
                if (playerObject != null && characterHealth.NetworkedHealth > 0)
                {
                    alivePlayerList++;
                    var characterStamina = playerObject.GetComponentInParent<CharacterStamina>();
                    var characterDecals = playerObject.GetComponentInParent<CharacterDecals>();
                    characterHealth.ResetPlayerHealth();
                    characterStamina.ResetPlayerStamina();
                    characterDecals.DisableBloodDecals();
                
                }
            }
        }
       
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void DespawnAllPlayersRpc()
    {
        //if (!Runner.IsSharedModeMasterClient) return;
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);

            if (playerObject != null)
            {

                Runner.Despawn(playerObject);
            }

        }
    }
   
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RequestDespawnRpc(PlayerRef playerRef)
    {
      
        if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
        {
            Debug.Log("PlayerId: " + playerNetworkObject.GetComponentInParent<NetworkObject>().Id);
            Runner.Despawn(playerNetworkObject);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ForcePlayersSpawnRpc()
    {
        Debug.Log("A");
        _activePlayers = Runner.ActivePlayers.ToList();
       foreach (var player in Runner.ActivePlayers)
       {
            
            var playerObject = Runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();
                if (characterHealth.NetworkedHealth <= 0)
                {
                    var playerWarriorType = playerObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerWarrior;
                    RespawnWithDelay(player, playerWarriorType).Forget();
                }
            }
            else
            {
               TeamManager.Teams availableTeam = DetermineAvailableTeamRpc();
               EventLibrary.OnPlayerSelectTeam.Invoke(player, CharacterStats.CharacterType.KnightCommander, availableTeam);
               continue;

            }
       }
    }
   
    private async UniTaskVoid RespawnWithDelay(PlayerRef player, CharacterStats.CharacterType playerWarriorType)
    {
        await UniTask.Delay(50, cancellationToken: this.GetCancellationTokenOnDestroy());
        EventLibrary.OnRespawnRequested.Invoke(player, playerWarriorType);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void DisablePlayerInputsRpc(bool condition)
    {
    
        foreach (var player in Runner.ActivePlayers)
        {

            var playerObject = Runner.GetPlayerObject(player);
            if(playerObject != null)
            {
              
               playerObject.GetComponentInParent<CharacterController>().enabled = condition;
            }
            else
            {
                Debug.LogWarning("playerObjectYok");
            }
        }
    }
  
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void TeleportPlayersToStartPositionsRpc()
    {
        Debug.Log("C");

        int currentRedTeamCount = 0;
        int currentBlueTeamCount = 0;
        
        for (int i = 0; i < Runner.ActivePlayers.ToList().Count; i++)
        {
            if (Runner.TryGetPlayerObject(Runner.ActivePlayers.ToList()[i], out var playerNetworkObject))
            {
                var playerTeam = playerNetworkObject.GetComponentInParent<PlayerStatsController>().PlayerTeam;
                playerNetworkObject.GetComponentInParent<CharacterController>().enabled = false;
                Vector3 spawnPosition = Vector3.zero;
                if (playerTeam == TeamManager.Teams.Red)
                {
                  spawnPosition = _testPlayerSpawner.RedTeamPlayerSpawnPositions[currentRedTeamCount].transform.position;
                  currentRedTeamCount++;
                }
                else
                {
                    spawnPosition = _testPlayerSpawner.BlueTeamPlayerSpawnPositions[currentBlueTeamCount].transform.position;
                    currentBlueTeamCount++;
                }
               
                if (spawnPosition != Vector3.zero)
                {
                    playerNetworkObject.transform.position = spawnPosition;
                    
                }
            }
        }
      
    }
   
    TeamManager.Teams DetermineAvailableTeamRpc()
    {
        if (RedTeamPlayerCount >= MaxPlayerCount / 2) return TeamManager.Teams.Blue;
        if (BlueTeamPlayerCount >= MaxPlayerCount / 2) return TeamManager.Teams.Red;
        return RedTeamPlayerCount <= BlueTeamPlayerCount ? TeamManager.Teams.Red : TeamManager.Teams.Blue;
    }

 

    private static void OnPlayerCountChange(Changed<LevelManager> changed)
    {
        //Debug.LogWarning($"<color=yellow>Player count updated: {changed.Behaviour.CurrentPlayerCount}</color>");

    }
    private static void OnGamePhaseChanged(Changed<LevelManager> changed)
    {

        EventLibrary.OnGamePhaseChange.Invoke(changed.Behaviour.CurrentGamePhase);
    }


    #region UniTaskAsync
    public async UniTask ForcePlayersSpawnAsync()
    {
        ForcePlayersSpawnRpc();
        await UniTask.WaitUntil(() => SpawnedPlayers.Count() == _activePlayers.Count());
        await UniTask.Delay(500);
        await BalanceTeamsAsync();
        await DisablePlayerInputsAsync(false);
        await TeleportPlayersToStartPositionsAsync();
        await UniTask.Delay(4000);
        DisablePlayerInputsRpc(true);
    }

    public async UniTask BalanceTeamsAsync()
    {
        BalanceTeamsRpc();
        await UniTask.Yield();
    }

    public async UniTask TeleportPlayersToStartPositionsAsync()
    {
        TeleportPlayersToStartPositionsRpc();
        await UniTask.Yield();
    }

    public async UniTask DisablePlayerInputsAsync(bool disable)
    {
        DisablePlayerInputsRpc(disable);
        await UniTask.Yield();
    }

    #endregion


}

