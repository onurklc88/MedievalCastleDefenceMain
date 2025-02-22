
using UnityEngine;
using System.Linq;
using Fusion;
using Cysharp.Threading.Tasks;
using System;
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
    [SerializeField] private Transform[] _redTeamPlayerSpawnPositions;
    [SerializeField] private Transform[] _blueTeamPlayerSpawnPositions;
    [SerializeField] private TestPlayerSpawner _testPlayerSpawner;
    
   
  
    public override void Spawned()
    {
       //EventLibrary.OnPlayerSelectTeam.AddListener(UpdatePlayerCountRpc);
      // EventLibrary.OnRespawnRequested.AddListener(HandleRespawnRequestRpc);
        ChangeGamePhaseRpc(GamePhase.Warmup);
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
    private async void UpdatePlayerCountRpc()
    {
        if (!Runner.IsSharedModeMasterClient) return;
        CurrentPlayerCount += 1;
        UpdateTeamPlayerCounts();
       
        if(CurrentPlayerCount == MaxPlayerCount)
        {
            EnsureAllPlayersHaveTeamsRpc();
            await UniTask.Delay(1000);
            BalanceTeamsRpc();
            await UniTask.Delay(2000);
            ChangeGamePhaseRpc(GamePhase.Preparation);
        }
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
                await UniTask.Delay(1000);
                DisablePlayerInputsRpc(false);
                break;

            case GamePhase.RoundStart:
                await UniTask.Delay(300);
                _uiManager.ShowScoreboard(true);
                if (!Runner.IsSharedModeMasterClient) return;
               
                EventLibrary.DebugMessage.Invoke("PlayerCount: " + CurrentPlayerCount + " RedTeamPlayerCount  " + RedTeamPlayerCount + "BlueTeamPlayerCount: " + BlueTeamPlayerCount);
                if(RedTeamPlayerCount == 0 || BlueTeamPlayerCount == 0)
                {
                    Debug.LogWarning("END GAME IMMEDIATELY");
                }
                ForcePlayersSpawnRpc();
                await UniTask.Delay(300);
                TeleportPlayersToStartPositionsRpc();
                await UniTask.Delay(2000);
                DisablePlayerInputsRpc(true);
                break;

            case GamePhase.RoundEnd:
                if (!Runner.IsSharedModeMasterClient) return;
               
                break;
            case GamePhase.GameEnd:
                //if (!Runner.IsSharedModeMasterClient) return;
                await UniTask.Delay(200);
                _uiManager.ShowEndingLeaderboardRpc();
              
                await UniTask.Delay(4000);
                if (!Runner.IsSharedModeMasterClient) return;
                DespawnPlayersSpawnRpc();
                break;
            } 
    }


  
    public void ChangeGamePhaseRpc(GamePhase newPhase)
    {
        CurrentGamePhase = newPhase;
        UpdateGameState(CurrentGamePhase);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void BalanceTeamsRpc()
    {
        if (!Runner.IsSharedModeMasterClient) return;

      

        for (int i = 0; i < 10; i++) 
        {
            int teamDifference = Math.Abs(RedTeamPlayerCount - BlueTeamPlayerCount);
            if (teamDifference < 2) break;

            if (RedTeamPlayerCount > BlueTeamPlayerCount)
            {
                MovePlayerToTeam(TeamManager.Teams.Red, TeamManager.Teams.Blue);
            }
            else if (BlueTeamPlayerCount > RedTeamPlayerCount)
            {
                MovePlayerToTeam(TeamManager.Teams.Blue, TeamManager.Teams.Red);
            }
        }

        await UniTask.Delay(500);
        //UpdateTeamPlayerCounts();

    }
    private void MovePlayerToTeam(TeamManager.Teams fromTeam, TeamManager.Teams toTeam)
    {
        var playerList = Runner.ActivePlayers.ToList();
        foreach (var player in playerList)
        {
            var playerNetworkObject = Runner.GetPlayerObject(player);
            if (playerNetworkObject != null)
            {
                var playerStats = playerNetworkObject.GetComponentInParent<PlayerStatsController>();
                if (playerStats != null)
                {
                    var playerTeam = playerStats.PlayerNetworkStats.PlayerTeam;

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
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RestorePlayerWarriorRpc()
    {
        var alivePlayerList = 0;
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            
            if (playerObject == null)
            {
                //Debug.LogWarning($"PlayerObject for player {player} is null.");
                continue;
            }

            var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();
            if (characterHealth == null)
            {
                //Debug.LogWarning($"CharacterHealth for player {player} is null.");
                continue;
            }

           
            if (playerObject.GetComponentInParent<CharacterHealth>()!= null)
            {
               // var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();
            
              
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
    public void DespawnPlayersSpawnRpc()
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
    private bool _hasForcedSpawn;

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ForcePlayersSpawnRpc()
    {
        if (_hasForcedSpawn) return;
        _hasForcedSpawn = true;

        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);

            if (playerObject != null)
            {
               
                var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();
               
                if (characterHealth.NetworkedHealth <= 0)
                {
                    //characterHealth.IsPlayerDead = false;
                    var playerWarriorType = playerObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerWarrior;
                    RespawnWithDelay(player, playerWarriorType).Forget();
                }
               
            }
            UniTask.Void(async () =>
            {
                await UniTask.Delay(1000);
                var playerObjectAfterDelay = Runner.GetPlayerObject(player);
                if (playerObjectAfterDelay == null)
                {
                    UpdateTeamPlayerCounts();
                    TeamManager.Teams availableTeam = DetermineAvailableTeam();
                    EventLibrary.OnPlayerSelectTeam.Invoke(player, CharacterStats.CharacterType.KnightCommander, availableTeam);
                  
                    return;
                }

                var playerStatsAfterDelay = playerObjectAfterDelay.GetComponentInParent<PlayerStatsController>();
                if (playerStatsAfterDelay == null) return;

                var existingTeam = playerStatsAfterDelay.PlayerNetworkStats.PlayerTeam;
                if (existingTeam == TeamManager.Teams.None)
                {
                    UpdateTeamPlayerCounts();
                    TeamManager.Teams availableTeam = DetermineAvailableTeam();
                    EventLibrary.OnPlayerSelectTeam.Invoke(player, CharacterStats.CharacterType.KnightCommander, availableTeam);
                }
            });
     

        }

      
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void EnsureAllPlayersHaveTeamsRpc()
    {
        //if (!Runner.IsSharedModeMasterClient) return;
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            if(playerObject.GetComponentInParent<PlayerStatsController>() != null)
            {
                var playerTeam = playerObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;
                if (playerTeam == TeamManager.Teams.None)
                {
                    TeamManager.Teams availableTeam = DetermineAvailableTeam();
                    EventLibrary.OnPlayerSelectTeam.Invoke(player, CharacterStats.CharacterType.KnightCommander, availableTeam);
                }
            }
            
           
        }
    }
    private async UniTaskVoid RespawnWithDelay(PlayerRef player, CharacterStats.CharacterType playerWarriorType)
    {
        await UniTask.Delay(50, cancellationToken: this.GetCancellationTokenOnDestroy());
        EventLibrary.OnRespawnRequested.Invoke(player, playerWarriorType);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
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


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void TeleportPlayersToStartPositionsRpc()
    {
        

        int currentRedTeamCount = 0;
        int currentBlueTeamCount = 0;

        for (int i = 0; i < Runner.ActivePlayers.ToList().Count; i++)
        {
            if (Runner.TryGetPlayerObject(Runner.ActivePlayers.ToList()[i], out var playerNetworkObject))
            {
                Debug.Log("CurrentPlayerIndexCount: " + currentRedTeamCount);
                var playerTeam = playerNetworkObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;
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
                Debug.Log("isSpawnPosition vector3.zero? : " + spawnPosition);
                if (spawnPosition != Vector3.zero)
                {
                    playerNetworkObject.transform.position = spawnPosition;
                    
                }
            }
        }
    }
    public void UpdateTeamPlayerCounts()
    {

       // if (!Runner.IsSharedModeMasterClient) return;
       
        var playerList = Runner.ActivePlayers.ToList();
        RedTeamPlayerCount = 0;
        BlueTeamPlayerCount = 0;
        foreach (var player in playerList)
        {
            var playerNetworkObject = Runner.GetPlayerObject(player);
            if (playerNetworkObject != null)
            {
                var playerStats = playerNetworkObject.GetComponentInParent<PlayerStatsController>();
                if (playerStats != null)
                {
                    CurrentPlayerCount += 1;
                    var playerTeam = playerStats.PlayerTeam;
                    if (playerTeam == TeamManager.Teams.Red)
                        RedTeamPlayerCount += 1;
                    else if (playerTeam == TeamManager.Teams.Blue)
                        BlueTeamPlayerCount += 1;
                }
            }
            
        }
       
        
        Debug.LogError("PlayerCount: " + CurrentPlayerCount+  " RedTeamPlayerCount  " + RedTeamPlayerCount + "BlueTeamPlayerCount: " + BlueTeamPlayerCount);
    }
    TeamManager.Teams DetermineAvailableTeam()
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



    #region legacy
    /*
   [Rpc(RpcSources.All, RpcTargets.All)]
   private void CheckRoundEndByDefeatRpc(TeamManager.Teams playerTeam)
   {
       if (!Runner.IsSharedModeMasterClient) return;
       if (playerTeam == TeamManager.Teams.Red)
       {
           _blueTeamDeadCount += 1;

       }
       else
       {
           _redTeamDeadCount += 1;
       }




       if (_redTeamDeadCount == TeamsPlayerCount)
       {
           _blueTeamScore += 1;
           _uiManager.UpdateTeamScoreRpc(TeamManager.Teams.Blue, _blueTeamScore);
       }
       if (_blueTeamDeadCount == TeamsPlayerCount)
       {
           _redTeamScore += 1;
           _uiManager.UpdateTeamScoreRpc(TeamManager.Teams.Red, _redTeamScore);
       }
       _blueTeamScore = 0;
       _redTeamScore = 0;
   }


   private static void OnPlayerCountChange(Changed<LevelManager> changed)
   {
      Debug.LogWarning($"<color=yellow>Player count updated: {changed.Behaviour.CurrentPlayerCount}</color>");

   }

   private static void OnRoundCounterChange(Changed<LevelManager> changed)
   {
       changed.Behaviour._uiManager.UpdateRoundCounterText(changed.Behaviour.RoundIndex.ToString());
   }
   */
    #endregion
}
