using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using Fusion;
using Cinemachine;


public class CharachterCameraController : NetworkBehaviour
{
    [SerializeField] private Transform _orientation;
    [SerializeField] private Transform _cameraTargetPoint;
    [SerializeField] private Transform _cameraLookAtTarget;
    [SerializeField] private Camera _uiCamera;
    private CinemachineFreeLook _cinemachineCamera;
    private Camera _playerCamera;
    [SerializeField] private GameObject _testCube;
    [SerializeField] private RectTransform targetUIElement;
  


    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            
           // _testCube = GameObject.Find("Test");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cinemachineCamera = FindObjectOfType<CinemachineFreeLook>();
            _playerCamera = Camera.main;
            var cameraData = _playerCamera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(_uiCamera);
            _uiCamera.enabled = true;
            _cinemachineCamera.Follow = _cameraTargetPoint;
            _cinemachineCamera.LookAt = _cameraLookAtTarget;
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
    private void LateUpdate()
    {
        //UpdateCursorPosition();
    }
   
    private void Update()
    {
        //UpdateCursorPosition();

    }

    private void FixedUpdate()
    {
       // UpdateCursorPosition();
    }
    private void UpdateCursorPosition()
    {
        if (_playerCamera == null || _testCube == null) return;
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

    private void ZoomInCharacterCamera()
    {

    }
}
