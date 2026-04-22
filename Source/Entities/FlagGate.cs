using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/FlagGate")]
public class FlagGate(EntityData data, Vector2 offset) : AbstractGate(data, offset) {
    private readonly bool State = data.Bool("State", true);
    private readonly string Flag = data.String("Flag", "");
    public override void OnTrigger(Player player)
    {
        Level level = SceneAs<Level>();
        level.Session.SetFlag(Flag, State);
    }
}
