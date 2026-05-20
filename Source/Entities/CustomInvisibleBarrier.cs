using System;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/CustomInvisibleBarrier")]
public class CustomInvisibleBarrier : InvisibleBarrier
{
    public readonly string? Flag;
    public readonly bool FlagState;

    public CustomInvisibleBarrier(EntityData data, Vector2 offset) : base(data, offset)
    {
        Player _;
        SurfaceSoundIndex = data.Int("SurfaceSoundIndex", 33);
        Flag = data.String("Flag");
        FlagState = data.Bool("FlagState");
        Collidable = true;
        if (data.Bool("Climbable"))
            Components.RemoveAll<ClimbBlocker>();
    }

    public override void Update()
    {
        base.Update();
        Active = true;
        Collidable = Flag is not string flag || SceneAs<Level>().Session.GetFlag(flag) == FlagState;
    }
}
