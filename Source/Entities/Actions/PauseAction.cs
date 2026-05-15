using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/PauseAction")]
public class PauseAction() : Entity(), IAction
{
    public void Alert(Level level)
    {
        if (level.Paused) return;
        level.Pause();
    }
}
