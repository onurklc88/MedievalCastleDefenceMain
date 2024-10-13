using UnityEngine;
using Fusion;

public struct PlayerInputData : INetworkInput
{
    public NetworkButtons NetworkButtons;
    public float HorizontalInput;
    public float VerticalInput;
    public float MouseX;
    public float MouseY;
    public bool IsPlayerRunning;
    public bool IsPlayerCrouching;
}
