using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System;
using MonoMod.Cil;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/MacabreBooster")]
public class MacabreBooster : Booster
{
    private Vector2 Acceleration;
    private Vector2 SpeedLimit;
    private readonly float LaunchSpeed;
    private readonly bool DuckMusic;

    public MacabreBooster(EntityData data, Vector2 offset, EntityID gid)
        : base(data, offset)
    {
        red = true;
        DuckMusic = data.Bool("DuckMusic", true);
        LaunchSpeed = data.Float("speed", 240);
        Acceleration = new(data.Float("accelX", 0), data.Float("accelY", 0));
        SpeedLimit = new(data.Float("limitX", 500), data.Float("limitY", 500));
        Remove(wiggler);
        Remove(sprite);
        Add(sprite = GFX.SpriteBank.Create("macabreBooster"));
        Add(wiggler = Wiggler.Create(0.5f, 4f, f => { sprite.Scale = Vector2.One * (1f + f * 0.25f); }));
        particleType = new(P_Burst) { Color = Calc.HexToColor("3d0000"), Color2 = Calc.HexToColor("250000") };
    }


    // ----------

    private static readonly MethodInfo PlayerRedDashCoro = typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)!.GetStateMachineTarget()!;
    private static ILHook? PlayerRedDashCoroHook;

    [OnLoad]
    public static void LoadHooks()
    {
        On.Celeste.Player.Bounce += BounceHook;
        On.Celeste.Player.SuperBounce += SuperBounceHook;
        On.Celeste.Player.SideBounce += SideBounceHook;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += ExplodeLaunchILHook;
        On.Celeste.Player.OnCollideH += HCollideHook;
        On.Celeste.Player.OnCollideV += VCollideHook;
        On.Celeste.Player.OnBoundsH += HBoundsHook;
        On.Celeste.Player.OnBoundsV += VBoundsHook;
        On.Celeste.Player.Update += UpdateHook;
        PlayerRedDashCoroHook = new(PlayerRedDashCoro, OnPlayerDashCoro);
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        On.Celeste.Player.Bounce -= BounceHook;
        On.Celeste.Player.SuperBounce -= SuperBounceHook;
        On.Celeste.Player.SideBounce -= SideBounceHook;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= ExplodeLaunchILHook;
        On.Celeste.Player.OnCollideH -= HCollideHook;
        On.Celeste.Player.OnCollideV -= VCollideHook;
        On.Celeste.Player.OnBoundsH -= HBoundsHook;
        On.Celeste.Player.OnBoundsV -= VBoundsHook;
        On.Celeste.Player.Update -= UpdateHook;
        PlayerRedDashCoroHook?.Dispose();
    }

    static bool wasInBooster = false;

    private static void UpdateHook(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (self.LastBooster is not null && self.LastBooster is MacabreBooster && self.LastBooster.Scene != self.Scene)
        {
            self.LastBooster = null;
            self.StateMachine.State = Player.StNormal;
        }
        if (self.LastBooster != null && self.LastBooster is MacabreBooster booster && booster.BoostingPlayer)
        {
            wasInBooster = true;
            if (booster.DuckMusic) Level.PauseSnapshot ??= Audio.CreateSnapshot("snapshot:/pause_menu");
            booster.cannotUseTimer = 0.45f;
            booster.respawnTimer = Booster.RespawnTime;
            self.Speed += booster.Acceleration * Engine.DeltaTime;
            self.Speed.X = Math.Clamp(self.Speed.X, -booster.SpeedLimit.X, booster.SpeedLimit.X);
            self.Speed.Y = Math.Clamp(self.Speed.Y, -booster.SpeedLimit.Y, booster.SpeedLimit.Y);
        }
        else if (wasInBooster)
        {
            wasInBooster = false;
            if (Level.PauseSnapshot is not null)
            {
                Audio.ReleaseSnapshot(Level.PauseSnapshot);
                Level.PauseSnapshot = null;
            }
        }
    }

    public static void HBoundsHook(On.Celeste.Player.orig_OnBoundsH orig, Player self)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster booster)
        {
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end", // works better for both verison
                self.LastBooster.sprite.RenderPosition
            );
            self.Die(-self.Speed.SafeNormalize(Vector2.UnitY));
        }
        else
            orig(self);
    }

    public static void VBoundsHook(On.Celeste.Player.orig_OnBoundsV orig, Player self)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster booster)
        {
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.Die(-self.Speed.SafeNormalize(Vector2.UnitY));
        }
        else
            orig(self);
    }

    public static void HCollideHook(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster)
        {
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.Die(-self.Speed.SafeNormalize(Vector2.UnitY));
        }
        else
            orig(self, data);
    }

    public static void VCollideHook(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster)
        {
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.Die(-self.Speed.SafeNormalize(Vector2.UnitY));
        }
        else
            orig(self, data);
    }

    public static void BounceHook(On.Celeste.Player.orig_Bounce orig, Player self, float fromY)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster)
        {
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.Die(-self.Speed.SafeNormalize(Vector2.UnitY));
        }
        else
        {
            orig(self, fromY);
        }
    }

    public static void SuperBounceHook(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster)
        {
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.Die(-self.Speed.SafeNormalize(Vector2.UnitY));
        }
        else
        {
            orig(self, fromY);
        }
    }

    public static bool SideBounceHook(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster)
        {
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.Die(-self.Speed.SafeNormalize(Vector2.UnitY));
            return true;
        }

        else

        {
            return orig(self, dir, fromX, fromY);
        }
    }

    private static void ExplodeLaunchILHook(ILContext ctx)
    {
        ILCursor cur = new(ctx);
        if (!cur.TryGotoNextBestFit(MoveType.After, 16,
            instr => instr.MatchCall<SlashFx>("Burst"),
            instr => instr.MatchPop()
        ))
            throw new InvalidOperationException("Macabre boosters failed to match IL code for the ExplodeLaunch hook.");
        cur.EmitLdarg0();
        static bool Delegate(Player self)
        {
            if ((self.LastBooster?.BoostingPlayer ?? false) && self.LastBooster is MacabreBooster)
            {
                Audio.Play(
                    "event:/game/05_mirror_temple/redbooster_end",
                    self.LastBooster.sprite.RenderPosition
                );
                self.Die(self.Speed.SafeNormalize(Vector2.UnitY));
            }
            return false;
        }
        cur.EmitDelegate(Delegate);
        ILLabel label = cur.DefineLabel();
        cur.EmitBrfalse(label);
        cur.EmitLdloc0();
        cur.EmitRet();
        cur.MarkLabel(label);
    }

    private static void OnPlayerDashCoro(ILContext il)
    {
        ILCursor cursor = new(il);
        static float ReplaceFloat(float orig, Player self)
        {
            if (self.CurrentBooster is MacabreBooster boost)
                return boost.LaunchSpeed;
            return orig;
        }
        while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 240f))
        {
            cursor.EmitLdloc1();
            cursor.EmitDelegate(ReplaceFloat);
        }
    }
}
