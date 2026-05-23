
using System;
using System.Reflection;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.ScugHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.ScugHelper;

[Tracked]
public class PlayerSeekerComponent(bool playSound = true) : Component(false, false)
{
    static readonly PlayerSeeker _; // For ctrl+click

    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Player.Update += OnUpdate;
        On.Celeste.Player.Die += OnDie;
        On.Celeste.Player.Render += OnRender;
        On.Celeste.Booster.OnPlayer += OnBoosterPlayer;
        On.Celeste.Seeker.CanSeePlayer += OnCanSeePlayer;
        On.Celeste.Seeker.OnAttackPlayer += OnAttackPlayer;
        On.Celeste.Seeker.OnBouncePlayer += OnBouncePlayer;
        On.Celeste.Solid.HasPlayerRider += OnHasPlayerRider;
    }

    private static bool OnHasPlayerRider(On.Celeste.Solid.orig_HasPlayerRider orig, Solid self) {
        if (self is MoveBlock block && self.Scene.Tracker.GetEntity<Player>()?.Get<PlayerSeekerComponent>() is not null)
            return block.triggered;
        return orig(self);
    }

    private static PlayerDeadBody OnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
        if (self?.Components?.Get<PlayerSeekerComponent>() is PlayerSeekerComponent comp && comp.disableDeath) return null;
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static void OnBoosterPlayer(On.Celeste.Booster.orig_OnPlayer orig, Booster self, Player player) {
        if (player.Get<PlayerSeekerComponent>() is not null) return;
        orig(self, player);
    }

    private static void OnAttackPlayer(On.Celeste.Seeker.orig_OnAttackPlayer orig, Seeker self, Player player) {
        if (player.Get<PlayerSeekerComponent>() is not null) return;
        orig(self, player);
    }
    private static void OnBouncePlayer(On.Celeste.Seeker.orig_OnBouncePlayer orig, Seeker self, Player player) {
        if (player.Get<PlayerSeekerComponent>() is not null) return;
        orig(self, player);
    }

    private static bool OnCanSeePlayer(On.Celeste.Seeker.orig_CanSeePlayer orig, Seeker self, Player player) {
        return player?.Get<PlayerSeekerComponent>() is null && orig(self, player);
    }

    private static void OnRender(On.Celeste.Player.orig_Render orig, Player self) {
        if (self.Get<PlayerSeekerComponent>() is PlayerSeekerComponent pleeker)
            pleeker.Render(self);
        else
            orig(self);
    }

    private static void OnUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        if (self.Get<PlayerSeekerComponent>() is PlayerSeekerComponent pleeker)
            pleeker.Update(self);
        else
            orig(self);
    }

    #region Logic

    internal class PlayerSeekerColliderList : ColliderList
    {
        public PlayerSeekerColliderList(Collider col) {
            colliders = [col];
        }
        public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o.Entity);
        public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o.Entity);

        private bool CheckEntity(Entity entity) => entity is not JumpThru;
    }

    private Sprite sprite;
    private Player player;
    private Collider oldCollider;
    private StateMachine oldStateMachine;
    private bool added;

    public override void Added(Entity entity) {
        if (entity is Player self) {
            self.calledDashEvents = true;
            player = self;
            if (playSound) Audio.Play("event:/game/05_mirror_temple/seeker_playercontrolstart");
            self.StateMachine.State = Player.StNormal;
            oldStateMachine = self.StateMachine;
            self.StateMachine.RemoveSelf();
            self.CurrentBooster = self.LastBooster = null;
            self.Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.Play("spotted");
            sprite.OnLastFrame = a =>
            {
                if (a == "flipMouth" || a == "flipEyes")
                    self.Facing = (Facings)(0 - self.Facing);
            };
            oldCollider = self.Collider;
            self.Collider = new PlayerSeekerColliderList(new Hitbox(10f, 10f, -5f, -3f));
            self.Position.Y -= 5;
            added = true;
            var sol = new Solid(Vector2.Zero, 0, 0, false);
            player.TrySquishWiggle(new CollisionData() { Hit = sol, Pusher = sol, TargetPosition = self.Position }, 10, 10);
        } else RemoveSelf();
    }
    public override void Removed(Entity entity) {
        added = false;
        if (entity is Player self) {
            self.Play("event:/game/06_reflection/feather_state_end");
            self.SceneAs<Level>().Particles.Emit(Seeker.P_Attack, 32, self.Center, new(5, 7), Calc.Random.NextAngle());
            self.Collider = oldCollider;
            self.Dashes = 0;
            self.Stamina = 0;
            MidairRefill.MidairDashCount = 0;
            LimboRefill.LimboTimer = 0f;
            OverchargeRefill.OverchargeDashCount = 0;
            self.RefillDash();
            self.RefillStamina();
            self.Remove(sprite);
            self.Add(oldStateMachine);
            self.StateMachine.State = Player.StNormal;
            self.CurrentBooster = self.LastBooster = null;
        }
    }

    float trailTimerA;
    float trailTimerB;
    internal bool disableDeath;
    readonly Seeker dummySeeker = new(Vector2.Zero, []);

    bool dreamDashing;
    bool wasDreamDashing;

    public void CreateTrail(Player self) => CreateTrail(self, Seeker.TrailColor);

    public void CreateTrail(Player self, Color color) {
        Vector2 scale = sprite.Scale;
        sprite.Scale.X *= (float)self.Facing;
        dummySeeker.Position = self.Position;
        dummySeeker.Speed = self.Speed;
        dummySeeker.sprite = sprite.CreateClone();
        dummySeeker.Depth = -100000;
        TrailManager.Add(dummySeeker, sprite.Scale, color, 0.5f);
        sprite.Scale = scale;
    }

    private static readonly FieldInfo FramesAlive = typeof(Player).GetField("framesAlive", BindingFlags.NonPublic | BindingFlags.Instance);

    private void Update(Player self) {
        if (!added) return;
        if (!self.Dead) FramesAlive.SetValue(self, (int)FramesAlive.GetValue(self) + 1);
        var scene = self.Scene;
        foreach (var barrier in scene.Tracker.GetEntities<SeekerBarrier>())
            barrier.Collidable = true;
        self.JustRespawned = false;
        self.noWindTimer = 0f;
        wasDreamDashing = dreamDashing;
        self.OnSafeGround = true;
        self.Components.Update();
        self.starFlyTimer = 0f;
        self.starFlyLoopSfx?.Stop();
        LimboRefill.LimboTimer = 0f;
        sprite.FlipY = self.Sprite.FlipY;

        Vector2 cameraPos = self.level.Camera.Position;
        Vector2 cameraTarget = self.CameraTarget;
        self.level.Camera.Position = cameraPos + (cameraTarget - cameraPos) * (1f - (float)Math.Pow(0.01f, Engine.DeltaTime));

        disableDeath = true;
        foreach (PlayerCollider coll in scene.Tracker.GetComponents<PlayerCollider>()) {
            coll.Check(self);
        }
        disableDeath = false;

        foreach (Trigger entity in player.Scene.Tracker.GetEntities<Trigger>()) {
            if (player.CollideCheck(entity)) {
                if (!entity.Triggered) {
                    entity.Triggered = true;
                    player.triggersInside.Add(entity);
                    entity.OnEnter(player);
                }

                entity.OnStay(player);
            } else if (entity.Triggered) {
                player.triggersInside.Remove(entity);
                entity.Triggered = false;
                entity.OnLeave(player);
            }
        }

        self.PreviousPosition = self.Position;

        Level level = scene as Level;
        self.StateMachine.state = Player.StNormal;

        sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
        sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
        if (dreamDashing) {
            self.NaiveMove(self.Speed * Engine.DeltaTime);
            if (self.Scene.OnInterval(Settings.Instance.DisableFlashes ? 0.3f : 0.04f))
                RandomizeDreamColor();
            if (self.Scene.OnInterval(0.04f))
                CreateTrail(self, RandomDreamColor);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            Vector2 position = self.Position;

            DreamBlock dreamBlock = self.CollideFirst<DreamBlock>();
            if (dreamBlock == null) {
                if (self.DreamDashedIntoSolid()) {
                    if (SaveData.Instance.Assists.Invincible) {
                        self.Position = position;
                        self.Speed *= -1f;
                        self.Play("event:/game/general/assist_dreamblockbounce");
                    } else
                        self.Die(Vector2.Zero);
                } else {
                    dreamDashing = false;

                    player.Stop(player.dreamSfxLoop);
                    player.Play("event:/char/madeline/dreamblock_exit");
                }
            } else {
                self.dreamBlock = dreamBlock;

                if (level.OnInterval(0.02f)) {
                    DisplacementRenderer.Burst burst = level.Displacement.AddBurst(self.Center, 0.3f, 0f, 40f);
                    burst.WorldClipCollider = dreamBlock.Collider;
                    burst.WorldClipPadding = 2;
                }
            }
        } else if (self.dashAttackTimer > 0f) {
            //self.StateMachine.state = Player.StDash;
            self.Speed = Calc.Approach(self.Speed, Vector2.Zero, 800f * Engine.DeltaTime);
            self.dashAttackTimer -= Engine.DeltaTime;
            if (self.dashAttackTimer <= 0f)
                sprite.Play("spotted");

            if (trailTimerA > 0f) {
                trailTimerA -= Engine.DeltaTime;
                if (trailTimerA <= 0f)
                    CreateTrail(self);
            }

            if (trailTimerB > 0f) {
                trailTimerB -= Engine.DeltaTime;
                if (trailTimerB <= 0f)
                    CreateTrail(self);
            }

            if (scene.OnInterval(0.04f)) {
                Vector2 vector = self.Speed.SafeNormalize();
                self.SceneAs<Level>().Particles.Emit(Seeker.P_Attack, 2, self.Position + vector * 4f, Vector2.One * 4f, vector.Angle());
            }
        } else {
            Vector2 vector2 = level.InCutscene ? Vector2.UnitY : Input.Aim.Value.SafeNormalize();
            float mul = self.SwimCheck() ? 0.4f : 1f;
            self.Speed += vector2 * 600f * Engine.DeltaTime * mul;
            float num = self.Speed.Length();
            if (num > 120f) {
                num = Calc.Approach(num, 120f, Engine.DeltaTime * 700f);
                self.Speed = self.Speed.SafeNormalize(num);
            }

            if (vector2.Y == 0f)
                self.Speed.Y = Calc.Approach(self.Speed.Y, 0f, 400f * Engine.DeltaTime);

            if (vector2.X == 0f)
                self.Speed.X = Calc.Approach(self.Speed.X, 0f, 400f * Engine.DeltaTime);

            int num2 = Math.Sign((int)self.Facing);
            int num3 = Math.Sign(self.Speed.X);
            if (num3 != 0 && num2 != num3 && Math.Sign(Input.Aim.Value.X) == Math.Sign(self.Speed.X) && Math.Abs(self.Speed.X) > 20f && sprite.CurrentAnimationID != "flipMouth" && sprite.CurrentAnimationID != "flipEyes")
                sprite.Play("flipMouth");

            if (Input.Dash.Pressed && !level.InCutscene)
                Dash(self, Input.Aim.Value.EightWayNormal());
        }
        self.LastBooster = self.CurrentBooster = null;
        if (!self.calledDashEvents) {
            self.calledDashEvents = true;

            SaveData.Instance.TotalDashes++;
            level.Session.Dashes++;
            Stats.Increment(Stat.DASHES);

            if (self.SwimCheck())
                self.Play("event:/char/madeline/water_dash_gen");

            foreach (DashListener component in self.Scene.Tracker.GetComponents<DashListener>())
                component.OnDash?.Invoke(self.DashDir);
        }

        if (!wasDreamDashing) {
            self.MoveH(self.Speed.X * Engine.DeltaTime, OnCollideH);
            self.MoveV(self.Speed.Y * Engine.DeltaTime, OnCollideV);
        }

        // Actor.Update
        self.LiftSpeed = Vector2.Zero;
        if (self.liftSpeedTimer > 0f) {
            self.liftSpeedTimer -= Engine.DeltaTime;
            if (self.liftSpeedTimer <= 0f) {
                self.lastLiftSpeed = Vector2.Zero;
            }
        }

        level.EnforceBounds(self);
        self.wasOnGround = false;
        
        foreach (var barrier in scene.Tracker.GetEntities<SeekerBarrier>())
            barrier.Collidable = false;
    }

    static readonly Color[] colors = [Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"), Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"), Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64")];
    private static Color RandomDreamColor;
    private static void RandomizeDreamColor() {
        RandomDreamColor = Calc.Random.Choose(colors);
    }

    private void OnSeekerCollide(CollisionData data) {

        if (player.dashAttackTimer <= 0f) {
            if (data.Direction.X != 0f) {
                player.Speed.X = 0f;
            }

            if (data.Direction.Y != 0f) {
                player.Speed.Y = 0f;
            }

            return;
        }

        data.Hit.OnDashCollide?.Invoke(player, data.Direction);
        if (data.Hit is MoveBlock block)
            block.triggered = true;

        if (player.DreamDashCheck(data.Direction)) {
            dreamDashing = true;
            player.Speed = player.Speed.SafeNormalize(Vector2.UnitY) * 400f;
            if (player.dreamSfxLoop == null)
                player.Add(player.dreamSfxLoop = new SoundSource());
            player.Play("event:/char/madeline/dreamblock_enter");
            player.Loop(player.dreamSfxLoop, "event:/char/madeline/dreamblock_travel");
            return;
        }

        float direction;
        Vector2 position;
        Vector2 positionRange;
        if (data.Direction.X > 0f) {
            direction = MathF.PI;
            position = new Vector2(player.Right, player.Y);
            positionRange = Vector2.UnitY * 4f;
        } else if (data.Direction.X < 0f) {
            direction = 0f;
            position = new Vector2(player.Left, player.Y);
            positionRange = Vector2.UnitY * 4f;
        } else if (data.Direction.Y > 0f) {
            direction = -MathF.PI / 2f;
            position = new Vector2(player.X, player.Bottom);
            positionRange = Vector2.UnitX * 4f;
        } else {
            direction = MathF.PI / 2f;
            position = new Vector2(player.X, player.Top);
            positionRange = Vector2.UnitX * 4f;
        }

        player.SceneAs<Level>().Particles.Emit(Seeker.P_HitWall, 12, position, positionRange, direction);
        if (data.Hit is SeekerBarrier) {
            (data.Hit as SeekerBarrier).OnReflectSeeker();
            Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", player.Position);
        } else {
            Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", player.Position);
        }

        if (data.Direction.X != 0f) {
            player.Speed.X *= -0.8f;
            sprite.Scale = new Vector2(0.6f, 1.4f);
        } else if (data.Direction.Y != 0f) {
            player.Speed.Y *= -0.8f;
            sprite.Scale = new Vector2(1.4f, 0.6f);
        }

        if (data.Hit is TempleCrackedBlock) {
            Celeste.Freeze(0.15f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            (data.Hit as TempleCrackedBlock).Break(player.Position);
        }

        if (data.Hit is DashSwitch dashSwitch) {
            dashSwitch.OnDashed(null, Vector2.UnitX * Math.Sign(player.DashDir.X));
            dashSwitch.OnDashed(null, Vector2.UnitY * Math.Sign(player.DashDir.Y));
        }
    }

    private void OnCollideH(CollisionData data) {
        OnSeekerCollide(data);
    }

    private void OnCollideV(CollisionData data) {
        OnSeekerCollide(data);
    }

    public void Dash(Player self, Vector2 dir) {
        if (self.dashAttackTimer <= 0f) {
            CreateTrail(self);
            trailTimerA = 0.1f;
            trailTimerB = 0.25f;
        }

        self.calledDashEvents = false;
        self.dashAttackTimer = 0.3f;
        self.DashDir = dir;
        if (self.DashDir == Vector2.Zero)
            self.DashDir.X = Math.Sign((int)self.Facing);

        if (self.DashDir.X != 0f) {
            self.Facing = (Facings)Math.Sign(self.DashDir.X);
        }

        self.Speed = self.DashDir * 400f;
        sprite.Play("attacking");
        self.SceneAs<Level>().DirectionalShake(self.DashDir);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        Audio.Play("event:/game/05_mirror_temple/seeker_dash", self.Position);
        if (self.DashDir.X == 0f)
            sprite.Scale = new Vector2(0.6f, 1.4f);
        else
            sprite.Scale = new Vector2(1.4f, 0.6f);
    }

    private void Render(Player self) {
        if (!added) return;
        if (dreamDashing) return;
        if (!SaveData.Instance.Assists.InvisibleMotion || !(self.Speed.LengthSquared() > 100f)) {
            Vector2 scale = sprite.Scale;
            sprite.Scale.X *= (float)self.Facing;
            sprite.Render();
            sprite.Scale = scale;
        }
    }

    #endregion

    [Command("pleeker", "Toggles the player seeker component.")]
    internal static void Pleeker() {
        if (Engine.Scene.Tracker.GetEntity<Player>() is not Player player) return;
        if (player.Get<PlayerSeekerComponent>() is PlayerSeekerComponent pleeker) player.Remove(pleeker);
        else player.Add(new PlayerSeekerComponent());
    }
}
