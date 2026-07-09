namespace Celeste.Mod.ScugHelper;

public class ScugHelperModuleSession : EverestModuleSession {
    public EntityID? BrassBerryFollowing = null;

    public bool RenderedEditorOnce { get; internal set; } = false;
    public bool BrassBerryCountNormal { get; internal set; } = false;
    public int JumpsAtLevelStart { get; internal set; } = 0;
    public int Jumps { get; internal set; } = 0;
}