using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/GiveKeyAction")]
public class GiveKeyAction() : Entity(), IAction
{
    public void Alert(Level level) {
        if (level.Tracker.GetEntity<Player>() is not Player player) return;
        Key key = new(player, new EntityID("unknown", 1073741823 + Calc.Random.Next(10000)));
        level.Add(key);
        level.Session.Keys.Add(key.ID);
    }
}