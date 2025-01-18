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
   public GamePhase CurrentGamePhase { get; set; }
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
    public int RedTeamPlayerCount { get; private set; }
    public int BlueTeamPlayerCount { get; private set; }
    public int MaxPlayerCount;
    public int TeamsPlayerCount;
    public GameManager.GameModes GameMode;
    public int TotalLevelRound;
    private UIManager _uiManager;
    [SerializeField] private Transform[] _redTeamPlayerSpawnPositions;
    [SerializeField] private Transform[] _blueTeamPlayerSpawnPositions;
    

    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameState);
    }
    public override void Spawned()
    {
      EventLibrary.OnPlayerSelectWarrior.AddListener(UpdatePlayerCountRpc);
       CurrentGamePhase = GamePhase.Warmup;
       EventLibrary.OnGamePhaseChange.Invoke(CurrentGamePhase);
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
        //Debug.Log("CurrentPlayerCount: " + CurrentPlayerCount);
        if(CurrentPlayerCount == MaxPlayerCount)
        {
            await UniTask.Delay(2000);
            CurrentGamePhase = GamePhase.Preparation;
            EventLibrary.OnGamePhaseChange.Invoke(CurrentGamePhase);
        }
    }

    

    public async void UpdateGameState(GamePhase currentGameState)
    {
        //    CurrentGamePhase = currentGameState;
        switch (currentGameState)
        {
            case GamePhase.GameStart:
                break;
            case GamePhase.Warmup:
               
                break;
            case GamePhase.Preparation:
                await UniTask.Delay(3000);
                if (!Runner.IsSharedModeMasterClient) return;
                ForcePlayersSpawnRpc();
                await UniTask.Delay(2000);
                DisablePlayerInputsRpc(false);
                break;
            case GamePhase.RoundStart:
                if (!Runner.IsSharedModeMasterClient) return;
                TeleportPlayersToStartPositionsRpc();
                await UniTask.Delay(2000);
                if (!Runner.IsSharedModeMasterClient) return;
                DisablePlayerInputsRpc(true);

                break;
            case GamePhase.RoundEnd:
                
                break;
            case GamePhase.GameEnd:

                break;
        } 
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void ForcePlayersSpawnRpc()
    {

        int nullPlayerObjectCount = 0;
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);

            if (playerObject != null)
            {
               
                var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();
               
                if (characterHealth != null && characterHealth.NetworkedHealth <= 0)
                {
                    nullPlayerObjectCount++;
                    characterHealth.IsPlayerDead = false;
                    var playerWarriorType = playerObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerWarrior;
                    EventLibrary.OnRespawnRequested.Invoke(player, playerWarriorType);
                }
            }
            else
            {
               
                UpdateTeamPlayerCounts();

                TeamManager.Teams availableTeam = DetermineAvailableTeam();

                
                EventLibrary.OnPlayerSelectTeam.Invoke(
                    player,
                    CharacterStats.CharacterType.KnightCommander,
                    availableTeam
                );
            }
           
        }
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
        Vector3[] shuffledRedPositions = _redTeamPlayerSpawnPositions
         .Select(t => t.position)
         .OrderBy(x => Random.value)
         .ToArray();

        Vector3[] shuffledBluePositions = _blueTeamPlayerSpawnPositions
            .Select(t => t.position)
            .OrderBy(x => Random.value)
            .ToArray();

       
        HashSet<Vector3> usedRedPositions = new HashSet<Vector3>();
        HashSet<Vector3> usedBluePositions = new HashSet<Vector3>();

        int redIndex = 0;
        int blueIndex = 0;

        foreach (var playerNetworkObject in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(playerNetworkObject);
            var playerStats = playerObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats;


            if (playerStats.PlayerTeam == TeamManager.Teams.Red)
            {
                
                while (redIndex < shuffledRedPositions.Length && usedRedPositions.Contains(shuffledRedPositions[redIndex]))
                {
                    redIndex++;
                }

                if (redIndex < shuffledRedPositions.Length)
                {
                    playerObject.transform.position = shuffledRedPositions[redIndex];
                    usedRedPositions.Add(shuffledRedPositions[redIndex]);
                    redIndex++;
                }
            }
            else if (playerStats.PlayerTeam == TeamManager.Teams.Blue)
            {
               
                while (blueIndex < shuffledBluePositions.Length && usedBluePositions.Contains(shuffledBluePositions[blueIndex]))
                {
                    blueIndex++;
                }

                if (blueIndex < shuffledBluePositions.Length)
                {
                    playerObject.transform.position = shuffledBluePositions[blueIndex];
                    usedBluePositions.Add(shuffledBluePositions[blueIndex]); 
                    blueIndex++;
                }
            }
        }
    }

    public void UpdateTeamPlayerCounts()
    {
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
                        RedTeamPlayerCount++;
                    else if (playerTeam == TeamManager.Teams.Blue)
                        BlueTeamPlayerCount++;
                }
            }
        }
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
