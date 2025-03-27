
public class StormshieldVFXController : PlayerVFXSytem
{
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        InitScript(this);
    }
}
