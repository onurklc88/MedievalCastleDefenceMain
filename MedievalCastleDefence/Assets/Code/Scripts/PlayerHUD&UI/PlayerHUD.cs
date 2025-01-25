using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;
public class PlayerHUD : CharacterRegistry, IGameStateListener
{
    [Networked(OnChanged = nameof(OnNetworkNickNameChanged))] public NetworkString<_16> PlayerNickName { get; set; }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }

    [SerializeField] private GameObject _playerUI;
    [SerializeField] private TextMeshProUGUI _characterStaminaText;
    [SerializeField] private TextMeshProUGUI _characterHealth;
    private CharacterStamina _characterStamina;
    [SerializeField] private GameObject _respawnPanel;
    [SerializeField] private GameObject[] _arrowImage;
    [SerializeField] private GameObject _aimTarget;
    [SerializeField] private TextMeshProUGUI _stateTest;
    [SerializeField] private TextMeshProUGUI _nickNameText;

    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameState);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameState);
    }

    public void UpdateGameState(LevelManager.GamePhase currentGameState)
    {
      
        CurrentGamePhase = currentGameState;
        Debug.LogError("Current GamePhase:" + currentGameState);
    }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        var type = transform.GetComponent<PlayerStatsController>().SelectedCharacter;
        InitScript(this);
        _playerUI.SetActive(true);
        //_characterStamina = GetScript<CharacterStamina>(type);
    }
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        //_nickNameText.maxVisibleCharacters = 3;
        var type = transform.GetComponent<PlayerStatsController>().SelectedCharacter;
        _characterStamina = GetScript<CharacterStamina>();
    }

  
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if(_characterStamina != null)
            _characterStaminaText.text = "Character Stamina: " +_characterStamina.CurrentStamina.ToString();

        //_stateTest.text = Object.HasStateAuthority.ToString();
    }

    public void UpdatePlayerHealthUI(float updatedHealth)
    {
        if (!Object.HasStateAuthority) return;
        _characterHealth.text = "PlayerHealth: " +updatedHealth.ToString();
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

       

        //base.OnObjectDestroy();


    }

    public void OnRespawnStormshieldButtonClicked()
    {
        if (!Object.HasStateAuthority) return;
        base.OnObjectDestroy();
        //EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer);
        EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.FootKnight);

        _respawnPanel.SetActive(false);
    }
    /*
    public void OnRespawnKnightCommanderButtonClicked()
    {
        if (!Object.HasStateAuthority) return;
        base.OnObjectDestroy();
        //EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer);
        EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.KnightCommander);

        _respawnPanel.SetActive(false);
    }
    */
    public void OnRespawnGallowButtonClicked()
    {
        if (!Object.HasStateAuthority) return;
        base.OnObjectDestroy();
        //EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer);
        EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.Gallowglass);
        _respawnPanel.SetActive(false);
    }
    public void OnRespawnRangerButtonClicked()
    {
        if (!Object.HasStateAuthority) return;
        base.OnObjectDestroy();
        //EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer);
        EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.Ranger);
        _respawnPanel.SetActive(false);
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
        EventLibrary.DebugMessage.Invoke(nickname);
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

   
}
