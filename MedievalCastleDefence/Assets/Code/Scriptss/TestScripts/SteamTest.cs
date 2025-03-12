using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
public class SteamTest : MonoBehaviour
{
    void Start()
    {
        if (SteamManager.Initialized)
        {
            string playerName = SteamFriends.GetPersonaName();
            Debug.Log("Oyuncu Ýsmi: " + playerName);
        }
        else
        {
            Debug.LogError("Steam baþlatýlamadý!");
        }
    }
}
