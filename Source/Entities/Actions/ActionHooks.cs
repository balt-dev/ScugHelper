using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ScugHelper.Entities.Actions;
#nullable enable

public static class ActionHooks
{
    public static void LoadHooks() {
        On.Celeste.Player.Jump += OnJump;
        On.Celeste.Player.WallJump += OnWallJump;
        On.Celeste.Player.SuperJump += OnSuperJump;
        On.Celeste.Player.SuperWallJump += OnSuperWallJump;
        On.Celeste.Player.ClimbJump += OnClimbJump;
        On.Celeste.Player.ClimbBegin += OnGrab;
        On.Celeste.Player.CallDashEvents += OnDashEvents;
        On.Celeste.Player.Die += OnDie;
        On.Celeste.Seeker.ctor_Vector2_Vector2Array += OnSeekerCtor;
        On.Celeste.Player.Update += OnUpdate;
        On.Celeste.CassetteBlockManager.SetActiveIndex += OnCassetteBlock;
        On.Celeste.Strawberry.OnCollect += OnBerryCollect;
    }

    private static void OnBerryCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self)
    {
        orig(self);
        ActionManager.AlertActions(["#BerryCollect"], self.SceneAs<Level>());
    }

    private static void OnCassetteBlock(On.Celeste.CassetteBlockManager.orig_SetActiveIndex orig, CassetteBlockManager self, int index)
    {
        orig(self, index);
        ActionManager.AlertActions([$"#CassetteBlock{index}"], self.SceneAs<Level>());
    }

    private static void OnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (!self.wasOnGround && self.OnGround())
             ActionManager.AlertActions(["#PlayerLand"], self.level);
    }

    private static void OnSeekerCtor(On.Celeste.Seeker.orig_ctor_Vector2_Vector2Array orig, Seeker self, Vector2 position, Vector2[] patrolPoints)
    {
        orig(self, position, patrolPoints);
        var prevCB = self.SquishCallback;
        self.SquishCallback = d => {
            bool was = self.dead;
            prevCB(d);
            if (self.dead && !was) ActionManager.AlertActions(["#SeekerDie"], self.SceneAs<Level>());
        };
    }

    private static PlayerDeadBody OnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        var was = self.Dead;
        var res = orig(self, direction, evenIfInvincible, registerDeathInStats);
        if (self.Dead && !was)
            ActionManager.AlertActions(["#PlayerDie"], self.level);
        return res;
    }

    private static void OnDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self)
    {
        var was = self.calledDashEvents;
        orig(self);
        if (self.calledDashEvents && !was)
            ActionManager.AlertActions(["#PlayerDash"], self.level);
    }

    public static void UnloadHooks()
    {
        On.Celeste.Player.Jump -= OnJump;
        On.Celeste.Player.WallJump -= OnWallJump;
        On.Celeste.Player.SuperJump -= OnSuperJump;
        On.Celeste.Player.SuperWallJump -= OnSuperWallJump;
        On.Celeste.Player.ClimbJump -= OnClimbJump;
        On.Celeste.Player.ClimbBegin -= OnGrab;
        On.Celeste.Player.CallDashEvents -= OnDashEvents;
    }

    private static void OnGrab(On.Celeste.Player.orig_ClimbBegin orig, Player self)
    {
        orig(self);
        ActionManager.AlertActions(["#PlayerGrab"], self.level);
    }

    private static void OnClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        orig(self);
        ActionManager.AlertActions(["#PlayerClimbJump"], self.level);
    }

    private static void OnSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        orig(self, dir);
        ActionManager.AlertActions(["#PlayerSuperWallJump"], self.level);
    }

    private static void OnSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        orig(self);
        ActionManager.AlertActions(["#PlayerSuperJump"], self.level);
    }

    private static void OnWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        orig(self, dir);
        ActionManager.AlertActions(["#PlayerWallJump"], self.level);
    }

    private static void OnJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        orig(self, particles, playSfx);
        ActionManager.AlertActions(["#PlayerJump"], self.level);
    }
}
