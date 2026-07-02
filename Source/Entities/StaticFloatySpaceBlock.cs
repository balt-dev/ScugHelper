using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[TrackedAs(typeof(FloatySpaceBlock))]
[CustomEntity("ScugHelper/StaticFloatySpaceBlock")]
public class StaticFloatySpaceBlock(EntityData data, Vector2 offset) : FloatySpaceBlock(data, offset)
{
    [OnLoad]
    internal static void LoadHooks() {
        Utils.UninlineMethod((FloatySpaceBlock p) => p.MoveToTarget());
        On.Celeste.FloatySpaceBlock.MoveToTarget += OnMoveToTarget;
    }
    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.FloatySpaceBlock.MoveToTarget -= OnMoveToTarget;
    }
    private static void OnMoveToTarget(On.Celeste.FloatySpaceBlock.orig_MoveToTarget orig, FloatySpaceBlock self) {
        if (self is StaticFloatySpaceBlock)
            self.sineWave = 0f;
        orig(self);
    }
}

#nullable restore
