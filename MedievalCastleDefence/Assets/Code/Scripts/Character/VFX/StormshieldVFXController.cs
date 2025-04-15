using UnityEngine;
public class StormshieldVFXController : PlayerVFXSytem
{
    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        _playerStatsController = GetScript<PlayerStatsController>();
        _stunnedText.transform.localPosition = new Vector3(_stunnedText.transform.localPosition.x, _stunnedText.transform.localPosition.y - 0.3f, _stunnedText.transform.localPosition.z);
    }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }

   
}
