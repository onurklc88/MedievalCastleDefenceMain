using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct PlayerInfo : INetworkStruct
{
    public NetworkString<_16> PlayerNickName { get; set; }
    public TeamManager.Teams PlayerTeam;
    public CharacterStats.CharacterType PlayerWarrior;
    public NetworkId PlayerID;
    public int PlayerKillCount;
    public int PlayerDieCount;
}
