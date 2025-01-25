using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Steamworks;
public class TestPlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public Transform[] RedTeamPlayerSpawnPositions;
    public Transform[] BlueTeamPlayerSpawnPositions;
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
        //EventLibrary.OnPlayerSelectTeam.AddListener(SpawnPlayer);
    }

    private void OnDisable()
    {
        EventLibrary.OnPlayerSelectTeam.RemoveListener(SpawnPlayer);
        EventLibrary.OnRespawnRequested.RemoveListener(RespawnPlayer);
    }


    public void PlayerJoined(PlayerRef player)
    {
        

        if (player == Runner.LocalPlayer)
            {
            EventLibrary.OnRespawnRequested.AddListener(RespawnPlayer);
            EventLibrary.OnPlayerSelectTeam.AddListener(SpawnPlayer);
            /*
            var playerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, player);
               
                if (playerObject != null)
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
               
                stats.PlayerWarrior = CharacterStats.CharacterType.KnightCommander;
                Runner.SetPlayerObject(player, playerObject);
                EventLibrary.OnRespawnRequested.AddListener(RespawnPlayer);
                playerObject.transform.GetComponentInParent<PlayerStatsController>().SetPlayerInfo(stats);
               */
        }



        //}
      
          /*
        if (player == Runner.LocalPlayer)
        {
            var playerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, player);

            if (playerObject != null)
            {
                Runner.SetPlayerObject(player, playerObject);
                EventLibrary.OnRespawnRequested.AddListener(RespawnPlayer);

                //Debug.Log("Player object set successfully for player: " + player.PlayerId);
            }

        }
          */
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

                stats.PlayerWarrior = CharacterStats.CharacterType.KnightCommander;
                stats.PlayerTeam = selectedTeam;
                Runner.SetPlayerObject(playerRef, _currentPlayerObject);
                //  EventLibrary.OnRespawnRequested.AddListener(RespawnPlayer);
                _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().SetPlayerInfo(stats);
                _currentPlayerObject.transform.GetComponentInParent<PlayerHUD>().CurrentGamePhase = _levelManager.CurrentGamePhase;

            }
        }
        else
        {
            Debug.Log("Local player yok");
        }
    }
 
  public void PlayerLeft(PlayerRef player)
  {
        var isPlayerAlreadySpawned = Runner.GetPlayerObject(player);
       
        if (isPlayerAlreadySpawned)
        {
            Runner.Despawn(isPlayerAlreadySpawned);
            
        }
  }
    private void RespawnPlayer(PlayerRef playerRef, CharacterStats.CharacterType warriorType)
    {
        var isPlayerAlreadySpawned = Runner.GetPlayerObject(playerRef);
        Debug.Log("isPlayerAlreadySpawned: " +isPlayerAlreadySpawned);
        if (isPlayerAlreadySpawned)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
            {
                _oldPlayerInfo = playerNetworkObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats;
                Runner.Despawn(playerNetworkObject);
                StartCoroutine(SpawnDelay(playerRef, warriorType));
                //SpawnDelay(playerRef, warriorType);
            }
        }
    }
  /*
    private IEnumerator SpawnDelay(PlayerRef playerRef, CharacterStats.CharacterType selectedWarrirorType)
    {
        yield return new WaitForSeconds(1f);
        if (playerRef == Runner.LocalPlayer)
        {

            switch (selectedWarrirorType)
            {
                case CharacterStats.CharacterType.FootKnight:
                    _currentPlayerObject = Runner.Spawn(_stormshieldNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.KnightCommander:
                    _currentPlayerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Gallowglass:
                    _currentPlayerObject = Runner.Spawn(_gallowglassNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
                    break;
                case CharacterStats.CharacterType.Ranger:
                    _currentPlayerObject = Runner.Spawn(_theSaxonMarkNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
                    break;

            }
        
      
              _oldPlayerInfo.PlayerWarrior = selectedWarrirorType;
              Runner.SetPlayerObject(playerRef, _currentPlayerObject);
              _currentPlayerObject.transform.GetComponentInParent<PlayerStatsController>().SetPlayerInfo(_oldPlayerInfo);
        }
}
  */
  
    private IEnumerator SpawnDelay(PlayerRef playerRef, CharacterStats.CharacterType selectedWarrirorType)
    {
        yield return new WaitForSeconds(0.05f);

        if (playerRef == Runner.LocalPlayer)
        {
            
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
            _currentPlayerObject.transform.GetComponentInParent<PlayerHUD>().CurrentGamePhase = _levelManager.CurrentGamePhase;
        }
    }
  /*
    private void SpawnDelay(PlayerRef playerRef, CharacterStats.CharacterType selectedWarrirorType)
    {
       

        if (playerRef == Runner.LocalPlayer)
        {

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
        }
    }
  */
    private HashSet<Vector3> _usedRedSpawnPositions = new HashSet<Vector3>();
    private HashSet<Vector3> _usedBlueSpawnPositions = new HashSet<Vector3>();

    private Vector3 GetRandomSpawnPosition(TeamManager.Teams selectedTeam)
    {
        Transform[] spawnPositions;

       
        if (selectedTeam == TeamManager.Teams.Red)
        {
            spawnPositions = RedTeamPlayerSpawnPositions;
        }
        else
        {
            spawnPositions = BlueTeamPlayerSpawnPositions;
        }

      
        List<Vector3> availablePositions = new List<Vector3>();
        foreach (var pos in spawnPositions)
        {
            if (!_usedRedSpawnPositions.Contains(pos.position) && !_usedBlueSpawnPositions.Contains(pos.position))
            {
                availablePositions.Add(pos.position);
            }
        }

       
        if (availablePositions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availablePositions.Count);
            Vector3 selectedPosition = availablePositions[randomIndex];

           
            if (selectedTeam == TeamManager.Teams.Red)
            {
                _usedRedSpawnPositions.Add(selectedPosition);
            }
            else
            {
                _usedBlueSpawnPositions.Add(selectedPosition);
            }

            return selectedPosition;
        }

        
        return Vector3.zero;
    }




}
