using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
public class UIManager : NetworkBehaviour, IReadInput
{
    [Header("TeamSelect Panel Variables")]
    [SerializeField] private Button[] _teamSelectButton;
    [SerializeField] private GameObject _teamPanel;
    [SerializeField] private GameObject _characterPanel;
    private TeamManager.Teams _playerTeam;
    private CharacterStats.CharacterType _playerWarrior;


    [Header("Killfeed Variables")]
    [SerializeField] private Transform _killfeedContext;
    [SerializeField] private GameObject _killfeed;
    
    [SerializeField] private GameObject _scoreboard;
    private bool _previousCondition = false;

    public NetworkButtons PreviousButton { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void OnEnable()
    {
        EventLibrary.OnPlayerKill.AddListener(ShowKillFeedRpc);
    }

    private void OnDisable()
    {
        EventLibrary.OnPlayerKill.RemoveListener(ShowKillFeedRpc);
    }

    private void Awake()
    {
        _teamSelectButton[0].onClick.AddListener(() => UpdatePlayerTeamButton(TeamManager.Teams.Red));
        _teamSelectButton[1].onClick.AddListener(() => UpdatePlayerTeamButton(TeamManager.Teams.Blue));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void ShowKillFeedRpc(CharacterStats.CharacterType warriorType, string killerPlayer, string deadPlayer)
    {
        var killFeed = GameObject.Instantiate(_killfeed, _killfeedContext);
        TextMeshProUGUI[] playerNames = killFeed.GetComponentsInChildren<TextMeshProUGUI>();
        playerNames[0].text = killerPlayer;
        playerNames[1].text = deadPlayer;
        StartCoroutine(HideKillfeed(killFeed));
    }
    private IEnumerator HideKillfeed(GameObject context)
    {
        yield return new WaitForSeconds(3f);
        Destroy(context);
    }

    public void ShowScoreboard(bool condition)
    {
        if (condition != _previousCondition)
        {
            _previousCondition = condition;
            _scoreboard.SetActive(condition); 
        }
    }

    public void ReadPlayerInputs(PlayerInputData input)
    {
         ShowScoreboard(input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Tab));
    }


    public void UpdatePlayerTeamButton(TeamManager.Teams selectedTeam)
    {
        Debug.Log("selectedTeam: " + selectedTeam);
        _playerTeam = selectedTeam;
    }

    public void UpdatePlayerSelectedWarrior(CharacterStats.CharacterType selectedCharacter)
    {
        _playerWarrior = selectedCharacter;
    }

    
}
