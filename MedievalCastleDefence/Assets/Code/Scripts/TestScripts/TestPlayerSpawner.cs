using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TestPlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private NetworkPrefabRef _playerNetworkPrefab = NetworkPrefabRef.Empty;

    public void PlayerJoined(PlayerRef player)
    {
        
        if (player == Runner.LocalPlayer)
            {
                var playerObject = Runner.Spawn(_playerNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, player);
                //  Debug.Log("test: " + player.PlayerId);
                if (playerObject != null)
                {
                    Runner.SetPlayerObject(player, playerObject);

                EventLibrary.OnRespawnRequested.AddListener(RespawnPlayer);
                Debug.Log("Player object set successfully for player: " + player.PlayerId);
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
                var playerStats = playerNetworkObject.transform.GetComponent<PlayerStatsController>().PlayerStats.PlayerWarrior;
                Runner.Despawn(playerNetworkObject);
                StartCoroutine(SpawnDelay(playerRef));
               
            }
        }

    }

    private IEnumerator SpawnDelay(PlayerRef playerRef)
    {
        yield return new WaitForSeconds(1f);
        var playerObject = Runner.Spawn(_playerNetworkPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
        Runner.SetPlayerObject(playerRef, playerObject);
    }

 

    private void OnDisable()
    {
       EventLibrary.OnRespawnRequested.RemoveListener(RespawnPlayer);
       // FootKnightAttack.Test -= RespawnPlayer;
    }

   

    public override void FixedUpdateNetwork()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            RespawnPlayer(Runner.LocalPlayer);
        }
    }
    /*
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_RespawnPlayer(PlayerRef playerRef)
    {
        var isPlayerAlreadySpawned = Runner.GetPlayerObject(playerRef);
        Debug.Log("A");
        if (isPlayerAlreadySpawned)
        {
            Debug.Log("B");

            if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
            {
                var playerStats = playerNetworkObject.transform.GetComponent<PlayerStatsController>().PlayerStats.PlayerWarrior;
                Runner.Despawn(playerNetworkObject);
                Debug.Log("C");
                var playerObject = Runner.Spawn(_playerPrefab, new Vector3(0, 0, 0), Quaternion.identity, playerRef);
                Runner.SetPlayerObject(playerRef, playerObject);
            }

        }
    }

    private void RespawnPlayerTest()
    {
        
            Runner.Spawn(_playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            //Runner.SetPlayerObject(Runner.LocalPlayer, playerObject);
       
    }
    */
}
