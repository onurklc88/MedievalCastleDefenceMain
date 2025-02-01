using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
public class PlayerInput : NetworkBehaviour, IReadInput, IRPCListener
{
    public NetworkButtons PreviousButton { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public LevelManager.GamePhase CurrentGamePhase { get; set; }

    [SerializeField] private UIManager _uýManager;

    private void Start()
    {
        CurrentGamePhase = LevelManager.GamePhase.Warmup;
    }
    private void OnEnable()
    {
        EventLibrary.OnGamePhaseChange.AddListener(UpdateGameStateRpc);
    }

    private void OnDisable()
    {
        EventLibrary.OnGamePhaseChange.RemoveListener(UpdateGameStateRpc);
    }
    public void ReadPlayerInputs(PlayerInputData input)
    {
        /*
        if (!Object.HasStateAuthority) return;

      var scoreboardInput =  input.NetworkButtons.IsSet(LocalInputPoller.PlayerInputButtons.Tab);
       
        //if(scoreboardInput == false)
        _uýManager.ShowScoreboard(scoreboardInput);
        */
    }

    private void Update()
    {
        if (_uýManager == null) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
           
            //Test();
        }
     // && CurrentGamePhase != LevelManager.GamePhase.Warmup && CurrentGamePhase != LevelManager.GamePhase.Preparation
        if (Input.GetKey(KeyCode.Tab))
        {
            _uýManager.ShowScoreboard(true);
            //Test();
        }
        else 
        {
            _uýManager.ShowScoreboard(false);
        }
    }

    private void Test()
    {
        Debug.Log($"<color=yellow>Total Players on Server: {Runner.ActivePlayers.Count()}</color>");

        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            var playerNetworkObject = playerObject.transform.GetComponentInParent<NetworkObject>();
            if (playerObject == null)
            {
                Debug.LogWarning($"<color=red>Player object not found for player {player}.</color>");
                continue;
            }

            var stats = playerObject.GetComponent<PlayerStatsController>();
            if (stats == null)
            {
                Debug.LogWarning($"<color=red>PlayerStatsController not found for player {player}.</color>");
                continue;
            }

            PlayerInfo info = stats.PlayerNetworkStats;
            Debug.Log($"<color=cyan>Player {info.PlayerNickName}: Kills {info.PlayerKillCount}, Deaths {info.PlayerDieCount}, NetworkObjectID {playerNetworkObject.Id}</color>");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void UpdateGameStateRpc(LevelManager.GamePhase currentGameState)
    {
        CurrentGamePhase = currentGameState;
    }
}
