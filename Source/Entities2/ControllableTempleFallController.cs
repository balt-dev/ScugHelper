using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/ControllableTempleFallController")]
public class ControllableTempleFallController() : Entity() {
    const float MovementSpeed = 90f;
    const float TerminalVelocity = 320f;
    const float HorizontalAcceleration = 325f;
    const float Gravity = 225f;

    internal static bool ControllerExists(Player player) => player.Scene.Tracker.GetEntity<ControllableTempleFallController>() is not null;

    [OnLoad] internal static void LoadHooks() => On.Celeste.Player.TempleFallUpdate += OnPlayerTempleFallUpdate;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.Player.TempleFallUpdate -= OnPlayerTempleFallUpdate;

    private static int OnPlayerTempleFallUpdate(On.Celeste.Player.orig_TempleFallUpdate orig, Player self) {
        if (ControllerExists(self)) {
            if (!self.onGround) {
                int moveX = Input.MoveX.Value;
                if (moveX != 0)
                    self.Facing = (Facings)moveX;
                self.Speed.X = Calc.Approach(self.Speed.X, MovementSpeed * moveX, HorizontalAcceleration * Engine.DeltaTime);
                if (self.DummyGravity) {
                    self.Speed.Y = Calc.Approach(self.Speed.Y, TerminalVelocity, Gravity * Engine.DeltaTime);
                }
            }
            return Player.StTempleFall;
        }
        return orig(self);
    }
}