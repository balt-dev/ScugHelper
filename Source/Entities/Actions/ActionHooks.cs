using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ScugHelper.Entities.Actions;
#nullable enable

public static class ActionHooks
{
    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Player.Jump += OnJump_Action;
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
        On.Celeste.DashSwitch.OnDashed += OnDashSwitch;
        On.Celeste.TouchSwitch.TurnOn += OnTouchSwitch;
        On.Celeste.Torch.OnPlayer += OnTorch;
        On.Celeste.Player.Bounce += OnBounce;
        On.Celeste.Player.SuperBounce += OnSuperBounce;
        On.Celeste.Player.Pickup += OnPickup;
        On.Celeste.Player.Drop += OnDrop;
        On.Celeste.Player.PointBounce += OnPointBounce;
        On.Celeste.Player.Throw += OnThrow;
        On.Celeste.Player.Rebound += OnRebound;
        On.Celeste.Player.ReflectBounce += OnReflectBounce;
    }
    [OnUnload]
    public static void UnloadHooks()
    {
        On.Celeste.Player.Jump -= OnJump_Action;
        On.Celeste.Player.WallJump -= OnWallJump;
        On.Celeste.Player.SuperJump -= OnSuperJump;
        On.Celeste.Player.SuperWallJump -= OnSuperWallJump;
        On.Celeste.Player.ClimbJump -= OnClimbJump;
        On.Celeste.Player.Bounce -= OnBounce;
        On.Celeste.Player.SuperBounce -= OnSuperBounce;
        On.Celeste.Player.Pickup -= OnPickup;
        On.Celeste.Player.Drop -= OnDrop;
        On.Celeste.Player.PointBounce -= OnPointBounce;
        On.Celeste.Player.Throw -= OnThrow;
        On.Celeste.Player.Rebound -= OnRebound;
        On.Celeste.Player.ReflectBounce -= OnReflectBounce;
        On.Celeste.Player.ClimbBegin -= OnGrab;
        On.Celeste.Player.CallDashEvents -= OnDashEvents;
        On.Celeste.Player.Die -= OnDie;
        On.Celeste.Seeker.ctor_Vector2_Vector2Array -= OnSeekerCtor;
        On.Celeste.Player.Update -= OnUpdate;
        On.Celeste.CassetteBlockManager.SetActiveIndex -= OnCassetteBlock;
        On.Celeste.Strawberry.OnCollect -= OnBerryCollect;
        On.Celeste.DashSwitch.OnDashed -= OnDashSwitch;
        On.Celeste.TouchSwitch.TurnOn -= OnTouchSwitch;
        On.Celeste.Torch.OnPlayer -= OnTorch;
    }

    private static void OnReflectBounce(On.Celeste.Player.orig_ReflectBounce orig, Player self, Vector2 direction)
    {
        orig(self, direction);
        ActionManager.AlertActions(["#PlayerReflectBounce"], self.SceneAs<Level>());
    }

    private static void OnBounce(On.Celeste.Player.orig_Bounce orig, Player self, float fromY)
    {
        orig(self, fromY);
        ActionManager.AlertActions(["#PlayerBounce"], self.SceneAs<Level>());
    }

    private static void OnSuperBounce(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY)
    {
        orig(self, fromY);
        ActionManager.AlertActions(["#PlayerSuperBounce"], self.SceneAs<Level>());
    }

    private static bool OnPickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup)
    {
        var res = orig(self, pickup);
        if (res) ActionManager.AlertActions(["#PlayerPickup"], self.SceneAs<Level>());
        return res;
    }

    private static void OnDrop(On.Celeste.Player.orig_Drop orig, Player self)
    {
        orig(self);
        ActionManager.AlertActions(["#PlayerDrop"], self.SceneAs<Level>());
    }

    private static void OnPointBounce(On.Celeste.Player.orig_PointBounce orig, Player self, Vector2 from)
    {
        orig(self, from);
        ActionManager.AlertActions(["#PlayerPointBounce"], self.SceneAs<Level>());
    }

    private static void OnThrow(On.Celeste.Player.orig_Throw orig, Player self)
    {
        orig(self);
        ActionManager.AlertActions(["#PlayerThrow"], self.SceneAs<Level>());
    }

    private static void OnRebound(On.Celeste.Player.orig_Rebound orig, Player self, int direction)
    {
        orig(self, direction);
        ActionManager.AlertActions(["#PlayerRebound"], self.SceneAs<Level>());
    }

    private static void OnTorch(On.Celeste.Torch.orig_OnPlayer orig, Torch self, Player player)
    {
        var was = self.lit;
        orig(self, player);
        if (self.lit && !was)
            ActionManager.AlertActions(["#TorchLit"], self.SceneAs<Level>());
    }

    private static void OnTouchSwitch(On.Celeste.TouchSwitch.orig_TurnOn orig, TouchSwitch self)
    {
        var wasActivated = self.Switch.Activated;
        var wasFinished = self.Switch.Finished;
        orig(self);
        if (self.Switch.Activated && !wasActivated)
            ActionManager.AlertActions(["#TouchSwitchActivated"], self.SceneAs<Level>());
        if (self.Switch.Finished && !wasFinished)
            ActionManager.AlertActions(["#TouchSwitchFinished"], self.SceneAs<Level>());
    }

    private static DashCollisionResults OnDashSwitch(On.Celeste.DashSwitch.orig_OnDashed orig, DashSwitch self, Player player, Vector2 direction)
    {
        var was = self.pressed;
        var res = orig(self, player, direction);
        if (self.pressed && !was)
            ActionManager.AlertActions(["#DashSwitchHit"], self.SceneAs<Level>());
        return res;
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
        bool onGround = self.OnGround();
        if (!self.wasOnGround && onGround)
             ActionManager.AlertActions(["#PlayerLand"], self.level);
        else if (self.wasOnGround && !onGround)
            ActionManager.AlertActions(["#PlayerAirborne"], self.level);
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
        if (self.CurrentBooster is not null) { orig(self); return; }
        var was = self.calledDashEvents;
        orig(self);
        if (self.calledDashEvents && !was)
            ActionManager.AlertActions(["#PlayerDash"], self.level);
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

    private static void OnJump_Action(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        orig(self, particles, playSfx);
        ActionManager.AlertActions(["#PlayerJump"], self.level);
    }
}
