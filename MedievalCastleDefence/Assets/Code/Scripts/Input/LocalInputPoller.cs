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
        Sprint,
        Crouch,
        Jump,
        Slide,
        Interact,
        Back,
        Tab,
        Skill,
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
    }

    public PlayerInputData GetNetworkInput()
    {
        var data = new PlayerInputData();
        data.HorizontalInput = _horizontal;
        data.VerticalInput = _vertical;
        data.MouseX = _mouseXRotation;
        data.MouseY = _mouseYRotation;
        data.NetworkButtons.Set(PlayerInputButtons.Sprint, Input.GetKey(KeyCode.LeftShift));
        data.NetworkButtons.Set(PlayerInputButtons.Crouch, Input.GetKey(KeyCode.C));
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        data.NetworkButtons.Set(PlayerInputButtons.Interact, Input.GetKey(KeyCode.E));
        data.NetworkButtons.Set(PlayerInputButtons.Back, Input.GetKey(KeyCode.Q));
        data.NetworkButtons.Set(PlayerInputButtons.Tab, Input.GetKey(KeyCode.Tab));
        data.NetworkButtons.Set(PlayerInputButtons.Mouse0, Input.GetKey(KeyCode.Mouse0));
        data.NetworkButtons.Set(PlayerInputButtons.Mouse1, Input.GetKey(KeyCode.Mouse1));
        data.NetworkButtons.Set(PlayerInputButtons.Reload, Input.GetKey(KeyCode.R));
        data.NetworkButtons.Set(PlayerInputButtons.Hotkey1, Input.GetKey(KeyCode.Alpha1));
        data.NetworkButtons.Set(PlayerInputButtons.Hotkey2, Input.GetKey(KeyCode.Alpha2));
        data.NetworkButtons.Set(PlayerInputButtons.Skill, Input.GetKey(KeyCode.R));
        data.NetworkButtons.Set(PlayerInputButtons.LeftArrow, Input.GetKey(KeyCode.LeftArrow));
        data.NetworkButtons.Set(PlayerInputButtons.RightArrow, Input.GetKey(KeyCode.RightArrow));
        data.NetworkButtons.Set(PlayerInputButtons.SwitchTeamKey, Input.GetKey(KeyCode.M));
        return data;
    }


  
    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
       
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
       
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
       
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
       
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
       
    }

  
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
       
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
       
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
      
    }
}
