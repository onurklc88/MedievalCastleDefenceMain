using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct PlayerStats : INetworkStruct
{
    public NetworkString<_16> PlayerName { get; set; }
    //public PlayerStats.Team PlayerTeam;
    public CharacterStats.CharacterType PlayerWarrior;
    public int PlayerKillCount;
    public int PlayerDieCount;
}
