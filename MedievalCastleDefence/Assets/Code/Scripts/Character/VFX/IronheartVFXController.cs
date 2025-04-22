using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using CartoonFX;
using Cysharp.Threading.Tasks;
using static BehaviourRegistry;

public class IronheartVFXController : PlayerVFXSytem
{
    [SerializeField] private GameObject[] _swordLocalTrails;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
        _swordLocalTrails[0].SetActive(true);
        _swordLocalTrails[1].SetActive(true);
      
    }
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _playerStatsController = GetScript<PlayerStatsController>();
        _stunnedText.transform.localPosition = new Vector3(_stunnedText.transform.localPosition.x, _stunnedText.transform.localPosition.y - 0.3f, _stunnedText.transform.localPosition.z);

        if (_playerStatsController == null)
        {
            Debug.Log("Bulundu PlayerVFX");
        }
        else
        {
            Debug.Log("Bulunamadý PlayerVFX");
        }

    }


}
