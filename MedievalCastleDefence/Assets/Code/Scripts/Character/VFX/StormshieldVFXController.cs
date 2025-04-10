
public class StormshieldVFXController : PlayerVFXSytem
{
    private void Start()
    {
        _playerStatsController = GetScript<PlayerStatsController>();
    }
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }
}
