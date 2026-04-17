using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/PlaySoundAction")]
public class PlaySoundAction(EntityData data, Vector2 _) : Entity(), IAction
{
    readonly string SoundPath = data.String("SoundPath");
    public void Alert(Level level)
    {
        Audio.Play(SoundPath);
    }
}