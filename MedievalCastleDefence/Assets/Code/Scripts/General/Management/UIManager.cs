using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private TextMeshProUGUI _winnerTeamText;
    private bool _phaseProcessed = false;
    [Networked(OnChanged = nameof(OnGameStateTextChange))] public NetworkString<_16> GameStateTxt { get; set; }
    


    public NetworkButtons PreviousButton { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }
    private bool _test;
    private LevelManager _levelManager;

    private void OnEnable()
    {
        EventLibrary.OnPlayerKill.AddListener(ShowKillFeedRpc);
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
        EventLibrary.OnPlayerRespawn.AddListener(UpdatePlayerTeamButton);
        EventLibrary.OnLevelFinish.AddListener(ShowWinnerTeamRpc);
        EventLibrary.OnPlayerTeamSwitchRequested.AddListener(ShowTeamSelectionPanel);
       
    }

    private void OnDisable()
    {
        EventLibrary.OnPlayerKill.RemoveListener(ShowKillFeedRpc);
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameState);
        EventLibrary.OnPlayerRespawn.RemoveListener(UpdatePlayerTeamButton);
        EventLibrary.OnLevelFinish.RemoveListener(ShowWinnerTeamRpc);
        EventLibrary.OnPlayerTeamSwitchRequested.RemoveListener(ShowTeamSelectionPanel);
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

    private void Start()
    {
        _levelManager = GetScript<LevelManager>();
    }

   
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_test && CurrentGamePhase == LevelManager.GamePhase.Warmup)
        {
            _test = true;
            _teamPanel.SetActive(true);
        }
        
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void ShowKillFeedRpc(CharacterStats.CharacterType warriorType, string killerPlayer, string deadPlayer)
    {
        Debug.Log("Warrior: " + warriorType + " Killer: " + killerPlayer + " DeadPlayer: " + deadPlayer);
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
        if (CurrentGamePhase == LevelManager.GamePhase.Warmup || CurrentGamePhase == LevelManager.GamePhase.Preparation) return;
         ShowScoreboard(input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Tab));
    }


    public void UpdatePlayerTeamButton(TeamManager.Teams selectedTeam)
    {
        if(Cursor.visible == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
      
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

    public async void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
       
        if (_phaseProcessed) return;
      
        
        switch (currentGameState)
        {
            case LevelManager.GamePhase.Warmup:
                if (!Runner.IsSharedModeMasterClient) return;
                EnableTeamImagesRpc(false);
                GameStateTxt = "WARMUP";
               
                break;
            case LevelManager.GamePhase.Preparation:
                if (!Runner.IsSharedModeMasterClient) return;
                GameStateTxt = "PREPERATION";
                EnableTeamImagesRpc(true);
                break;
            case LevelManager.GamePhase.RoundStart:
                HideSelectionPanel();
                if (!Runner.IsSharedModeMasterClient) return;
                EnableTimerImageRpc(false);
                await UniTask.Delay(1000);
                GameStateTxt = "ROUND";
                _phaseProcessed = true;
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
        
        if (team == TeamManager.Teams.Red)
        {
            _redTeamScore.text = killCount.ToString();
        }
        else
        {
            _blueTeamScore.text = killCount.ToString();
        }

    }

    
    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void ShowWinnerTeamRpc(TeamManager.Teams winnerTeam)
    {
       
        if(winnerTeam == TeamManager.Teams.Red)
        {
            _winnerTeamText.color = Color.red;
        }
        else
        {
            _winnerTeamText.color = Color.blue;
        }


        _winnerTeamText.text = winnerTeam.ToString() + " team wins ";
        await UniTask.Delay(2000);
        _winnerTeamText.text = " ";
    }

    private void ShowTeamSelectionPanel()
    {
        if (CurrentGamePhase != LevelManager.GamePhase.Warmup) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _teamPanel.SetActive(true);
    }
    
    private void HideSelectionPanel()
    {
        _teamPanel.SetActive(false);
        _characterPanel.SetActive(false);
    }
    public void UpdateRoundCounterText(string index)
    {
        _roundIndex.text = index;
    }

    private static void OnGameStateTextChange(Changed<UIManager> changed)
    {
        changed.Behaviour._gameStateText.text = changed.Behaviour.GameStateTxt.ToString();
       
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ShowEndingLeaderboardRpc()
    {
        _scoreboard.SetActive(true);
    }
}
