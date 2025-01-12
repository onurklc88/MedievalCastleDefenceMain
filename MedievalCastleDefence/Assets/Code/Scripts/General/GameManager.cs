using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameManager : NetworkBehaviour
{
   public enum GameModes
    {
        None,
        OneVsOne,
        TwoVsTwo,
        ThreeVsThree,
        Tournament,
        Conquest
   }

    public GameMode GameMode;
}
