#pragma warning disable IDE0130
#nullable enable

using System;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/HoldableTrajectoryController")]
public class HoldableTrajectoryController(EntityData data, Vector2 _) : Entity()
{
    public readonly float MultiplierX = data.Float("MultiplierX", 1);
    public readonly float MultiplierY = data.Float("MultiplierY", 1);
    public readonly float InheritMultiplierX = 
        data.Float("InheritMultiplierX", (data.Has("InheritXSpeed") && !data.Bool("InheritXSpeed")) ? 0 : 1);
    public readonly float InheritMultiplierY = 
        data.Float("InheritMultiplierY", (data.Has("InheritYSpeed") && !data.Bool("InheritYSpeed")) ? 0 : 1);
    
    [OnLoad]
    internal static void LoadHooks() => On.Celeste.Player.Throw += OnThrow;
    [OnUnload]
    internal static void UnloadHooks() => On.Celeste.Player.Throw -= OnThrow;

    
    private static void OnThrow(On.Celeste.Player.orig_Throw orig, Player self) {
        // evil gotos
        if (self.Holding is not Holdable holdable) goto nope;
        if (self.Scene.Tracker.GetEntity<HoldableTrajectoryController>() is not HoldableTrajectoryController ttc) goto nope;
        if (SpeedAccessor.For(holdable.Entity) is not SpeedAccessor spd) goto nope;
        goto yep;
        
    nope:
        orig(self);
        return;
        
    yep:
        Vector2 oldSpeed = self.Speed; // player speed (while being held, should be equal to holdable speed)
        orig(self);
        Vector2 mutSpeed = spd.Speed; // actual, new holdable speed
        mutSpeed.X *= ttc.MultiplierX;
        mutSpeed.Y *= ttc.MultiplierY;
        mutSpeed.X += oldSpeed.X * ttc.InheritMultiplierX;
        mutSpeed.Y += oldSpeed.Y * ttc.InheritMultiplierY;
        spd.Speed = mutSpeed;
    }
}
