using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/PlayerKillAction")]
public class PlayerKillAction() : Entity(), IAction
{
    public void Alert(Level level)
    {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        if (!player.Dead) player.Die(Vector2.Zero);
    }
}