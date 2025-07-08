using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using Steamworks;
using System.Linq;
using Fusion.Sockets;

public class TestPlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft, IGameStateListener, INetworkRunnerCallbacks
{
    public NetworkRunner NetworkRunner;
    public LevelManager.GamePhase CurrentGamePhase { get; set; }
    public Transform[] RedTeamPlayerSpawnPositions;
    public Transform[] BlueTeamPlayerSpawnPositions;
    public static HashSet<Vector3> UsedSpawnPositions = new HashSet<Vector3>();
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private NetworkPrefabRef _stormshieldNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _knightCommanderdNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _gallowglassNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _theSaxonMarkNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private LevelManager _levelManager;
    private NetworkObject _currentPlayerObject;
    private PlayerInfo _oldPlayerInfo;
  

    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameState);
        EventLibrary.OnPlayerSelectTeam.RemoveListener(SpawnPlayer);
        EventLibrary.OnRespawnRequested.RemoveListener(RespawnPlayer);
    }


    public void PlayerJoined(PlayerRef player)
    {


        if (player == Runner.LocalPlayer)
        {
            EventLibrary.OnRespawnRequested.AddListener(RespawnPlayer);
            EventLibrary.OnPlayerSelectTeam.AddListener(SpawnPlayer);
        }
    }
   
   
    public void SpawnPlayer(PlayerRef playerRef, CharacterStats.CharacterType warriorType, TeamManager.Teams selectedTeam)
    {
       
        if (Runner == null)
        {
            Debug.Log("Runner yok");
            return;
        }
        
        Vector3 spawnPosition = GetRandomSpawnPosition(selectedTeam);
        if (playerRef == Runner.LocalPlayer)
        {
            var isPlayerAlreadySpawned = Runner.GetPlayerObject(playerRef);
            
            if (isPlayerAlreadySpawned)
            {
                if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
                {
                    _oldPlayerInfo = playerNetworkObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats;
                    Runner.Despawn(playerNetworkObject);
                    SwitchTeamDelay(playerRef, warriorType, selectedTeam);
                    return;
                }
            }


            switch (warriorType)
            {
                case CharacterStats.CharacterType.FootKnight:
                    _currentPlayerObject = Runner.Spawn(_stormshieldNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    _currentPlayerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    _currentPlayerObject = Runner.Spawn(_gallowglassNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Ranger:
                    _currentPlayerObject = Runner.Spawn(_theSaxonMarkNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;

            }

            if (_currentPlayerObject != null)
            {
                PlayerInfo stats = new PlayerInfo();

                if (SteamManager.Initialized)
                {
                    string playerName = SteamFriends.GetPersonaName();
                    stats.PlayerNickName = playerName;
                }
                else
                {
                    Debug.LogError("Steam baþlatýlamadý!");
                }

                stats.PlayerWarrior = warriorType;
             
                stats.PlayerTeam = selectedTeam;
                Runner.SetPlayerObject(playerRef, _currentPlayerObject);
                _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().SetPlayerInfo(stats);
               _currentPlayerObject.transform.GetComponentInParent<PlayerHUD>().CurrentGamePhase = _levelManager.CurrentGamePhase;
               _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().UpdateGameStateRpc(CurrentGamePhase);
               Debug.Log("SPAWNER ___________PlayerName: " + stats.PlayerNickName.ToString() + " PlayerWarrior: " + stats.PlayerWarrior);

                if (_currentPlayerObject != null)
                {
                  
                    _levelManager.NotifySpawnCompletedRpc(playerRef);
                }
                else
                {
                    Debug.LogError("Failed to spawn player: " + playerRef.PlayerId);
                }

                _levelManager.UpdatePlayerCountRpc(stats.PlayerTeam, true);
            }
        }
        else
        {
            //Debug.Log("Local player yok");
        }

       
    }
 
   public void PlayerLeft(PlayerRef player)
   {

        if (Runner.IsSharedModeMasterClient)
        {
            _levelManager.CheckPlayerCountRpc();
        }
        
    }

   
    public void RespawnPlayer(PlayerRef playerRef, CharacterStats.CharacterType warriorType)
    {
        //await UniTask.WaitUntil(() => CurrentGamePhase == LevelManager.GamePhase.RoundStart);
        if (playerRef != Runner.LocalPlayer) return;
            var isPlayerAlreadySpawned = Runner.GetPlayerObject(playerRef);
            Debug.Log("isPlayerAlreadySpawned: " + isPlayerAlreadySpawned);
            if (isPlayerAlreadySpawned)
            {
                if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
                {
                    _oldPlayerInfo = playerNetworkObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats;
                    Runner.Despawn(playerNetworkObject);

                     SpawnDelay(playerRef, warriorType);
                  //  SpawnDelay(playerRef, warriorType);
                }
            }
         
       
    }
  
    private async void SpawnDelay(PlayerRef playerRef, CharacterStats.CharacterType selectedWarrirorType)
    {
        //yield return new WaitForSeconds(0.05f);
        Debug.LogError("EventGeldi: " + CurrentGamePhase);
        if (CurrentGamePhase != LevelManager.GamePhase.Warmup)
        {
            await UniTask.WaitUntil(() => CurrentGamePhase == LevelManager.GamePhase.RoundStart);
        }
        else
        {
            await UniTask.Delay(3000);
        }
        
        if (playerRef == Runner.LocalPlayer)
        {
            Debug.LogError("POS BELÝRLENMESÝ ÝÇÝN GÝRDÝ");
            Vector3 spawnPosition = GetRandomSpawnPosition(_oldPlayerInfo.PlayerTeam);
            switch (selectedWarrirorType)
            {
                case CharacterStats.CharacterType.FootKnight:
                    _currentPlayerObject = Runner.Spawn(_stormshieldNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    _currentPlayerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    _currentPlayerObject = Runner.Spawn(_gallowglassNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Ranger:
                    _currentPlayerObject = Runner.Spawn(_theSaxonMarkNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;

            }
        
      
              _oldPlayerInfo.PlayerWarrior = selectedWarrirorType;
              Runner.SetPlayerObject(playerRef, _currentPlayerObject);
              _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().SetPlayerInfo(_oldPlayerInfo);
                Debug.LogError("SpawnerPhase: " + CurrentGamePhase);
            _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().UpdateGameStateRpc(CurrentGamePhase);
            if (CurrentGamePhase != LevelManager.GamePhase.Warmup)
            {
                _currentPlayerObject.transform.GetComponentInParent<CharacterController>().enabled = false;
            }
        }
    }


    private async void SwitchTeamDelay(PlayerRef playerRef, CharacterStats.CharacterType selectedWarrirorType, TeamManager.Teams selectedTeam)
    {
        if (CurrentGamePhase != LevelManager.GamePhase.Warmup)
        {
            await UniTask.WaitUntil(() => CurrentGamePhase == LevelManager.GamePhase.RoundStart);
        }
        else
        {
            await UniTask.Delay(1000);
        }

        if (playerRef == Runner.LocalPlayer)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(selectedTeam);
            switch (selectedWarrirorType)
            {
                case CharacterStats.CharacterType.FootKnight:
                    _currentPlayerObject = Runner.Spawn(_stormshieldNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    _currentPlayerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    _currentPlayerObject = Runner.Spawn(_gallowglassNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Ranger:
                    _currentPlayerObject = Runner.Spawn(_theSaxonMarkNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);
                    break;

            }


            _oldPlayerInfo.PlayerWarrior = selectedWarrirorType;
            _oldPlayerInfo.PlayerTeam = selectedTeam;
            Runner.SetPlayerObject(playerRef, _currentPlayerObject);
            _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().SetPlayerInfo(_oldPlayerInfo);

            _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().UpdateGameStateRpc(CurrentGamePhase);
            if (CurrentGamePhase != LevelManager.GamePhase.Warmup)
            {
                _currentPlayerObject.transform.GetComponentInParent<CharacterController>().enabled = false;
            }
        }
    }
  
 
    public Vector3 GetRandomSpawnPosition(TeamManager.Teams selectedTeam)
    {
        Transform[] spawnPositions = selectedTeam == TeamManager.Teams.Red
       ? RedTeamPlayerSpawnPositions
       : BlueTeamPlayerSpawnPositions;

        if (spawnPositions.Length == 0) return Vector3.zero;

        return spawnPositions[Random.Range(0, spawnPositions.Length)].position;
    }


    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
      CurrentGamePhase = currentGameState;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
      
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
       
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
       
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
       
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
      
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
       
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
       
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
      
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
       
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data)
    {
       
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
       
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
}
