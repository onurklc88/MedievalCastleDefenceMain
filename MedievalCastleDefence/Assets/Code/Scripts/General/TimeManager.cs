using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TimeManager : NetworkBehaviour
{
    [Networked] private TickTimer _matchTimer { get; set; }
    private const float WARMUP_MATCH_TIME = 60f;
    private const float MATCH_TIME_AMOUNT = 600f;
    private float _currentTimeAmount;
}
