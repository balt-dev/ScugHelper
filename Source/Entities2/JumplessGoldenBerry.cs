using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using Microsoft.Xna.Framework;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[RegisterStrawberry(false, true)]
[CustomEntity("ScugHelper/JumplessGoldenBerry")]
class JumplessGoldenBerry : Strawberry, IStrawberry {
    public JumplessGoldenBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
        Golden = true;
        Winged = true;
        Components.RemoveAll<DashListener>();
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        if (
            scene is not Level level ||
            ScugHelperModule.Session.Jumps > 0 ||
            !(level.Session.StartedFromBeginning || level.Session.RestartedFromGolden)
        ) { RemoveSelf(); return; }
    }

    public override void Update() {
        base.Update();
        if (ScugHelperModule.Session.Jumps > 0 && !flyingAway)
            OnDash(Vector2.UnitX);
    }

    [OnLoad] internal static void LoadHooks() {
        On.Celeste.Player.Jump += OnJump;
        On.Celeste.Player.WallJump += OnWallJump;
        On.Celeste.Player.SuperJump += OnSuperJump;
        On.Celeste.Player.SuperWallJump += OnSuperWallJump;
        On.Celeste.Player.ClimbJump += OnClimbJump;
        On.Celeste.Level.Reload += OnLevelReload;
        On.Celeste.Session.UpdateLevelStartDashes += OnSessionUpdateLevelStartDashes;
    }

    [OnUnload] internal static void UnloadHooks() {
        On.Celeste.Player.Jump -= OnJump;
        On.Celeste.Player.WallJump -= OnWallJump;
        On.Celeste.Player.SuperJump -= OnSuperJump;
        On.Celeste.Player.SuperWallJump -= OnSuperWallJump;
        On.Celeste.Player.ClimbJump -= OnClimbJump;
        On.Celeste.Level.Reload -= OnLevelReload;
        On.Celeste.Session.UpdateLevelStartDashes -= OnSessionUpdateLevelStartDashes;
    }

    private static void OnJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
        orig(self, particles, playSfx);
        ScugHelperModule.Session.Jumps++;
    }

    private static void OnWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
        orig(self, dir);
        ScugHelperModule.Session.Jumps++;
    }

    private static void OnSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self) {
        orig(self);
        ScugHelperModule.Session.Jumps++;
    }

    private static void OnSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
        orig(self, dir);
        ScugHelperModule.Session.Jumps++;
    }

    private static void OnClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
        orig(self);
        ScugHelperModule.Session.Jumps++;
    }

    private static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self) {
        if (!self.Completed)
            ScugHelperModule.Session.Jumps = ScugHelperModule.Session.JumpsAtLevelStart;
        orig(self);
    }

    private static void OnSessionUpdateLevelStartDashes(On.Celeste.Session.orig_UpdateLevelStartDashes orig, Session self) {
        orig(self);
        ScugHelperModule.Session.JumpsAtLevelStart = ScugHelperModule.Session.Jumps;
    }
}
