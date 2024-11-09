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
    [SerializeField] private CinemachineFreeLook _archeryCamera;
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
        if (_playerCamera == null) return;
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
        if (_archeryCamera == null) return;
        _archeryCamera.Priority = state ? 20 : 0;
    }

    public void HandleArcheryCameraAction()
    {
        
        if (_archeryCamera == null) return;
        float horizontalInput = Input.GetAxis("Mouse X");
        float verticalInput = Input.GetAxis("Mouse Y");

        // Cinemachine FreeLook kamera i�in yatay ve dikey inputlar� ayarl�yoruz
        var xAxis = _cinemachineCamera.m_XAxis;
        var yAxis = _cinemachineCamera.m_YAxis;

        xAxis.Value += horizontalInput * 0.1f;  // X ekseninde sa�-sol d�n��
        yAxis.Value -= verticalInput * 0.1f;    // Y ekseninde yukar�-a�a�� d�n��

        // Karakterin y�n�n�, kameran�n bakt��� y�ne g�re ayarl�yoruz
        Vector3 dirToCombatLookAt = _cameraTargetPoint.position - _archeryCamera.transform.position;
        dirToCombatLookAt.y = 0;  // Y eksenindeki d�n��� s�f�rla, b�ylece sadece yatayda d�ner

        Quaternion targetRotation = Quaternion.LookRotation(dirToCombatLookAt);  // Kamera ile karakterin y�n�n� hizala

        // Karakterin rotas�n� yava��a d�nd�r
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        
    }
}
