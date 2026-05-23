using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/CenterCameraTrigger")]
public class CenterCameraTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    private readonly bool DoCenterX = data.Bool("CenterX", true);
    private readonly bool DoCenterY = data.Bool("CenterY", true);

    private static bool CenterCameraX = false;
    private static bool CenterCameraY = false;

    private static float CenterCameraXEffectiveness = 0f;
    private static float CenterCameraYEffectiveness = 0f;
    private static readonly float LerpTightness = 0.2f;

    private static Vector2 CameraOffsetLerp = Vector2.Zero;

    public override void OnEnter(Player player) {
        CenterCameraX = DoCenterX;
        CenterCameraY = DoCenterY;
    }
    public override void OnLeave(Player player) {
        CenterCameraX = false;
        CenterCameraY = false;
    }

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.LevelLoader.StartLevel += OnLevelLoaderStartLevel;
    }
    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.LevelLoader.StartLevel -= OnLevelLoaderStartLevel;
    }

    private static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self) {
        CenterCameraX = CenterCameraY = false;
        CenterCameraXEffectiveness = CenterCameraYEffectiveness = 0f;
        orig(self);
    }

    private static void OnLevelLoaderStartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
        CenterCameraX = CenterCameraY = false;
        CenterCameraXEffectiveness = CenterCameraYEffectiveness = 0f;
        orig(self);
    }

    // Called by CameraBlocker.cs, right before camera blockers process their stuff
    internal static void OnLevelUpdate(Level self) {
        bool shouldCenterX = CenterCameraX || ScugHelperModule.Settings.AlwaysCenterCameraX;
        bool shouldCenterY = CenterCameraY || ScugHelperModule.Settings.AlwaysCenterCameraY;
        CenterCameraXEffectiveness = float.Lerp(CenterCameraXEffectiveness, shouldCenterX ? 1 : 0, 1 - MathF.Pow(LerpTightness, Engine.DeltaTime));
        CenterCameraYEffectiveness = float.Lerp(CenterCameraYEffectiveness, shouldCenterY ? 1 : 0, 1 - MathF.Pow(LerpTightness, Engine.DeltaTime));
        CameraOffsetLerp = Vector2.Lerp(CameraOffsetLerp, self.CameraOffset, 1 - MathF.Pow(LerpTightness, Engine.DeltaTime));
        if (self.Tracker.GetEntity<Player>() is not Player player) return;
        var result = player.ExactPosition - new Vector2(self.Camera.Right - self.Camera.Left, self.Camera.Bottom - self.Camera.Top) / 2 + CameraOffsetLerp;
        result.X = MathHelper.Clamp(result.X, self.Bounds.Left, self.Bounds.Right - (self.Camera.Right - self.Camera.Left));
        result.Y = MathHelper.Clamp(result.Y, self.Bounds.Top, self.Bounds.Bottom - (self.Camera.Bottom - self.Camera.Top));
        self.Camera.X = float.Lerp(self.Camera.X, result.X, CenterCameraXEffectiveness);
        self.Camera.Y = float.Lerp(self.Camera.Y, result.Y, CenterCameraYEffectiveness);
    }
}
