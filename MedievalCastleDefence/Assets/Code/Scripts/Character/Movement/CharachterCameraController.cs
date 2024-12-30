using UnityEngine.Rendering.Universal;
using UnityEngine;
using Fusion;
using Cinemachine;


public class CharachterCameraController : BehaviourRegistry
{
    [SerializeField] private Transform _orientation;
    [SerializeField] private Transform _cameraTargetPoint;
    [SerializeField] private Transform _cameraLookAtTarget;
    [SerializeField] private Camera _uiCamera;
    private CinemachineFreeLook _cinemachineCamera;
    private Camera _playerCamera;
    
    [SerializeField] private RectTransform targetUIElement;
  


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

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            HandleCameraRotation();
            UpdateCursorPosition();
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

    public void EnableCameraDepth()
    {
      

    }


}
