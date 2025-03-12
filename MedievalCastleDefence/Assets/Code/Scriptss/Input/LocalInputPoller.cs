using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

public class LocalInputPoller : NetworkBehaviour, INetworkRunnerCallbacks, IBeforeUpdate
{
    public enum PlayerInputButtons
    {
        None,
        Walk,
        UtilitySkill,
        Crouch,
        Jump,
        Slide,
        Interact,
        Back,
        Tab,
        UltimateSkill,
        Mouse0,
        Mouse1,
        Reload,
        Hotkey1,
        Hotkey2,
        LeftArrow,
        RightArrow,
        SwitchTeamKey
    }

    private float _horizontal;
    private float _vertical;
    private float _mouseXRotation;
    private float _mouseYRotation;

    private float _wTapTime = -1f, _aTapTime = -1f, _dTapTime = -1f, _sTapTime = -1;
    private bool _wDoubleTap, _aDoubleTap, _dDoubleTap, _sDoubleTap;
    private const float DoubleTapTimeThreshold = 0.5f;

    private bool _wKeyPrevious = false, _aKeyPrevious = false, _dKeyPrevious = false, _sKeyPrevious = false;

    public override void Spawned()
    {
        if (HasStateAuthority == false)
        {
            return;
        }

        Runner.AddCallbacks(this);
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = GetNetworkInput();
        input.Set(data);
    }

    public void BeforeUpdate()
    {
        _vertical = Input.GetAxisRaw("Vertical");
        _horizontal = Input.GetAxisRaw("Horizontal");
        _mouseXRotation = Input.GetAxis("Mouse X");
        _mouseYRotation = Input.GetAxis("Mouse Y");
        CheckDoubleTaps();
    }

    public PlayerInputData GetNetworkInput()
    {
        var data = new PlayerInputData();
        data.HorizontalInput = _horizontal;
        data.VerticalInput = _vertical;
        data.MouseX = _mouseXRotation;
        data.MouseY = _mouseYRotation;
        data.ForwardDoubleTab = _wDoubleTap;
        data.LeftDoubleTab = _aDoubleTap;
        data.RightDoubleTab = _dDoubleTap;
        data.BacwardsDoubleTab = _sDoubleTap;
        _wDoubleTap = false;
        _aDoubleTap = false;
        _dDoubleTap = false;
        _sDoubleTap = false;
        data.NetworkButtons.Set(PlayerInputButtons.UtilitySkill, Input.GetKey(KeyCode.LeftShift));
        data.NetworkButtons.Set(PlayerInputButtons.Crouch, Input.GetKey(KeyCode.C));
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        data.NetworkButtons.Set(PlayerInputButtons.Interact, Input.GetKey(KeyCode.E));
        data.NetworkButtons.Set(PlayerInputButtons.Back, Input.GetKey(KeyCode.Q));
        data.NetworkButtons.Set(PlayerInputButtons.Tab, Input.GetKey(KeyCode.Tab));
        data.NetworkButtons.Set(PlayerInputButtons.Mouse0, Input.GetKey(KeyCode.Mouse0));
        data.NetworkButtons.Set(PlayerInputButtons.Mouse1, Input.GetKey(KeyCode.Mouse1));
        data.NetworkButtons.Set(PlayerInputButtons.Hotkey1, Input.GetKey(KeyCode.Alpha1));
        data.NetworkButtons.Set(PlayerInputButtons.Hotkey2, Input.GetKey(KeyCode.Alpha2));
        data.NetworkButtons.Set(PlayerInputButtons.UltimateSkill, Input.GetKey(KeyCode.R));
        data.NetworkButtons.Set(PlayerInputButtons.LeftArrow, Input.GetKey(KeyCode.LeftArrow));
        data.NetworkButtons.Set(PlayerInputButtons.RightArrow, Input.GetKey(KeyCode.RightArrow));
        data.NetworkButtons.Set(PlayerInputButtons.SwitchTeamKey, Input.GetKey(KeyCode.M));

        return data;
    }

    private void CheckDoubleTaps()
    {
        bool wKeyCurrent = Input.GetKey(KeyCode.W);
        bool aKeyCurrent = Input.GetKey(KeyCode.A);
        bool dKeyCurrent = Input.GetKey(KeyCode.D);
        bool sKeyCurrent = Input.GetKey(KeyCode.S);
       
        if (wKeyCurrent && !_wKeyPrevious)
        {
            if (_wTapTime > 0 && Time.time - _wTapTime <= DoubleTapTimeThreshold)
            {
                _wDoubleTap = true; 
                _wTapTime = -1f;
            }
            else
            {
                _wTapTime = Time.time;
            }
        }

        
        if (aKeyCurrent && !_aKeyPrevious)
        {
            if (_aTapTime > 0 && Time.time - _aTapTime <= DoubleTapTimeThreshold)
            {
                _aDoubleTap = true;
                _aTapTime = -1f;
            }
            else
            {
                _aTapTime = Time.time;
            }
        }

       
        if (dKeyCurrent && !_dKeyPrevious)
        {
            if (_dTapTime > 0 && Time.time - _dTapTime <= DoubleTapTimeThreshold)
            {
                _dDoubleTap = true;
                _dTapTime = -1f;
            }
            else
            {
                _dTapTime = Time.time;
            }
        }

        if (sKeyCurrent && !_sKeyPrevious)
        {
            if (_sTapTime > 0 && Time.time - _sTapTime <= DoubleTapTimeThreshold)
            {
                _sDoubleTap = true;
                _sTapTime = -1f;
            }
            else
            {
                _sTapTime = Time.time;
            }
        }

        _wKeyPrevious = wKeyCurrent;
        _aKeyPrevious = aKeyCurrent;
        _dKeyPrevious = dKeyCurrent;
        _sKeyPrevious = sKeyCurrent;
    }

    #region Callbacks
    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    #endregion
}