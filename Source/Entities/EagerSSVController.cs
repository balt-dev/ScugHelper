using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Utils;
using Celeste.Mod.ScugHelper.SpecialSessionVariables;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/EagerSSVController")]
[Tracked(false)]
public class EagerSSVController(EntityData data, Vector2 _) : Entity() {
    public readonly float UpdatePeriod = data.Float("UpdatePeriod", 0.05f);
    public override void Update() {
        base.Update();
        Level level = Scene as Level;
        
        if (!level.OnInterval(UpdatePeriod)) return;

        SSV.FlushSSVStates(level);
    }
}
