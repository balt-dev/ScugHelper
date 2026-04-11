using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System;
using MonoMod.Cil;

[Tracked]
[CustomEntity("ScugHelper/PinballBooster")]
public class PinballBooster : Booster
{
    private EntityID ID;
    private Vector2 Acceleration;
    private Vector2 SpeedLimit;

    public PinballBooster(EntityData data, Vector2 offset, EntityID gid)
        : base(data, offset)
    {
        ID = gid;
        Acceleration = new(data.Float("accelX", 0), data.Float("accelY", 0));
        SpeedLimit = new(data.Float("limitX", 500), data.Float("limitY", 500));
        Remove(wiggler);
        Remove(sprite);
        Add(sprite = GFX.SpriteBank.Create(red ? "pinballBoosterRed" : "pinballBooster"));
        Add(wiggler = Wiggler.Create(0.5f, 4f, f => { sprite.Scale = Vector2.One * (1f + f * 0.25f); }));
    }


    // ----------


    public static void LoadHooks()
    {
        On.Celeste.Player.Bounce += BounceHook;
        On.Celeste.Player.SuperBounce += SuperBounceHook;
        On.Celeste.Player.SideBounce += SideBounceHook;
        On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += ExplodeLaunchOnHook;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += ExplodeLaunchILHook;
        On.Celeste.Player.OnCollideH += HCollideHook;
        On.Celeste.Player.OnCollideV += VCollideHook;
        On.Celeste.Player.OnBoundsH += HBoundsHook;
        On.Celeste.Player.OnBoundsV += VBoundsHook;
        On.Celeste.Player.Update += UpdateHook;
        On.Celeste.Booster.Update += UpdateHook;
    }

    public static void UnloadHooks()
    {
        On.Celeste.Player.Bounce -= BounceHook;
        On.Celeste.Player.SuperBounce -= SuperBounceHook;
        On.Celeste.Player.SideBounce -= SideBounceHook;
        On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= ExplodeLaunchOnHook;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= ExplodeLaunchILHook;
        On.Celeste.Player.OnCollideH -= HCollideHook;
        On.Celeste.Player.OnCollideV -= VCollideHook;
        On.Celeste.Player.OnBoundsH -= HBoundsHook;
        On.Celeste.Player.OnBoundsV -= VBoundsHook;
        On.Celeste.Player.Update -= UpdateHook;
        On.Celeste.Booster.Update -= UpdateHook;
    }

    private static void UpdateHook(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (self.LastBooster != null && self.LastBooster is PinballBooster booster && booster.BoostingPlayer) {
            booster.cannotUseTimer = 0.45f;
            booster.respawnTimer = Booster.RespawnTime;
            self.Speed += booster.Acceleration * Engine.DeltaTime;
            self.Speed.X = Math.Clamp(self.Speed.X, -booster.SpeedLimit.X, booster.SpeedLimit.X);
            self.Speed.Y = Math.Clamp(self.Speed.Y, -booster.SpeedLimit.Y, booster.SpeedLimit.Y);
        }
    }

    private static void UpdateHook(On.Celeste.Booster.orig_Update orig, Booster self)
    {
        orig(self);
        if (self is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce)
        {
            self.sprite.Scale.X = Calc.Approach(self.sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
            self.sprite.Scale.Y = Calc.Approach(self.sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);
        }
    }

    public static void HBoundsHook(On.Celeste.Player.orig_OnBoundsH orig, Player self)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
        {
            self.Speed.X = -self.Speed.X;
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end", // works better for both verison
                self.LastBooster.sprite.RenderPosition
            );
            self.LastBooster.sprite.Scale = new Vector2(1.0f / ScugHelperModule.Settings.PinballBumperSquash, ScugHelperModule.Settings.PinballBumperSquash);
        }
        else
            orig(self);
    }

    public static void VBoundsHook(On.Celeste.Player.orig_OnBoundsV orig, Player self)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
        {
            self.Speed.Y = -self.Speed.Y;
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.LastBooster.sprite.Scale = new Vector2(ScugHelperModule.Settings.PinballBumperSquash, 1.0f / ScugHelperModule.Settings.PinballBumperSquash);
        }
        else
            orig(self);
    }

    public static void HCollideHook(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
        {
            if (self.StateMachine.State == Player.StDash || self.StateMachine.State == Player.StRedDash)
            {
                if (self.onGround && self.DuckFreeAt(self.Position + Vector2.UnitX * Math.Sign(self.Speed.X)))
                {
                    self.Ducking = true;
                    return;
                }
                else if (self.Speed.Y == 0 && self.Speed.X != 0)
                {
                    for (int i = 1; i <= Player.DashCornerCorrection; i++)
                    {
                        for (int j = 1; j >= -1; j -= 2)
                        {
                            if (!self.CollideCheck<Solid>(self.Position + new Vector2(Math.Sign(self.Speed.X), i * j)))
                            {
                                self.MoveVExact(i * j);
                                self.MoveHExact(Math.Sign(self.Speed.X));
                                return;
                            }
                        }
                    }
                }
            }

            if (self.wallSpeedRetentionTimer <= 0)
            {
                self.wallSpeedRetained = self.Speed.X;
                self.wallSpeedRetentionTimer = Player.WallSpeedRetentionTime;
            }

            if (data.Hit != null && data.Hit.OnCollide != null)
                data.Hit.OnCollide(data.Direction);
            self.Speed.X = -self.Speed.X;
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.LastBooster.sprite.Scale = new Vector2(1.0f / ScugHelperModule.Settings.PinballBumperSquash, ScugHelperModule.Settings.PinballBumperSquash);
        }
        else
            orig(self, data);
    }

    public static void VCollideHook(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
        {
            if (self.Speed.Y > 0)
            {
                //Dash corner correction
                if ((self.StateMachine.State == Player.StDash || self.StateMachine.State == Player.StRedDash) && !self.dashStartedOnGround)
                {
                    if (self.Speed.X <= 0)
                    {
                        for (int i = -1; i >= -Player.DashCornerCorrection; i--)
                        {
                            if (!self.OnGround(self.Position + new Vector2(i, 0)))
                            {
                                self.MoveHExact(i);
                                self.MoveVExact(1);
                                return;
                            }
                        }
                    }

                    if (self.Speed.X >= 0)
                    {
                        for (int i = 1; i <= Player.DashCornerCorrection; i++)
                        {
                            if (!self.OnGround(self.Position + new Vector2(i, 0)))
                            {
                                self.MoveHExact(i);
                                self.MoveVExact(1);
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                if (self.Speed.Y < 0)
                {
                    //Corner Correction
                    {
                        if (self.Speed.X <= 0)
                        {
                            for (int i = 1; i <= Player.UpwardCornerCorrection; i++)
                            {
                                if (!self.CollideCheck<Solid>(self.Position + new Vector2(-i, -1)))
                                {
                                    self.Position += new Vector2(-i, -1);
                                    return;
                                }
                            }
                        }

                        if (self.Speed.X >= 0)
                        {
                            for (int i = 1; i <= Player.UpwardCornerCorrection; i++)
                            {
                                if (!self.CollideCheck<Solid>(self.Position + new Vector2(i, -1)))
                                {
                                    self.Position += new Vector2(i, -1);
                                    return;
                                }
                            }
                        }
                    }

                    if (self.varJumpTimer < Player.VarJumpTime - Player.CeilingVarJumpGrace)
                        self.varJumpTimer = 0;
                }
            }

            if (data.Hit != null && data.Hit.OnCollide != null)
                data.Hit.OnCollide(data.Direction);
            self.Speed.Y = -self.Speed.Y;
            Audio.Play(
                "event:/game/05_mirror_temple/redbooster_end",
                self.LastBooster.sprite.RenderPosition
            );
            self.LastBooster.sprite.Scale = new Vector2(ScugHelperModule.Settings.PinballBumperSquash, 1.0f / ScugHelperModule.Settings.PinballBumperSquash);
        }
        else
            orig(self, data);
    }

    public static void BounceHook(On.Celeste.Player.orig_Bounce orig, Player self, float fromY)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
        {
            self.MoveV(fromY - self.CenterY);
            Vector2 normalized = Vector2.Normalize(self.Speed) * Player.DashSpeed;
            self.Speed.Y = -Math.Max(Math.Abs(self.Speed.Y), normalized.Y);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            self.LastBooster.sprite.Scale = new Vector2(ScugHelperModule.Settings.PinballBumperSquash, 1.0f / ScugHelperModule.Settings.PinballBumperSquash);
        }
        else
        {
            orig(self, fromY);
        }
    }

    public static void SuperBounceHook(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
        {
            self.MoveV(fromY - self.CenterY);
            self.Speed.Y = -Math.Max(Math.Abs(self.Speed.Y), Player.DashSpeed);
            self.level.DirectionalShake(-Vector2.UnitY, 0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            self.LastBooster.sprite.Scale = new Vector2(ScugHelperModule.Settings.PinballBumperSquash, 1.0f / ScugHelperModule.Settings.PinballBumperSquash);
        }
        else
        {
            orig(self, fromY);
        }
    }

    public static bool SideBounceHook(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY)
    {
        if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
        {
            if (Math.Abs(self.Speed.X) > 240f && Math.Sign(self.Speed.X) == dir) return false;
            self.MoveV(Calc.Clamp(fromY - self.CenterY, -4f, 4f));
            if (dir > 0) self.MoveH(fromX - self.Left);
            else if (dir < 0) self.MoveH(fromX - self.Right);
            self.Speed.X = dir * Math.Max(Math.Abs(self.Speed.X), Player.DashSpeed);
            self.level.DirectionalShake(Vector2.UnitX * dir, 0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            self.LastBooster.sprite.Scale = new Vector2(1.0f / ScugHelperModule.Settings.PinballBumperSquash, ScugHelperModule.Settings.PinballBumperSquash);
            return true;
        }

        else

        {
            return orig(self, dir, fromX, fromY);
        }
    }

    private static Vector2 smuggledLocal = new();


    private static Vector2 ExplodeLaunchOnHook(On.Celeste.Player.orig_ExplodeLaunch_Vector2_bool_bool orig, Player self, Vector2 from, bool snapUp, bool sidesOnly)
    {
        smuggledLocal = self.Speed;
        return orig(self, from, snapUp, sidesOnly);
    }

    private static void ExplodeLaunchILHook(ILContext ctx)
    {
        ILCursor cur = new(ctx);
        if (!cur.TryGotoNext(MoveType.After,
            instr => instr.MatchCall<SlashFx>("Burst"),
            instr => instr.MatchPop()
        ))
            throw new InvalidOperationException("Pinball bumpers failed to match IL code for the ExplodeLaunch hook.");
        cur.EmitLdarg0();
        static bool Delegate (Player self) {
            if ((self.LastBooster?.BoostingPlayer ?? false) && (self.LastBooster is PinballBooster || ScugHelperModule.Settings.AllBoostersBounce))
            {
                self.LastBooster.sprite.Scale = new Vector2(ScugHelperModule.Settings.PinballBumperSquash, ScugHelperModule.Settings.PinballBumperSquash);
                self.Speed = Vector2.Normalize(self.Speed) * Math.Max(self.Speed.Length(), smuggledLocal.Length());
                return true;
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
}
