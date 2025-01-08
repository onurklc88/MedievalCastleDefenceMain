using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class ActivePlayerList : NetworkBehaviour
{
    [Networked, Capacity(24)]
    public NetworkLinkedList<PlayerRef> PlayerList { get; } = new NetworkLinkedList<PlayerRef>();

    private void OnEnable()
    {
       // EventLibrary.OnPlayerJoiningServer.AddListener(UpdatePlayerList);
    }

    private void OnDisable()
    {
      //  EventLibrary.OnPlayerJoiningServer.RemoveListener(UpdatePlayerList);
    }

    public override void Spawned()
    {
        Debug.Log($"<color=yellow>Total Players on Server: {Runner.ActivePlayers.Count()}</color>");

        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            if (playerObject == null)
            {
                Debug.LogWarning($"<color=red>Player object not found for player {player}.</color>");
                continue;
            }

            var stats = playerObject.GetComponent<PlayerStatsController>();
            if (stats == null)
            {
                Debug.LogWarning($"<color=red>PlayerStatsController not found for player {player}.</color>");
                continue;
            }

            PlayerInfo info = stats.PlayerNetworkStats;
            Debug.Log($"<color=cyan>Player {info.PlayerNickName}: Kills {info.PlayerKillCount}, Deaths {info.PlayerDieCount}</color>");
        }
    }

   
}
