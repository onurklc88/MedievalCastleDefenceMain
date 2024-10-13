using System;
using UnityEngine;
using Fusion;
public static class EventLibrary
{

    //Main Menu Events
    /*
    public static readonly GameEvent OnPlayerCreateNickname = new GameEvent();
    public static readonly GameEvent<bool> OnPlayerJoinedServer = new GameEvent<bool>();
    public static readonly GameEvent<string> OnJoinButtonClicked = new GameEvent<string>();
    public static readonly GameEvent OnPasswordPanelRequested = new GameEvent();
    public static readonly GameEvent<bool> OnTeamSelected = new GameEvent<bool>();
    public static readonly GameEvent<NetworkString<_16>> OnTeamsUpdate = new GameEvent<NetworkString<_16>>();
    */
    #region Animation Events
    public static readonly GameEvent OnPlayerJump = new GameEvent();
    public static readonly GameEvent<bool> OnPlayerSwing = new GameEvent<bool>();
    #endregion
    public static readonly GameEvent<PlayerRef> OnRespawnRequested = new GameEvent<PlayerRef>();


}
