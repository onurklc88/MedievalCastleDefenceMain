using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using static BehaviourRegistry;
public class UIManager : ManagerRegistry, IReadInput, IGameStateListener
{
    [Header("TeamSelect Panel Variables")]
    [SerializeField] private Button[] _teamSelectButton;
    [SerializeField] private GameObject _teamPanel;
    [SerializeField] private GameObject _characterPanel;
    private TeamManager.Teams _playerTeam;
    private CharacterStats.CharacterType _playerWarrior;
    [Header("Warrior Panel Variables")]
    [SerializeField] private Button[] _warriorButtons;

    [Header("Killfeed Variables")]
    [SerializeField] private Transform _killfeedContext;
    [SerializeField] private GameObject _killfeed;
    
    [SerializeField] private GameObject _scoreboard;
    private bool _previousCondition = false;
    [Header("Timer Variables")]
    [SerializeField] private TextMeshProUGUI _timer;
   
    
    public NetworkButtons PreviousButton { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public LevelManager.GamePhase CurrentGameState { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    private bool _test;

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
        InitScript(this);
        _teamSelectButton[0].onClick.AddListener(() => UpdatePlayerTeamButton(TeamManager.Teams.Red));
        _teamSelectButton[1].onClick.AddListener(() => UpdatePlayerTeamButton(TeamManager.Teams.Blue));
        _warriorButtons[0].onClick.AddListener(() => UpdatePlayerSelectedWarrior(CharacterStats.CharacterType.FootKnight));
        _warriorButtons[1].onClick.AddListener(() => UpdatePlayerSelectedWarrior(CharacterStats.CharacterType.KnightCommander));
        _warriorButtons[2].onClick.AddListener(() => UpdatePlayerSelectedWarrior(CharacterStats.CharacterType.Gallowglass));
        _warriorButtons[3].onClick.AddListener(() => UpdatePlayerSelectedWarrior(CharacterStats.CharacterType.Ranger));
       
    }

   

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_test)
        {
            _test = true;
            _teamPanel.SetActive(true);
        }
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

    public void UpdateTimer(string value)
    {
        _timer.text = value;
    }

    public void ReadPlayerInputs(PlayerInputData input)
    {
         ShowScoreboard(input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Tab));
    }


    public void UpdatePlayerTeamButton(TeamManager.Teams selectedTeam)
    {
        Debug.Log("selectedTeam: " + selectedTeam);
        _playerTeam = selectedTeam;
        _teamPanel.SetActive(false);
        _characterPanel.SetActive(true);
    }

    public void UpdatePlayerSelectedWarrior(CharacterStats.CharacterType selectedCharacter)
    {
        _playerWarrior = selectedCharacter;
        EventLibrary.OnPlayerSelectTeam.Invoke(Runner.LocalPlayer, _playerWarrior, _playerTeam);
        EventLibrary.OnPlayerSelectWarrior.Invoke();
        _characterPanel.SetActive(false);
    }

    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
       
    }

    private IEnumerator PanelDelay()
    {
       
        yield return new WaitForSeconds(5f);
        _teamPanel.SetActive(true);
    }
}
