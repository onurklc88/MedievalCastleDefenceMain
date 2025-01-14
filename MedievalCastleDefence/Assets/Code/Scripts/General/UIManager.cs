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
    [Header("Round Variables")]
    [SerializeField] private TextMeshProUGUI _gameStateText;
    [SerializeField] private TextMeshProUGUI _redTeamScore;
    [SerializeField] private TextMeshProUGUI _blueTeamScore;
    [SerializeField] private GameObject[] _teamImages;
    [SerializeField] private TextMeshProUGUI _roundIndex;
    [SerializeField] private TextMeshProUGUI _roundText;

    [Networked(OnChanged = nameof(OnGameStateTextChange))] public NetworkString<_16> GameStateTxt { get; set; }
    


    public NetworkButtons PreviousButton { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }
    private bool _test;

    private void OnEnable()
    {
        EventLibrary.OnPlayerKill.AddListener(ShowKillFeedRpc);
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
       
    }

    private void OnDisable()
    {
        EventLibrary.OnPlayerKill.RemoveListener(ShowKillFeedRpc);
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameState);
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
        CurrentGamePhase = currentGameState;
        switch (currentGameState)
        {
            case LevelManager.GamePhase.Warmup:
                if (Object.HasStateAuthority)
                    EnableTeamImagesRpc(false);
                GameStateTxt = "WARMUP";
               
                break;
            case LevelManager.GamePhase.Preparation:
                GameStateTxt = "PREPERATION";
                EnableTeamImagesRpc(true);
                break;
            case LevelManager.GamePhase.GameStart:
             

                break;
            case LevelManager.GamePhase.RoundStart:
                if (Object.HasStateAuthority)
                {
                    EnableTimerImageRpc(false);
                   
                }
                GameStateTxt = "ROUND";
                break;
            case LevelManager.GamePhase.RoundEnd:
                break;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void EnableTeamImagesRpc(bool condition)
    {
     
        _teamImages[0].SetActive(condition);
        _teamImages[1].SetActive(condition);
       
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void EnableTimerImageRpc(bool condition)
    {
        if (_timer.enabled != false)
        {
            _timer.enabled = false;
        }

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateTeamScoreRpc(TeamManager.Teams team, int killCount)
    {
        Debug.Log("VBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        if (team == TeamManager.Teams.Red)
        {
            _redTeamScore.text = killCount.ToString();
        }
        else
        {
            _blueTeamScore.text = killCount.ToString();
        }

    }

    public void UpdateRoundCounterText(string index)
    {
        _roundIndex.text = index;
    }

    private static void OnGameStateTextChange(Changed<UIManager> changed)
    {
        changed.Behaviour._gameStateText.text = changed.Behaviour.GameStateTxt.ToString();
       
    }
}
