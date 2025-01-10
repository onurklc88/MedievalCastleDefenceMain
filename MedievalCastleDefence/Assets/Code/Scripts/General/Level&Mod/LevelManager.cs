using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;

public class LevelManager : NetworkBehaviour
{
    protected int MaxPlayerCount;
    protected int MaxTeamPlayerCount;
    
    protected int CurrentLevelIndex;
    protected int TotalLevelRound;
    [Networked(OnChanged = nameof(OnPlayerCountChange))]
    public int CurrentPlayerCount { get; set; }
    private int _localPlayerCount;
    public enum GamePhase
    {
        None,
        Warmup,
        War,
        End
    }
    public GamePhase CurrentGamePhase;
    
    public override void Spawned()
    {
       EventLibrary.OnPlayerSelectWarrior.AddListener(UpdatePlayerCount);
       CurrentGamePhase = GamePhase.Warmup;
      
    }

    private void CheckCurrentPlayerCount()
    {

    }

    private void UpdatePlayerCount()
    {
         CurrentPlayerCount += 1;
        if(CurrentPlayerCount == MaxPlayerCount)
        {
            CurrentGamePhase = GamePhase.War;
        }
    }

    private static void OnPlayerCountChange(Changed<LevelManager> changed)
    {
        Debug.LogWarning($"<color=yellow>Player count updated: {changed.Behaviour.CurrentPlayerCount}</color>");
    }

 


}
