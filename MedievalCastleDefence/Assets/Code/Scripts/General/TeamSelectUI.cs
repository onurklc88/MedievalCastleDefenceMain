using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using static BehaviourRegistry;
using UnityEngine.UI;

public class TeamSelectUI : ManagerRegistry
{
    [SerializeField] private Button _redTeamButton;
    [SerializeField] private Button _blueTeamButton;
    private LevelManager _levelManager;
   
    private void OnEnable()
    {
        _levelManager = GetScript<LevelManager>();
        CheckTeamPlayerCount();
    }

 
    private void CheckTeamPlayerCount()
    {
      
        var playerList = Runner.ActivePlayers.ToList();
        var redTeamPlayerCount = 0;
        var blueTeamPlayerCount = 0;
        for (int i = 0; i < playerList.Count; i++)
        {
            var player = playerList[i];
            var playerNetworkObject = Runner.GetPlayerObject(player);
            if (playerNetworkObject != null)
            {
                if (playerNetworkObject.gameObject.GetComponentInParent<PlayerStatsController>() != null)
                {
                    var playerTeam = playerNetworkObject.gameObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerTeam;

                  
                    if (playerTeam == TeamManager.Teams.Red)
                    {
                        redTeamPlayerCount += 1;
                    }
                    else
                    {
                        blueTeamPlayerCount += 1;
                    }
                }
                else
                {
                    Debug.Log("Runner not Ready");
                }
            }
           

        }
        
       
        if (redTeamPlayerCount == _levelManager.MaxPlayerCount / 2)
        {
            _redTeamButton.interactable = false;
        }
        
        if(blueTeamPlayerCount == _levelManager.MaxPlayerCount / 2)
        {
            _blueTeamButton.interactable = false;
        }
       

    }

}
