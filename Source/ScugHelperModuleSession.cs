namespace Celeste.Mod.ScugHelper;

public class ScugHelperModuleSession : EverestModuleSession {
    public EntityID? BrassBerryFollowing = null;

    public bool RenderedEditorOnce { get; internal set; } = false;
}