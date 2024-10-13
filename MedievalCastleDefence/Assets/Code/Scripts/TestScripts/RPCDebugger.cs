using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
public class RPCDebugger : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _debugText;
    [SerializeField] private TextMeshProUGUI _localText;
    public string NetworkedMessage { get; set; } = "";

    public override void Spawned()
    {
        
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ShowDebugMessageRPC(string text)
    {
        _localText.text = text;
        _localText.color = UnityEngine.Random.ColorHSV();
    }

    public void ShowLocalMessage(string text)
    {
        _localText.text = "Player Local State: " +text;
        _localText.color = UnityEngine.Random.ColorHSV();

    }
}
