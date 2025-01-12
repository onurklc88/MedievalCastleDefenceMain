using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System.Linq;

public class Scoreboard : NetworkBehaviour
{
    
    [SerializeField] private GameObject _playerStatsEntry;
    [SerializeField] private Transform _content;
    private List<GameObject> _entryList = new List<GameObject>();
    private void OnEnable()
    {
        GetPlayerList();
    }

    private void OnDisable()
    {
        ClearScoreboard();
    }

    private void GetPlayerList()
    {
       
        var playerList = Runner.ActivePlayers.ToList();
        for (int i = 0; i < playerList.Count; i++)
        {
            var player = playerList[i];
          var  playerNetworkObject = Runner.GetPlayerObject(player);
            GameObject entry = GameObject.Instantiate(_playerStatsEntry, _content);
            for (int j = 0; j < entry.GetComponentsInChildren<TextMeshProUGUI>().Length; j++)
            {
                switch (j)
                {
                    case 0:
                        entry.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerNetworkObject.gameObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerNickName.ToString();
                        break;
                    case 1:
                        entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = playerNetworkObject.gameObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerKillCount.ToString();
                        break;
                    case 2:
                        entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = playerNetworkObject.gameObject.GetComponentInParent<PlayerStatsController>().PlayerNetworkStats.PlayerDieCount.ToString();
                        break;
                    case 3:
                        entry.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = playerNetworkObject.gameObject.GetComponentInParent<NetworkObject>().Id.ToString();
                        break;
                }
            }
            _entryList.Add(entry);
        }
    }

    /*
    private void GetPlayerList()
    {
        Debug.Log("Test: " + _activePlayerList.PlayerList.Count);
       
        for (int i = 0; i < _activePlayerList.PlayerList.Count; i++)
        {
            if (Runner.TryGetPlayerObject(_activePlayerList.PlayerList[i], out var playerNetworkObject))
            {
               // Debug.Log("NetowrkObjectID:" + playerNetworkObject.Id);
                /*
               var playerTeam = playerNetworkObject.gameObject.GetComponent<PlayerStatsController>().PlayerLocalStats.PlayerTeam;
               var contentIndex = 0;

                if (playerTeam == PlayerStats.Team.RedPanters)
                    contentIndex = 0;
                else
                    contentIndex = 1;

                GameObject entry = GameObject.Instantiate(_playerStatsEntry, _content[contentIndex]);
                entry.gameObject.GetComponent<Image>().color = _teamColors[contentIndex];
                

                GameObject entry = GameObject.Instantiate(_playerStatsEntry, _content);

                for (int j = 0; j < entry.GetComponentsInChildren<TextMeshProUGUI>().Length; j++)
                {
                    switch (j)
                    {
                        case 0:
                            entry.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerNetworkObject.gameObject.GetComponent<PlayerStatsController>().PlayerLocalStats.PlayerNickName.ToString();
                            break;
                        case 1:
                            entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = playerNetworkObject.gameObject.GetComponent<PlayerStatsController>().PlayerLocalStats.PlayerKillCount.ToString();
                            break;
                        case 2:
                            entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = playerNetworkObject.gameObject.GetComponent<PlayerStatsController>().PlayerLocalStats.PlayerKillCount.ToString();
                            break;
                    }
                }

                _entryList.Add(entry);
            }
        }
      
    }
*/
    private void ClearScoreboard()
    {
        for (int i = 0; i < _entryList.Count; i++)
        {
            Destroy(_entryList[i].transform.gameObject);
        }
        _entryList.Clear();
    }
}
