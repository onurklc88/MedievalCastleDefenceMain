using UnityEngine.Rendering.Universal;
using UnityEngine;
using Fusion;
using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using static BehaviourRegistry;

public class CharacterCameraController : CharacterRegistry, IReadInput
{
    [SerializeField] private Transform _orientation;
    [SerializeField] private Transform _cameraTargetPoint;
    [SerializeField] private Transform _cameraLookAtTarget;
    [SerializeField] private Camera _uiCamera;
    private CinemachineFreeLook _cinemachineCamera;
    private Camera _playerCamera;
    private List<PlayerRef> _teamPlayers = new List<PlayerRef>();
    private PlayerStatsController _playerStatController;
    [SerializeField] private RectTransform targetUIElement;
    private int _currentFollowingPlayerIndex = 0;

    public NetworkButtons PreviousButton { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cinemachineCamera = FindObjectOfType<CinemachineFreeLook>();
            _playerCamera = Camera.main;
            _uiCamera = GameObject.Find("PlayerUICamera").transform.GetComponent<Camera>();
            var cameraData = _playerCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(_uiCamera);
            _uiCamera.enabled = true;
            _cinemachineCamera.Follow = _cameraTargetPoint;
            _cinemachineCamera.LookAt = _cameraLookAtTarget;
            InitScript(this);
           // Test();
        }
    }

    private void Start()
    {
        _playerStatController = GetScript<PlayerStatsController>();
    }

 
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            HandleCameraRotation();
            UpdateCursorPosition();
            if (Runner.TryGetInputForPlayer<PlayerInputData>(Runner.LocalPlayer, out var input))
            {
                ReadPlayerInputs(input);
            }
        }
    }
    public void ReadPlayerInputs(PlayerInputData input)
    {
        if (!Object.HasStateAuthority) return;
        var switchButton = input.NetworkButtons.GetPressed(PreviousButton);

        if (switchButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.RightArrow))
        {
            _currentFollowingPlayerIndex++;
            if (_currentFollowingPlayerIndex >= _teamPlayers.Count)
            {
                _currentFollowingPlayerIndex = 0;
            }
            ChangeFollowingPlayer();
        }
        else if(switchButton.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.LeftArrow))
        {
            _currentFollowingPlayerIndex--;
            if (_currentFollowingPlayerIndex < 0)
            {
                _currentFollowingPlayerIndex = _teamPlayers.Count - 1;
            }
            ChangeFollowingPlayer();
        }
    }

    public void UpdateCameraFollow()
    {
        if (!Object.HasStateAuthority) return;

        //_teamPlayers.Clear();

        Debug.Log("ARAMA BAÞLADI");
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            var playerStats = playerObject.GetComponentInParent<PlayerStatsController>();
            var characterHealth = playerObject.GetComponentInParent<CharacterHealth>();

            if (playerStats != null && playerStats.PlayerNetworkStats.PlayerTeam == _playerStatController.PlayerLocalStats.PlayerTeam && characterHealth.NetworkedHealth > 0)
            {
                _teamPlayers.Add(player);
                Debug.Log("Takým arkadaþlarý bulundu");
            }
        }
        ChangeFollowingPlayer();
    }


    private async void ChangeFollowingPlayer()
    {
        await UniTask.Delay(3000);
        Debug.Log("ARAMA BAÞLADI");
        var playerObject = Runner.GetPlayerObject(_teamPlayers[_currentFollowingPlayerIndex]);
        var transforms = playerObject.GetComponentsInChildren<Transform>(true);

        var  headTransform = transforms.FirstOrDefault(t => t.name == "Head");
        if (headTransform != null)
        {
            _cinemachineCamera.Follow = headTransform;
            _cinemachineCamera.LookAt = headTransform;
        }
        else
        {
            Debug.Log("Bulunamadý");
        }
       
    }

    private void Test()
    {
        var transforms = transform.GetComponentsInChildren<Transform>(true);

        var headTransform = transforms.FirstOrDefault(t => t.name == "Head");
        if (headTransform != null)
        {
            Debug.Log($"Head bulundu: {headTransform.name}", headTransform.gameObject);
            _cinemachineCamera.Follow = headTransform;
            _cinemachineCamera.LookAt = headTransform;
        }
        else
        {
            Debug.Log("Bulunamadý");
        }
    }
  
    private void UpdateCursorPosition()
    {
        if (!Object.HasStateAuthority) return;
        if (_playerCamera == null || targetUIElement == null) return;
        Vector2 mousePosition = Input.mousePosition;
        targetUIElement.position = mousePosition;
    }
    private void HandleCameraRotation()
    {
        if (_cinemachineCamera == null) return;
        Vector3 dirToCombatLookAt = _cameraTargetPoint.position - new Vector3(_cinemachineCamera.transform.position.x, _cameraTargetPoint.transform.position.y, _cinemachineCamera.transform.position.z);
        _orientation.forward = dirToCombatLookAt.normalized;
        dirToCombatLookAt.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(dirToCombatLookAt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 500f);
        //transform.forward = dirToCombatLookAt.normalized;
    }

    public void UpdateCameraPriority(bool state)
    {
        if (!state)
        {
            _cinemachineCamera.m_Lens.FieldOfView = 45;
        }
        else
        {
            if(_cinemachineCamera.m_Lens.FieldOfView > 30)
                _cinemachineCamera.m_Lens.FieldOfView -= Time.deltaTime * 150f;
        }

    }

  
}
