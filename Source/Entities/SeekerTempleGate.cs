using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[TrackedAs(typeof(TempleGate))]
[CustomEntity("ScugHelper/SeekerTempleGate")]
public class SeekerTempleGate(EntityData data, Vector2 offset, EntityID id) : TempleGate(data.Position + offset, data.Height, ActionType, data.Attr("sprite", "default"), id.Level)
{
    bool Opening = false;
    public static readonly Types ActionType = (Types)(-0x5EECE9);
    public override void Update() {
        base.Update();
        if (!open && !Opening && Scene.Tracker.GetEntity<Seeker>() is null)
        {
            Opening = true;
            SwitchOpen();
        }
    }
}
#nullable restore
