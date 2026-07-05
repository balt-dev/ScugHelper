
using System;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Spear")]
public class Spear : Actor, IHasSpeed {
    public Vector2 Speed { get => speed; set => speed = value; }

    public const int StIdle = 0;
    public const int StSidethrow = 1;
    public const int StDownthrow = 2;
    public const int StBonk = 3;

    public int State { get => StateMachine.State; protected set => StateMachine.State = value; }
    public bool PointingDown => Math.Abs(Sprite.Rotation - Math.PI / 2) < 0.04f;

    internal readonly string SpritePath;
    internal Vector2 speed;
    internal Image Sprite;
    internal StateMachine StateMachine;
    internal PufferCollider PufferCollider;
    internal SeekerCollider SeekerCollider;
    internal EntityID OriginID;

    internal readonly Hitbox PickupHitbox = new(16, 16, -8, -8);
    internal readonly Hitbox SidethrowHitbox = new(24, 2, -12, -1);
    internal readonly Hitbox DownthrowHitbox = new(2, 24, -1, -12);

    public Spear(EntityData data, Vector2 offset, EntityID id) : this(data.String("Sprite", ""), data.Position + offset, StIdle, id, false) { wasOnGround = true; }
    public Spear(string sprite, Vector2 position, int state, EntityID originID, bool killIdle = false): base(position) {
        this.killIdle = killIdle;
        OriginID = originID;
        Depth = 50;
        SpritePath = sprite;
        Add(Sprite = new(GFX.Game[sprite]));
        Sprite.CenterOrigin();
        Add(PufferCollider = new PufferCollider(OnPuffer));
        Add(SeekerCollider = new SeekerCollider(OnSeeker));
        Add(new SpringCollider(OnSpring));
        Add(new TouchSwitchCollider(static ts => ts.TurnOn()));
        StateMachine = new StateMachine(4);
        StateMachine.SetCallbacks(StIdle, IdleUpdate, null, IdleBegin, IdleEnd);
        StateMachine.SetCallbacks(StSidethrow, SidethrowUpdate, null, SidethrowBegin);
        StateMachine.SetCallbacks(StDownthrow, () => StDownthrow, null, DownthrowBegin, null);
        StateMachine.SetCallbacks(StBonk, BonkUpdate, null, BonkBegin, BonkEnd);
        Add(StateMachine);
        State = state;
        Collider ??= SidethrowHitbox;
    }

    internal const float Gravity = 700f;
    internal const float RotationCoefficient = 0.112f;
    internal const float BonkCoefficient = -0.4f;
    internal const float DownSpeedLimit = 500f;
    internal const float Friction = 900f;
    internal const float BonkVerical = -100;

    internal float RotationSpeed;
    internal bool onGround;
    internal bool wasOnGround = false;
    internal int oldState = StIdle;
    internal bool killIdle;

    public override void Update() {
        base.Update();
        if (Scene is not Level level) return;
        if (killIdle && State == StIdle) {
            Logger.Log(nameof(ScugHelper), "Killing idle...");
            RemoveSelf();
            return;
        }

        var movedPos = Position + (Speed * Engine.DeltaTime);
        if (level.Bounds.Left > movedPos.X || movedPos.X > level.Bounds.Right) {
            if (State == StSidethrow) {
                var sol = new Solid(Vector2.Zero, 0, 0, false);
                OnCollideH(new CollisionData() { Hit = sol, Pusher = sol, TargetPosition = Position, Direction = new(Math.Sign(Speed.X), Math.Sign(Speed.Y)) });
            } else {
                Audio.Play("event:/scughelper/objects/spear/bounce", Position);
                speed.X *= -1;
            }
        }
        if (movedPos.Y > level.Bounds.Bottom + 32) { RemoveSelf(); return; }

        MoveH(speed.X * Engine.DeltaTime, OnCollideH);
        MoveV(speed.Y * Engine.DeltaTime, OnCollideV);

        onGround = OnGround();
        if (onGround && !wasOnGround && State == StBonk) {
            Audio.Play("event:/scughelper/objects/spear/bounce", Position);
            State = StIdle;
        } else if (!onGround && wasOnGround) {
            State = StBonk;
        }
        wasOnGround = onGround;
        if (onGround) {
            if (State != StDownthrow) {
                speed.Y = 0;
                speed.X = Calc.Approach(speed.X, 0, Friction * Engine.DeltaTime);
                State = StIdle;
            }
        }
        else speed.Y += Gravity * Engine.DeltaTime;
        if (speed.Y > DownSpeedLimit)
            speed.Y = Calc.Approach(speed.Y, DownSpeedLimit, Friction * Engine.DeltaTime);

        oldState = State;

        Sprite.Position.Y = PointingDown && State == StIdle ? 2 : 0;
    }

    private void OnCollideH(CollisionData data) {
        if (Speed.X == 0) return;
        if (State == StBonk) { RotationSpeed = 0; speed.X = 0; return; }
        if (State == StSidethrow && TryBreak(data)) return;
        if (data.Direction.X != Math.Sign(speed.X)) return;
        data.Pusher ??= new Solid(Vector2.Zero, 0, 0, false);
        TrySquishWiggle(data, 24, 0);
        Logger.Log(nameof(ScugHelper), "OnCollideH");
        Audio.Play("event:/scughelper/objects/spear/bounce", Position);
        speed.Y = BonkVerical;
        speed.X *= BonkCoefficient;
        RotationSpeed = speed.Length() * RotationCoefficient;
        State = StBonk;
    }

    private void OnCollideV(CollisionData data) {
        if (speed.Y == 0) return;
        if (data.Direction.Y < 0) { speed.Y = 0; return; }
        if (State == StDownthrow) {
            if (TryBreak(data)) return;
            data.Pusher ??= new Solid(Vector2.Zero, 0, 0, false);
            TrySquishWiggle(data, 0, 24);
            Audio.Play("event:/scughelper/objects/spear/stick", Position);
        }
        State = StIdle;
    }

    private bool TryBreak(CollisionData data) {
        switch (data.Hit) {
            case DashSwitch button: {
                button.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
                return true;
            }
            case DashBlock block: {
                block.Break(Position, Vector2.UnitX * Math.Sign(Speed.X), true, true);
                return true;
            }
            case FastfallBlock block: {
                block.Break(Vector2.UnitX * Math.Sign(Speed.X), true, true);
                return true;
            }
            case Platform platform when platform.OnDashCollide is not null && Scene.Tracker.GetEntity<Player>() is Player player: {
                Vector2 dir = State == StSidethrow ? Vector2.UnitX * Math.Sign(Speed.X) : Vector2.UnitY * Math.Sign(Speed.Y);
                ScugHelperModule.PreventDeath = true;
                platform.OnDashCollide(player, dir);
                ScugHelperModule.PreventDeath = false;
                return false;
            }
            default: return false;
        }
    }

    private void OnPuffer(Puffer puffer) {
        if (State == StSidethrow || State == StDownthrow) {
            if (!(puffer.state == Puffer.States.Gone || !(puffer.cantExplodeTimer <= 0f))) {
                puffer.Explode();
                puffer.GotoGone();

                speed.Y = BonkVerical;
                speed.X *= -1;
                RotationSpeed = speed.Length() * RotationCoefficient;
                State = StBonk;
            }
        }
    }

    private void OnSeeker(Seeker seeker) {
        if (State == StSidethrow || State == StDownthrow) {
            ScugHelperModule.KillSeeker(seeker);
            speed.Y = BonkVerical;
            speed.X *= -1;
            RotationSpeed = speed.Length() * RotationCoefficient;
            State = StBonk;
        }
    }

    internal const float MinimumSpringSpeed = 100f;
    private void OnSpring(Spring spring) {
        switch (spring.Orientation) {
            case Spring.Orientations.Floor: {
                RotationSpeed = speed.Length() * RotationCoefficient;
                State = StBonk;
                speed.Y = -Math.Max(Math.Abs(speed.Y), MinimumSpringSpeed);
                break;
            }
            case Spring.Orientations.WallRight:
            case Spring.Orientations.WallLeft: {
                speed.X = Math.Max(Math.Abs(speed.X), MinimumSpringSpeed) * (spring.Orientation is Spring.Orientations.WallRight ? -1 : 1);
                if (State != StSidethrow) {
                    State = StBonk;
                    RotationSpeed = speed.Length() * RotationCoefficient;
                }
                break;
            }
        }
        spring.BounceAnimate();
    }

    private void OnPickup(Player player) {
        RemoveSelf();
        Input.Grab.ConsumeBuffer();
        Audio.Play("event:/scughelper/objects/spear/pickup");
        player.Add(new SpearComponent(SpritePath, OriginID));
    }

#region States
    internal void IdleBegin() {
        Add(new SpearPickupComponent(PickupHitbox.Bounds, Vector2.UnitY * -10f, OnPickup));
    }
    internal int IdleUpdate() {
        if (Get<SpearPickupComponent>() is null)
            Add(new SpearPickupComponent(PickupHitbox.Bounds, Vector2.UnitY * -10f, OnPickup));
        if (PointingDown) Collider = DownthrowHitbox;
        else Collider = SidethrowHitbox;
        return StIdle;
    }

    internal void IdleEnd() {
        Components.RemoveAll<SpearPickupComponent>();
    }
    internal void SidethrowBegin() {
        Collider = SidethrowHitbox;
        Sprite.Rotation = 0;
        Sprite.FlipX = Speed.X < 0;
    }
    internal int SidethrowUpdate() {
        if (Scene is not Level level) return StSidethrow;
        Sprite.FlipX = Speed.X < 0;
        return StSidethrow;
    }
    internal void DownthrowBegin() {
        Collider = DownthrowHitbox;
        Sprite.Rotation = MathF.PI / 2;
    }
    internal void BonkBegin() {
        Collider = PointingDown && RotationSpeed == 0 ? DownthrowHitbox : SidethrowHitbox;
    }
    internal int BonkUpdate() {
        Sprite.Rotation += RotationSpeed * Engine.DeltaTime;
        return OnGround() ? StIdle : StBonk;
    }
    internal void BonkEnd() {
        Logger.Log(nameof(ScugHelper), "BonkEnd");
        Audio.Play("event:/scughelper/objects/spear/bounce");
        if (!PointingDown || RotationSpeed != 0)
            Sprite.Rotation = 0;
    }
    #endregion

    static float RefillCooldown;

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Player.Update += OnPlayerUpdate;
        On.Celeste.Player.RefillDash += OnPlayerRefillDash;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Player.Update -= OnPlayerUpdate;
        On.Celeste.Player.RefillDash -= OnPlayerRefillDash;
    }
    
    private static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
        orig(self);
        RefillCooldown -= Engine.DeltaTime;
    }

    internal static bool ShouldHaveInfiniteSpears(Level? level = null) {
        level ??= Engine.Scene as Level;
        return (level?.Session.GetFlag("ScugHelper.InfiniteSpears") ?? false) || ScugHelperModule.Settings.InfiniteSpears;
    }
    
    private static bool OnPlayerRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self) {
        if (ShouldHaveInfiniteSpears(self.level) && self.Get<SpearComponent>() is null && RefillCooldown <= 0f) {
            RefillCooldown = self.level.Session.GetSlider("ScugHelper.InfiniteSpearCooldown");
            self.Add(new SpearComponent("objects/ScugHelper/spear/metal", new EntityID("", Calc.Random.Range(int.MinValue, -1)), true));
        }
        return orig(self);
    }
    
    [Command("spear", "Gives you a spear.")]
    internal static void CmdSpear() {
        if (
            Engine.Scene is Level level &&
            level.Tracker.GetEntity<Player>() is Player player &&
            player.Get<SpearComponent>() is null
        ) player.Add(new SpearComponent("objects/ScugHelper/spear/metal", new EntityID("", Calc.Random.Range(int.MinValue, -1)), false));
    }
}

internal class SpearComponent(string sprite, EntityID originID, bool killIdle = false): Component(true, true) {
    internal Player Player => (Entity as Player)!;
    internal const float AddedSpeed = 200f;
    internal const float MinimumSpeed = 120f;
    internal const float ThrowThreshold = 80f;
    internal const float HoldMaxTime = 0.25f;
    internal const float BackboostStrength = 0.6f;
    internal float HoldTimer = 0f;
    internal bool FirstGrab = true;
    internal bool Done = false;
    internal MTexture? Icon;
    public override void Added(Entity entity) {
        Icon = GFX.Game["objects/ScugHelper/spear/icon"];
        base.Added(entity);
    }
    public override void Update() {
        if (Player.Get<SpearComponent>() != this) {
            var spear = new Spear(sprite, Player.TopCenter.Round(), Spear.StBonk, originID, killIdle);
            Scene.Add(spear);
            RemoveSelf();
            return;
        }
        if (Done) return;
        base.Update();
        if (FirstGrab) {
            if (Input.Grab.Check) return;
            FirstGrab = false;
        }
        if (Player.StateMachine.State != Player.StNormal) HoldTimer = 100f;
        if (Input.Grab.Check) HoldTimer += Engine.DeltaTime;
        else {
            if (HoldTimer > 0f && HoldTimer < HoldMaxTime) {
                var aim = Input.Aim.Value;
                float aimX = (int)Player.Facing;
                float aimY = aim.Y;
                Vector2 pSpeed = Player.AdjustedSpeed();
                Logger.Log(nameof(ScugHelper), $"{aimX}, {aimY}");
                if (aimY > 0.8f && Math.Abs(aim.X) < 0.1f) {
                    var spear = new Spear(sprite, Player.TopCenter.Round(), Spear.StDownthrow, originID, killIdle);
                    spear.speed.Y = pSpeed.Y + 400f;
                    if (!Player.onGround)
                        Player.SetAdjustedSpeed(pSpeed.X, pSpeed.Y - 200f);
                    Scene.Add(spear);
                } else {
                    var spear = new Spear(sprite, Player.TopCenter.Round(), Spear.StSidethrow, originID, killIdle);
                    var speedX = Math.Max(Math.Abs(Player.Speed.X), MinimumSpeed) * (Player.Speed.X == 0 ? aimX : Math.Sign(Player.Speed.X));
                    Logger.Log(nameof(ScugHelper), $"{speedX}");
                    spear.speed.X = speedX + aimX * (Math.Abs(speedX) * (1 + BackboostStrength) + AddedSpeed);
                    Player.Speed.X -= aimX * (Math.Abs(speedX) * (1 - BackboostStrength) + AddedSpeed);
                    Scene.Add(spear);
                }
                RemoveSelf();
                Audio.Play("event:/char/madeline/crystaltheo_throw");
                Player.Sprite.Play("throw");
                Done = true;
            }
            HoldTimer = 0f;
        }
    }

    public override void Render() {
        if (Icon is null || Player is null) { Logger.Warn(nameof(ScugHelper), "Could not render icon!"); return; }
        var drawPosition = Player.TopCenter;
        drawPosition -= Vector2.UnitX * (int)Player.Facing * 8f;
        Icon.Draw(drawPosition, new Vector2(Icon.Width, Icon.Height) / 2, Color.White);
    }
}

// Mostly copied from Celeste.TalkComponent. This portion of the code is not licenseable. Use at your own discretion.
#region Unlicenseable
internal class SpearPickupComponent(Rectangle bounds, Vector2 drawAt, Action<Player> onGrab) : Component(true, true) {
    public class HoverDisplay() {
        public MTexture? Texture;
        public Vector2 InputPosition;
    }

    public class SpearPickupComponentUI : Entity {
        public SpearPickupComponent Handler;

        private bool highlighted;
        private float slide;
        private float timer;
        private readonly Wiggler wiggler;
        private float alpha = 1f;
        private Color lineColor = Color.White;
        public bool Highlighted {
            get => highlighted;
            set {
                if ((highlighted != value) & Display) {
                    highlighted = value;
                    wiggler.Start();
                }
            }
        }

        public bool Display {
            get {
                if (!Handler.Enabled || Scene == null || Scene.Tracker.GetEntity<Textbox>() != null)
                    return false;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity == null || entity.StateMachine.State == 11)
                    return false;

                Level level = (Scene as Level)!;
                if (!level.FrozenOrPaused)
                    return level.RetryPlayerCorpse == null;

                return false;
            }
        }

        public SpearPickupComponentUI(SpearPickupComponent handler) {
            Handler = handler;
            AddTag((int)Tags.HUD | (int)Tags.Persistent);
            Add(wiggler = Wiggler.Create(0.25f, 4f));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (Highlighted) alpha = 1f;
        }

        public override void Update() {
            timer += Engine.DeltaTime;
            slide = Calc.Approach(slide, Display ? 1 : 0, Engine.DeltaTime * 4f);
            if (Highlighted) alpha = 1f;

            base.Update();
        }

        public override void Render() {
            if (Scene is not Level level) return;
            if (level.FrozenOrPaused || !(slide > 0f) || Handler.Entity == null) return;

            Vector2 vector = level.Camera.Position.Floor();
            Vector2 vector2 = Handler.Entity.Position + Handler.DrawAt - vector;
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                vector2.X = 320f - vector2.X;

            vector2.X *= 6f;
            vector2.Y *= 6f;
            vector2.Y += (float)Math.Sin(timer * 4f) * 12f + 64f * (1f - Ease.CubeOut(slide));
            float num = (!Highlighted) ? (1f + wiggler.Value * 0.5f) : (1f - wiggler.Value * 0.5f);
            float num2 = Ease.CubeInOut(slide) * alpha;
            Color color = lineColor * num2;
            MTexture tex = (Highlighted && Handler.HoverUI.Texture is { } t) ? t : GFX.Gui["hover/idle"];

            if (Highlighted) {
                tex.DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, num);
                Vector2 position = vector2 + Handler.HoverUI.InputPosition * num;
                if (Input.GuiInputController(Input.PrefixMode.Latest))
                    Input.GuiButton(Input.Grab, Input.PrefixMode.Latest).DrawJustified(position, new Vector2(0.5f), Color.White * num2, num);
                else
                    ActiveFont.DrawOutline(Input.FirstKey(Input.Grab).ToString().ToUpper(), position, new Vector2(0.5f), new Vector2(num), Color.White * num2, 2f, Color.Black);
            }
        }
    }

    public static SpearPickupComponent? PlayerOver;
    public bool Enabled = true;
    public Rectangle Bounds = bounds;
    public Vector2 DrawAt = drawAt;
    public Action<Player> OnGrab = onGrab;
    public SpearPickupComponentUI? UI;
    public HoverDisplay HoverUI = new() {
        Texture = GFX.Gui["hover/highlight"],
        InputPosition = new Vector2(0f, -75f)
    };

    private float cooldown;
    private float hoverTimer;
    private float disableDelay;

    public override void Update() {
        if (Entity.Scene is null) {
            RemoveSelf();
            return;
        }
        if (UI == null) Entity.Scene.Add(UI = new SpearPickupComponentUI(this));

        if (Scene.Tracker.GetEntity<Player>() is not Player player) return;
        bool hovered = disableDelay < 0.05f
            && player.CollideRect(new Rectangle((int)(Entity.X + Bounds.X), (int)(Entity.Y + Bounds.Y), Bounds.Width, Bounds.Height))
            && player.Get<SpearComponent>() is null
            && player.StateMachine.State == 0
            && (PlayerOver == null || PlayerOver == this);
        if (hovered) hoverTimer += Engine.DeltaTime;
        else if (UI.Display) hoverTimer = 0f;

        if (PlayerOver == this && !hovered) PlayerOver = null;
        else if (hovered) PlayerOver = this;

        if (hovered && cooldown <= 0f && (int)player.StateMachine == 0 && Input.Grab.Pressed && Enabled && !Scene.Paused) {
            cooldown = 0.1f;
            OnGrab?.Invoke(player);
            RemoveSelf();
        }

        if (hovered && (int)player.StateMachine == 0)
            cooldown -= Engine.DeltaTime;

        if (!Enabled) disableDelay += Engine.DeltaTime;
        else disableDelay = 0f;

        UI.Highlighted = hovered && hoverTimer > 0.1f;
        base.Update();
    }

    public override void Removed(Entity entity) {
        Dispose();
        base.Removed(entity);
    }

    public override void EntityRemoved(Scene scene) {
        Dispose();
        base.EntityRemoved(scene);
    }

    public override void SceneEnd(Scene scene) {
        Dispose();
        base.SceneEnd(scene);
    }

    private void Dispose() {
        if (PlayerOver == this) PlayerOver = null;
        Scene?.Remove(UI);
        UI = null;
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        Draw.HollowRect(Entity.X + Bounds.X, Entity.Y + Bounds.Y, Bounds.Width, Bounds.Height, Color.Green);
    }
}
#endregion
