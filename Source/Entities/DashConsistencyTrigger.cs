using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using MonoMod.Cil;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/DashSnapConsistencyTrigger")]
[Tracked(false)]
public class DashSnapConsistencyTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    Vector2 SnapPosition = data.FirstNodeNullable(offset) ?? throw new Exception("Node required for dash consistency snap trigger.");
    Vector2 SnapSubpixel = new(Math.Clamp(data.Float("SubpixelX"), -0.5f, 0.5f), Math.Clamp(data.Float("SubpixelY"), -0.5f, 0.5f));
    
    DashConsistencySnapComponent? maybeComp;
    public override void OnEnter(Player player) {
        base.OnEnter(player);
        player.Add(maybeComp = new DashConsistencySnapComponent(SnapPosition, SnapSubpixel));
    }
    public override void OnLeave(Player player) {
        base.OnLeave(player);
        if (maybeComp is DashConsistencySnapComponent comp) player.Remove(comp);
    }

    private class DashConsistencySnapComponent(Vector2 snapPos, Vector2 snapSubpx): Component(false, false) {
        internal readonly Vector2 SnapPosition = snapPos;
        internal readonly Vector2 SnapSubpixel = snapSubpx;
    }

    [OnLoad] internal static void LoadHooks() => On.Celeste.Player.CallDashEvents += OnPlayerCallDashEvenets;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.Player.CallDashEvents -= OnPlayerCallDashEvenets;

    private static void OnPlayerCallDashEvenets(On.Celeste.Player.orig_CallDashEvents orig, Player self) {
        if (self.Get<DashConsistencySnapComponent>() is DashConsistencySnapComponent dashSnap) {
            if (self.DashDir.Y != 0) {
                self.Position.X = dashSnap.SnapPosition.X;
                self.movementCounter.X = dashSnap.SnapSubpixel.X;
            }
            if (self.DashDir.X != 0) {
                self.Position.Y = dashSnap.SnapPosition.Y;
                self.movementCounter.Y = dashSnap.SnapSubpixel.Y;
            }
        }
        orig(self);
    }
}
