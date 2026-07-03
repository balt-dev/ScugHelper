
using System;
using Celeste.Mod.Entities;
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

    internal Vector2 speed;
    internal Image Sprite;
    internal StateMachine StateMachine;
    internal TouchSwitchCollider TouchSwitchCollider;
    internal PufferCollider PufferCollider;
    internal SeekerCollider SeekerCollider;

    internal static readonly Hitbox BonkHitbox = new(12, 12, -6, -6);
    internal static readonly Hitbox PickupHitbox = new(16, 16, -8, -8);
    internal static readonly Hitbox SidethrowHitbox = new(24, 2, -12, -1);
    internal static readonly Hitbox DownthrowHitbox = new(2, 24, -1, -12);

    public Spear(EntityData data, Vector2 offset) : this(data, offset, StIdle) { }
    public Spear(EntityData data, Vector2 offset, int state): base(data.Position + offset) {
        Add(Sprite = new(GFX.Game[data.String("Sprite", "")]));
        Sprite.CenterOrigin();
        Add(StateMachine = new StateMachine());
        Add(TouchSwitchCollider = new TouchSwitchCollider());
        Add(PufferCollider = new PufferCollider(OnPuffer));
        Add(SeekerCollider = new SeekerCollider(OnSeeker));
        StateMachine.AddState("Idle", () => StIdle, null, IdleBegin, IdleEnd);
        StateMachine.AddState("Sidethrow", () => StSidethrow, null, SidethrowBegin, SidethrowEnd);
        StateMachine.AddState("Downthrow", () => StDownthrow, null, DownthrowBegin, DownthrowEnd);
        StateMachine.AddState("Bonk", BonkUpdate, null, BonkBegin, null);

        State = state;
    }

    internal const float Gravity = 500f;
    internal const float RotationCoefficient = 0.1f;
    internal const float BonkCoefficient = -0.6f;
    internal float RotationSpeed;
    internal bool onGround;
    internal bool wasOnGround;

    public override void Update() {
        base.Update();
        onGround = OnGround();
        if (onGround && !wasOnGround) {
            State = StIdle;
        } else if (!onGround && wasOnGround) {
            RotationSpeed = 0f;
            State = StBonk;
        }
        wasOnGround = onGround;
        MoveH(speed.X * Engine.DeltaTime, OnCollideH);
        MoveV(speed.Y * Engine.DeltaTime, OnCollideV);
        if (onGround) speed.Y = 0;
        else speed.Y -= Gravity * Engine.DeltaTime;
    }

    private void OnCollideH(CollisionData data) {
        if (State == StBonk) { speed.X = 0; return; }
        if (State == StSidethrow && TryBreak(data)) return;
        if (data.Direction.X != -Math.Sign(speed.X)) return;
        speed.X *= BonkCoefficient;
        RotationSpeed = speed.X * RotationCoefficient;
        State = StBonk;
    }
    
    private void OnCollideV(CollisionData data) {
        if (data.Direction.Y <= 0) { speed.Y = 0; return; }
        if (State == StDownthrow && TryBreak(data)) return;
        speed = Vector2.Zero;
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
            default: return false;
        }
    }
    
    private void OnPuffer(Puffer puffer) {
        if (State == StSidethrow || State == StDownthrow) {
            if (!(puffer.state == Puffer.States.Gone || !(puffer.cantExplodeTimer <= 0f))) {
                puffer.Explode();
                puffer.GotoGone();
            }
        }
    }
    
    private void OnSeeker(Seeker seeker) {
        if (State == StSidethrow || State == StDownthrow)
            ScugHelperModule.KillSeeker(seeker);
    }
    
    private void OnPickup(Player player) {
        RemoveSelf();
        // TODO: SpearComponent stuff
    }

#region States
    internal void IdleBegin() {
        Add(new SpearPickupComponent(Collider.Bounds, Vector2.Zero, OnPickup));
        Collider = PickupHitbox;
    }
    
    internal void IdleEnd() {
        Components.RemoveAll<SpearPickupComponent>();
    }
    internal void SidethrowBegin() {
        Collider = SidethrowHitbox;
    }
    internal void SidethrowEnd() {
        
    }
    internal void DownthrowBegin() {
        Collider = DownthrowHitbox;
    }
    internal void DownthrowEnd() {
        
    }
    internal int BonkUpdate() {
        Sprite.Rotation += RotationSpeed * Engine.DeltaTime;
        return StBonk;
    }
    internal void BonkBegin() {
        Collider = BonkHitbox;
    }
#endregion
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
            tex.DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, num);

            if (Highlighted) {
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
    public bool PlayerMustBeFacing = true;
    public SpearPickupComponentUI? UI;
    public HoverDisplay HoverUI = new() {
        Texture = GFX.Gui["hover/highlight"],
        InputPosition = new Vector2(0f, -75f)
    };

    private float cooldown;
    private float hoverTimer;
    private float disableDelay;

    public override void Update() {
        if (UI == null) Entity.Scene.Add(UI = new SpearPickupComponentUI(this));

        if (Scene.Tracker.GetEntity<Player>() is not Player player) return;
        bool hovered = disableDelay < 0.05f
            && player.CollideRect(new Rectangle((int)(Entity.X + Bounds.X), (int)(Entity.Y + Bounds.Y), Bounds.Width, Bounds.Height))
            && player.OnGround() && player.Bottom < Entity.Y + Bounds.Bottom + 4f
            && player.StateMachine.State == 0 && (
                !PlayerMustBeFacing || 
                Math.Abs(player.X - Entity.X) <= 16f || 
                player.Facing == (Facings)Math.Sign(Entity.X - player.X)
            ) && (PlayerOver == null || PlayerOver == this);
        if (hovered) hoverTimer += Engine.DeltaTime;
        else if (UI.Display) hoverTimer = 0f;

        if (PlayerOver == this && !hovered) PlayerOver = null;
        else if (hovered) PlayerOver = this;

        if (hovered && cooldown <= 0f && (int)player.StateMachine == 0 && Input.Grab.Pressed && Enabled && !Scene.Paused) {
            cooldown = 0.1f;
            OnGrab?.Invoke(player);
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
        Scene.Remove(UI);
        UI = null;
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        Draw.HollowRect(Entity.X + Bounds.X, Entity.Y + Bounds.Y, Bounds.Width, Bounds.Height, Color.Green);
    }
}
#endregion