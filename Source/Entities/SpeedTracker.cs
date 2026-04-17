using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.ScugHelper.Entities;

internal class SpeedTracker() : Component(true, true)
{
    private SpeedAccessor? accessor = null;
    private Vector2? TrackedSpeed;

    public override void Added(Entity ent) {
        base.Added(ent);
        var access = SpeedAccessor.For(ent);
        if (access is not SpeedAccessor speedAccessor)
        {
            RemoveSelf();
            return;
        }
        Logger.Info(nameof(ScugHelperModule), $"Attached speed tracker to entity of type {ent.GetType()}!");
        accessor = speedAccessor;
    }
    public override void Update()
    {
        base.Update();
        if (accessor is not SpeedAccessor access) return;
        TrackedSpeed = access.Speed;
    }
    public override void Render()
    {
        base.Render();
        if (Entity is null) return;
        if (TrackedSpeed is not Vector2 speed) return;
        string speedX = ((int)speed.X).ToString();
        string speedY = ((int)speed.Y).ToString();
        string speedTotal = ((int)speed.Length()).ToString();
        int speedXLen = speedX.Length * 4 + 1;
        int speedYLen = speedY.Length * 4 + 1;
        int speedTotalLen = speedTotal.Length * 4 + 1;
        int lineWidth = ScugHelperModule.Settings.AlternativeFont ? 6 : 5;
        int offset = 16;

        Text.RenderText(speedX, Entity.TopCenter - new Vector2(speedXLen / 2, offset + lineWidth * 2), Color.Red);
        Text.RenderText(speedY, Entity.TopCenter - new Vector2(speedYLen / 2, offset + lineWidth), Color.Green);
        Text.RenderText(speedTotal, Entity.TopCenter - new Vector2(speedTotalLen / 2, offset), Color.Blue);
    }

    [Command("trackspeed", "Attaches speed trackers to every entity in the scene that supports them.")]
    internal static void TrackSpeed() {
        Scene scene = Engine.Instance.scene;
        foreach (Entity entity in scene.Entities)
            entity.Add(new SpeedTracker());
    }
}
