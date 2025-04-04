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
    private List<PlayerRef> _playerTeamMates = new List<PlayerRef>();
    private PlayerStatsController _playerStatsController;
    [SerializeField] private RectTransform targetUIElement;
    private int _currentFollowingPlayerIndex = 0;
    private CharacterHealth _characterHealth;
    private CharacterMovement _characterMovement;
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
        }
    }

    private void Start()
    {
        _playerStatsController = GetScript<PlayerStatsController>();
        _characterHealth = GetScript<CharacterHealth>();
        _characterMovement = GetScript<CharacterMovement>();
    }

 
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            if (_characterHealth == null) return;
            if (_characterHealth.IsPlayerDead) return;
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

      
        var currentButtons = input.NetworkButtons;

        if (!_characterHealth.IsPlayerDead) return;


        if (currentButtons.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Interact))
        {
           
            _currentFollowingPlayerIndex++;
            if (_currentFollowingPlayerIndex >= _playerTeamMates.Count)
            {
                _currentFollowingPlayerIndex = 0; 
            }
            FollowTeamPlayerCams();
        }
        
        else if (currentButtons.WasPressed(PreviousButton, LocalInputPoller.PlayerInputButtons.Back))
        {
           
            _currentFollowingPlayerIndex--;
            if (_currentFollowingPlayerIndex < 0)
            {
                _currentFollowingPlayerIndex = _playerTeamMates.Count - 1; 
            }
            FollowTeamPlayerCams();
        }

    
        PreviousButton = currentButtons;
    }

    public async void FollowTeamPlayerCams()
    {
       _playerTeamMates = _playerStatsController.GetAliveTeamPlayers();

       
        if (_playerTeamMates.Count < 1)
        {
            return;
        }

       
        await UniTask.Delay(1000);

      
        var playerObject = Runner.GetPlayerObject(_playerTeamMates[_currentFollowingPlayerIndex]);
        if (playerObject == null)
        {
             return;
        }

       
        var transforms = playerObject.GetComponentsInChildren<Transform>(true);
        var headTransform = transforms.FirstOrDefault(t => t.name == "Head");

        if (headTransform != null)
        {
            _cinemachineCamera.Follow = headTransform;
            _cinemachineCamera.LookAt = headTransform;
            Debug.Log("Kamera takip ediyor: " + headTransform.name);
        }
        else
        {
            Debug.Log("Head transform bulunamadý.");
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
        if (_cinemachineCamera == null || _characterMovement.IsInputDisabled) return;
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
