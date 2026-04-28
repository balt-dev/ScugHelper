using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[TrackedAs(typeof(TempleGate))]
[CustomEntity("ScugHelper/SeekerTempleGate")]
public class SeekerTempleGate(EntityData data, Vector2 offset, string levelID) : TempleGate(data.Position + offset, data.Height, ActionType, data.Attr("sprite", "default"), levelID)
{
    public static readonly Types ActionType = (Types)(-0x5EECE9);
    public override void Update() {
        base.Update();
        if (!open && Scene.Tracker.GetEntity<Seeker>() is null)
            StartOpen();
    }
}
#nullable restore
