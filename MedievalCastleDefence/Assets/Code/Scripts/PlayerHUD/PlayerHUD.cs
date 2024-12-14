using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class PlayerHUD : BehaviourRegistry
{
    [SerializeField] private TextMeshProUGUI _characterStaminaText;
    [SerializeField] private TextMeshProUGUI _characterHealth;
    private CharacterStamina _characterStamina;
    [SerializeField] private GameObject _respawnPanel;
    [SerializeField] private GameObject[] _arrowImage;
    [SerializeField] private GameObject _aimTarget;
    [SerializeField] private TextMeshProUGUI _stateTest;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        var type = transform.GetComponent<PlayerStatsController>().SelectedCharacter;
        InitScript(this);
        //_characterStamina = GetScript<CharacterStamina>(type);
    }
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
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

    public void OnRespawnStormshieldButtonClicked()
    {
        if (!Object.HasStateAuthority) return;
        base.OnObjectDestroy();
        //EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer);
        EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.FootKnight);

        _respawnPanel.SetActive(false);
    }
    public void OnRespawnKnightCommanderButtonClicked()
    {
        if (!Object.HasStateAuthority) return;
        base.OnObjectDestroy();
        //EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer);
        EventLibrary.OnRespawnRequested?.Invoke(Runner.LocalPlayer, CharacterStats.CharacterType.KnightCommander);

        _respawnPanel.SetActive(false);
    }
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
}
