using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TestPlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private NetworkPrefabRef _stormshieldNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _knightCommanderdNetworkPrefab = NetworkPrefabRef.Empty;
    private NetworkObject _currentPlayerObject;
    public void PlayerJoined(PlayerRef player)
    {
        
        if (player == Runner.LocalPlayer)
            {
                var playerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, player);
                //  Debug.Log("test: " + player.PlayerId);
                if (playerObject != null)
                {
                    Runner.SetPlayerObject(player, playerObject);

                EventLibrary.OnRespawnRequested.AddListener(RespawnPlayer);
               // EventLibrary.Sex.AddListener(RespawnCharacter);
                Debug.Log("Player object set successfully for player: " + player.PlayerId);
                }

            }
      
          
    }

  public void PlayerLeft(PlayerRef player)
  {
        DespawnPlayer(player);
  }
    private void DespawnPlayer(PlayerRef playerRef)
    {
        if (Runner.IsServer)
        {
            var isPlayerAlreadySpawned = Runner.GetPlayerObject(playerRef);
            if (isPlayerAlreadySpawned)
            {

                if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
                {
                    
                    Runner.Despawn(playerNetworkObject);
                }
            }
        }

    }

    private void Start()
    {
      
    }
    private void OnEnable()
    {
      

    }
    private void RespawnPlayer(PlayerRef playerRef)
    {
        var isPlayerAlreadySpawned = Runner.GetPlayerObject(playerRef);
        if (isPlayerAlreadySpawned)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
            {
                var playerStats = playerNetworkObject.transform.GetComponent<PlayerStatsController>().PlayerLocalStats.PlayerWarrior;
                Runner.Despawn(playerNetworkObject);
                //StartCoroutine(SpawnDelay(playerRef));
               
            }
        }

    }

    private void RespawnPlayer(PlayerRef playerRef, CharacterStats.CharacterType warriorType)
    {
        var isPlayerAlreadySpawned = Runner.GetPlayerObject(playerRef);
        if (isPlayerAlreadySpawned)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
            {
                Runner.Despawn(playerNetworkObject);
                StartCoroutine(SpawnDelay(playerRef, warriorType));

            }
        }
    }

    private IEnumerator SpawnDelay(PlayerRef playerRef, CharacterStats.CharacterType selectedWarrirorType)
    {
        yield return new WaitForSeconds(1f);
        if(selectedWarrirorType == CharacterStats.CharacterType.FootKnight)
        {
            _currentPlayerObject = Runner.Spawn(_stormshieldNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
        }
        else
        {
            _currentPlayerObject = Runner.Spawn(_knightCommanderdNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
        }
        PlayerStats stats = new PlayerStats();
        stats.PlayerWarrior = selectedWarrirorType;
        _currentPlayerObject.transform.GetComponentInChildren<PlayerStatsController>().SetPlayerInfo(stats);

        Runner.SetPlayerObject(playerRef, _currentPlayerObject);
    }

 

    private void OnDisable()
    {
       EventLibrary.OnRespawnRequested.RemoveListener(RespawnPlayer);
       // FootKnightAttack.Test -= RespawnPlayer;
    }

   

  
    
}
