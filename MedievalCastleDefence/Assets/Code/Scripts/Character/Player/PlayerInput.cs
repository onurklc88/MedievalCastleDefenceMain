using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
public class PlayerInput : NetworkBehaviour, IReadInput
{
    public NetworkButtons PreviousButton { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    [SerializeField] private UIManager _uýManager;
 
    public override void FixedUpdateNetwork()
    {
       
     
    }
 
    public void ReadPlayerInputs(PlayerInputData input)
    {
        /*
        if (!Object.HasStateAuthority) return;

      var scoreboardInput =  input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Tab);
       
        //if(scoreboardInput == false)
        _uýManager.ShowScoreboard(scoreboardInput);
        */
    }

    private void Update()
    {
        if (_uýManager == null) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
           
            //Test();
        }
        if (Input.GetKey(KeyCode.Tab))
        {
            _uýManager.ShowScoreboard(true);
            //Test();
        }
        else 
        {
            _uýManager.ShowScoreboard(false);
        }
    }

    private void Test()
    {
        Debug.Log($"<color=yellow>Total Players on Server: {Runner.ActivePlayers.Count()}</color>");

        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            var playerNetworkObject = playerObject.transform.GetComponentInParent<NetworkObject>();
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
            Debug.Log($"<color=cyan>Player {info.PlayerNickName}: Kills {info.PlayerKillCount}, Deaths {info.PlayerDieCount}, NetworkObjectID {playerNetworkObject.Id}</color>");
        }
    }

}
