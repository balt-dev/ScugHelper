using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/PlayerStateAction")]
public class PlayerStateAction(EntityData data, Vector2 _) : Entity(), IAction
{
    readonly int State = data.Int("State", 0);
    public void Alert(Level level) {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        player.StateMachine.State = State;
    }
}