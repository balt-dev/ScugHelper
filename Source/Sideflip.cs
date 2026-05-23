using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.ScugHelper;
using Celeste.Mod.ScugHelper.Entities.Actions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper;

public static class Sideflip
{
    public static bool Enabled {
        get {
            if (ScugHelperModule.Settings.SideflippingEverywhere) return true;
            if (Monocle.Engine.Scene is not Level level) return false;
            return level.Session.GetFlag("ScugHelper-AllowSideflipping");
        }
    }
    internal static readonly int SwapFrameLeniency = 6;
    internal static readonly int JumpFrameLeniency = 6;
    internal static readonly float BackflipYBoostLow = 160f;
    internal static readonly float BackflipYBoostHigh = 260f;
    internal static readonly float BackflipXMult = 0.6f;
    internal static readonly int BackflipXLow = 90;
    internal static readonly int BackflipXHigh = 220;

    internal static int LastNonZeroX = 0;
    internal static int FramesSinceNonZero = 100;
    internal static int FramesSinceSwap = 100;

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Player.Update += OnUpdate;
        On.Celeste.Player.Jump += OnJump_Sideflip;
    }
    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Player.Update -= OnUpdate;
        On.Celeste.Player.Jump -= OnJump_Sideflip;
    }

    private static void OnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        orig(self);
        FramesSinceSwap++;
        int inputX = Input.MoveX.Value;
        if (inputX == 0) FramesSinceNonZero++;
        if (FramesSinceNonZero > SwapFrameLeniency) LastNonZeroX = 0;
        if (inputX != 0) FramesSinceNonZero = 0;
        if (LastNonZeroX == 0) { LastNonZeroX = inputX; return; }
        if (inputX != 0) {
            if (inputX != LastNonZeroX) FramesSinceSwap = 0;
            LastNonZeroX = inputX;
        }
    }

    private static void OnJump_Sideflip(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
        bool wasNormal = self.StateMachine.State == Player.StNormal;
        if (Enabled && wasNormal && FramesSinceSwap <= JumpFrameLeniency) {
            Input.Jump.ConsumeBuffer();
            self.jumpGraceTimer = 0f;
            self.varJumpTimer = 0.2f;
            self.AutoJump = false;
            self.dashAttackTimer = 0f;
            self.gliderBoostTimer = 0f;
            self.wallSlideTimer = 1.2f;
            self.wallBoostTimer = 0f;
            self.Speed.X = BackflipXMult * Math.Abs(self.Speed.X) * LastNonZeroX;
            self.Speed.Y = -float.Lerp(BackflipYBoostLow, BackflipYBoostHigh, Math.Max(0, (Math.Abs(self.Speed.X) - BackflipXLow) / (BackflipXHigh - BackflipXLow)));
            if (self.Holding?.Entity is Entities.TungstenCube)
                self.Speed.Y *= 0.3f;
            self.Speed += self.LiftBoost;
            self.varJumpSpeed = self.Speed.Y;
            if (particles) {
                int index = -1;
                Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(self.CollideAll<Platform>(self.Position + Vector2.UnitY, self.temp));
                if (platformByPriority != null)
                    index = platformByPriority.GetLandSoundIndex(self);
                Dust.Burst(self.BottomCenter, -MathF.PI / 2f, 4, self.DustParticleFromSurfaceIndex(index));
            }

            SaveData.Instance.TotalJumps++;
            self.Play("event:/char/madeline/jump_super");
            self.launched = true;
            ActionManager.AlertActions(["#PlayerSideflip"], self.level);
            return;
        }
        orig(self, particles, playSfx);
    }
}
