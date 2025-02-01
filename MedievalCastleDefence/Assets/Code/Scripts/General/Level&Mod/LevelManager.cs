using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
using Cysharp.Threading.Tasks;
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
    private List<PlayerRef> _winnerPlayers = new List<PlayerRef>();
   
  
    public override void Spawned()
    {
       EventLibrary.OnPlayerSelectWarrior.AddListener(UpdatePlayerCountRpc);
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
            EnsureAllPlayersHaveTeams(); 
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
               if (!Runner.IsSharedModeMasterClient) return;
                await UniTask.Delay(150);
                ForcePlayersSpawnRpc();
                TeleportPlayersToStartPositionsRpc();
                await UniTask.Delay(2000);
                DisablePlayerInputsRpc(true);
                break;

            case GamePhase.RoundEnd:
                if (!Runner.IsSharedModeMasterClient) return;
               
                break;
            case GamePhase.GameEnd:

                break;
            } 
    }


  
    public void ChangeGamePhaseRpc(GamePhase newPhase)
    {
        CurrentGamePhase = newPhase;
        UpdateGameState(CurrentGamePhase);
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
                Debug.LogWarning($"PlayerObject for player {player} is null.");
                continue;
            }

            var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();
            if (characterHealth == null)
            {
                Debug.LogWarning($"CharacterHealth for player {player} is null.");
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
                _winnerPlayers.Add(player);
            }
            }
        }
       
    }
  
   [Rpc(RpcSources.All, RpcTargets.All)]
    public void ForcePlayersSpawnRpc()
    {

      
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
                else
                {
                    _winnerPlayers.Add(player);
                }
                
            }
            UniTask.Void(async () =>
            {
                await UniTask.Delay(1000);
                var playerObjectAfterDelay = Runner.GetPlayerObject(player);
                if (playerObjectAfterDelay == null) return;

                var existingTeam = playerObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;
                if (existingTeam == TeamManager.Teams.None) 
                {
                    UpdateTeamPlayerCounts();
                    TeamManager.Teams availableTeam = DetermineAvailableTeam();
                    EventLibrary.OnPlayerSelectTeam.Invoke(player, CharacterStats.CharacterType.KnightCommander, availableTeam);
                }
            });

        }

      
    }
    private void EnsureAllPlayersHaveTeams()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            var playerTeam = playerObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;
            if (playerTeam == TeamManager.Teams.None) 
            {
                TeamManager.Teams availableTeam = DetermineAvailableTeam();
                EventLibrary.OnPlayerSelectTeam.Invoke(player, CharacterStats.CharacterType.KnightCommander, availableTeam);
            }
        }
    }
    private async UniTaskVoid RespawnWithDelay(PlayerRef player, CharacterStats.CharacterType playerWarriorType)
    {
        int delayMs = Random.Range(50, 100); 
        await UniTask.Delay(delayMs, cancellationToken: this.GetCancellationTokenOnDestroy());
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
        System.Random random = new System.Random();

        Vector3[] shuffledRedPositions = _testPlayerSpawner.RedTeamPlayerSpawnPositions
            .Select(t => t.position)
            .OrderBy(x => random.Next())
            .ToArray();

        Vector3[] shuffledBluePositions = _testPlayerSpawner.BlueTeamPlayerSpawnPositions
            .Select(t => t.position)
            .OrderBy(x => random.Next())
            .ToArray();

        List<Vector3> availableRedPositions = new List<Vector3>(shuffledRedPositions);
        List<Vector3> availableBluePositions = new List<Vector3>(shuffledBluePositions);

        foreach (var playerNetworkObject in _winnerPlayers)
        {
            var playerObject = Runner.GetPlayerObject(playerNetworkObject);
            if (playerObject == null) continue;

            var playerStats = playerObject.GetComponentInParent<PlayerStatsController>();
            if (playerStats == null) continue;

            var playerNetworkStats = playerStats.PlayerNetworkStats;
            if (playerNetworkStats.PlayerTeam == TeamManager.Teams.Red && availableRedPositions.Count > 0)
            {
                playerObject.transform.position = availableRedPositions[0];
                availableRedPositions.RemoveAt(0);
            }
            else if (playerNetworkStats.PlayerTeam == TeamManager.Teams.Blue && availableBluePositions.Count > 0)
            {
                playerObject.transform.position = availableBluePositions[0];
                availableBluePositions.RemoveAt(0);
            }
        }

        _winnerPlayers.Clear();
    }

    public void UpdateTeamPlayerCounts()
    {

        if (!Runner.IsSharedModeMasterClient) return;
        Debug.LogError("KESLAN");
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
                    var playerTeam = playerStats.PlayerNetworkStats.PlayerTeam;
                    if (playerTeam == TeamManager.Teams.Red)
                        RedTeamPlayerCount += 1;
                    else if (playerTeam == TeamManager.Teams.Blue)
                        BlueTeamPlayerCount += 1;
                }
            }
            /*
            else
            {
                TeamManager.Teams availableTeam = DetermineAvailableTeam();
                EventLibrary.OnPlayerSelectTeam.Invoke(
                    player,
                    CharacterStats.CharacterType.KnightCommander,
                    availableTeam
                );
            }
            */
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
