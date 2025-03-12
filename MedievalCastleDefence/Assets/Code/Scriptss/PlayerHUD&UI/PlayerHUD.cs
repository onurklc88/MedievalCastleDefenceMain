using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;
public class PlayerHUD : CharacterRegistry, IRPCListener, IReadInput
{
    [Networked(OnChanged = nameof(OnNetworkNickNameChanged))] public NetworkString<_16> PlayerNickName { get; set; }
    [Networked(OnChanged = nameof(OnNetworkedHealthChange))] public float NetworkedHealth { get; set; }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }
    public NetworkButtons PreviousButton { get; set; }

    [SerializeField] private GameObject _playerUI;
    [SerializeField] private TextMeshProUGUI _characterStaminaText;
    [SerializeField] private TextMeshProUGUI _characterHealth;
    [SerializeField] private TextMeshProUGUI _chargeTxt;
    [SerializeField] private GameObject _respawnPanel;
    [SerializeField] private GameObject[] _arrowImage;
    [SerializeField] private GameObject _aimTarget;
    [SerializeField] private TextMeshProUGUI _stateTest;
    [SerializeField] private TextMeshProUGUI _nickNameText;
    [SerializeField] private GameObject _playerNetworkHealthBar;
    [SerializeField] private Image _healthBarFiller;
    private CharacterStamina _characterStamina;
  
    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameStateRpc);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameStateRpc);
    }

   
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {
       CurrentGamePhase = currentGameState;
     
    }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
      
        InitScript(this);
        _playerUI.SetActive(true);
      //  _playerNetworkHealthBar.SetActive(false);


    }
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        //_nickNameText.maxVisibleCharacters = 3;
        var type = transform.GetComponent<PlayerStatsController>().SelectedCharacter;
        _characterStamina = GetScript<CharacterStamina>();
        Debug.Log("HEalth: " + base._characterStats.TotalHealth);
        _healthBarFiller.fillAmount = base._characterStats.TotalHealth;
    }

  
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if(_characterStamina != null)
            _characterStaminaText.text = "Character Stamina: " +_characterStamina.CurrentStamina.ToString();
        if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
        {
            ReadPlayerInputs(input);
        }
        //_stateTest.text = Object.HasStateAuthority.ToString();
    }

    public void UpdatePlayerHealthUI(float updatedHealth)
    {
        if (!Object.HasStateAuthority) return;
        _characterHealth.text = "PlayerHealth: " +updatedHealth.ToString();
        NetworkedHealth = updatedHealth;
    }

    public void ShowRespawnPanel()
    {
        if (!Object.HasStateAuthority) return;
        _respawnPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
    }

    public void HandleArrowImages(CharacterAttackBehaviour.SwordPosition currentPos)
    {
        if (!Object.HasStateAuthority) return;
        if(currentPos == CharacterAttackBehaviour.SwordPosition.Right)
        {
            _arrowImage[0].SetActive(true);
            _arrowImage[1].SetActive(false);
        }
        else
        {
            _arrowImage[1].SetActive(true);
            _arrowImage[0].SetActive(false);
        }
    }
    public async void OnRespawnKnightCommanderButtonClicked()
    {
        if (!Object.HasStateAuthority) return;

        _respawnPanel.SetActive(false);

        if (CurrentGamePhase == LevelManager.GamePhase.Warmup)
        {
            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.KnightCommander);
        }
        else
        {
            await UniTask.WaitUntil(() => CurrentGamePhase == LevelManager.GamePhase.Preparation);

            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.KnightCommander);
        }
    }

    public async void OnRespawnStormshieldButtonClicked()
    {
        if (!Object.HasStateAuthority) return;

        _respawnPanel.SetActive(false);

        if (CurrentGamePhase == LevelManager.GamePhase.Warmup)
        {
            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.FootKnight);
        }
        else
        {
            await UniTask.WaitUntil(() => CurrentGamePhase == LevelManager.GamePhase.Preparation);

            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.FootKnight);
        }
    }

    public async void OnRespawnGallowButtonClicked()
    {
        if (!Object.HasStateAuthority) return;

        _respawnPanel.SetActive(false);

        if (CurrentGamePhase == LevelManager.GamePhase.Warmup)
        {
            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.Gallowglass);
           
        }
        else
        {
            await UniTask.WaitUntil(() => CurrentGamePhase == LevelManager.GamePhase.Preparation);

            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.Gallowglass);
        }
    }
    public async void OnRespawnRangerButtonClicked()
    {
        if (!Object.HasStateAuthority) return;

        _respawnPanel.SetActive(false);

        if (CurrentGamePhase == LevelManager.GamePhase.Warmup)
        {
            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.Gallowglass);
        }
        else
        {
            await UniTask.WaitUntil(() => CurrentGamePhase == LevelManager.GamePhase.Preparation);

            EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.Gallowglass);
        }
    }

    public void UpdateAimTargetState(bool condition)
    {
        if (!Object.HasStateAuthority) return;
        
        _aimTarget.SetActive(condition);
    }

    public void UpdatePlayerNickname(string nickname)
    {
        if (!Object.HasStateAuthority) return;
        PlayerNickName = nickname;
        _nickNameText.enabled = false;
       // EventLibrary.DebugMessage.Invoke(nickname);
    }

    public static void OnNetworkNickNameChanged(Changed<PlayerHUD> changed)
    {
        if (changed.Behaviour._nickNameText == null) return;
        string nickName = changed.Behaviour.PlayerNickName.ToString();
        changed.Behaviour._nickNameText.text = nickName.Length > 10
            ? nickName.Substring(0, 10) + "..."
            : nickName;


    }
    public void ShowNickName(bool condition)
    {
        _nickNameText.enabled = condition;
    }

    private static void OnNetworkedHealthChange(Changed<PlayerHUD> changed)
    {
        changed.Behaviour._healthBarFiller.fillAmount = changed.Behaviour.NetworkedHealth / changed.Behaviour._characterStats.TotalHealth;
        if(changed.Behaviour.NetworkedHealth <= 0)
        {
            changed.Behaviour.PlayerNickName = " ";
            changed.Behaviour._playerNetworkHealthBar.SetActive(false);
        }
    }

    public void ReadPlayerInputs(PlayerInputData input)
    {
        var pressedButton = input.NetworkButtons.GetPressed(PreviousButton);

        if (pressedButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.SwitchTeamKey))
        {
            EventLibrary.OnPlayerTeamSwitchRequested.Invoke();
        }

        PreviousButton = input.NetworkButtons;
    }

    public void UpdateSlideChargeCount(int count)
    {
        _chargeTxt.text = count.ToString();
    }
}

