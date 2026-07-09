using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/RemovePlayerTrigger")]
public class RemovePlayerTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    public override void OnEnter(Player player) {
        base.OnEnter(player);
        player.RemoveSelf();
    }
}
