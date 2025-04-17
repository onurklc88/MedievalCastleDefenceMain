using static BehaviourRegistry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TeamManager : ManagerRegistry
{
    public enum Teams
    {
        None,
        Red,
        Blue
    }
   
    private PlayerStatsController _playerStatsController;


    public override void Spawned()
    {
        InitScript(this);
    }
   
    public List<PlayerRef> GetAliveTeamPlayers()
    {

        List<PlayerRef> team = new List<PlayerRef>();
        

        Debug.Log("ARAMA BAÞLADI");
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            var playerStats = playerObject.GetComponentInParent<PlayerStatsController>();
            var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();

            if (playerStats != null && playerStats.PlayerNetworkStats.PlayerTeam == Teams.Red && characterHealth.NetworkedHealth > 0)
            {
                team.Add(player);
                Debug.Log("Takým arkadaþlarý bulundu");
            }
        }

        return team;
    }

    public List<PlayerRef> GetBlueTeamPlayers()
    {

        List<PlayerRef> team = new List<PlayerRef>();


        Debug.Log("ARAMA BAÞLADI");
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            var playerStats = playerObject.GetComponentInParent<PlayerStatsController>();
            var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();

            if (playerStats != null && playerStats.PlayerNetworkStats.PlayerTeam == Teams.Blue && characterHealth.NetworkedHealth > 0)
            {
                team.Add(player);
                Debug.Log("Takým arkadaþlarý bulundu");
            }
        }

        return team;
    }

}
