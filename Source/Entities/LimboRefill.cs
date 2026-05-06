using System;
using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/LimboRefill")]
public class LimboRefill : Refill, ICustomRefill
{
    private static readonly float InitialLimboLength = 2f;
    private static readonly float RefreshLimboLength = 0.5f;

    public LimboRefill(Vector2 position, bool oneUse) : base(position, false, oneUse)
    {
        Depth = -100;
        Remove(outline);
        Remove(sprite);
        Remove(flash);
        Add(sprite = new Sprite(GFX.Game, "objects/limboRefill/idle"));
        Add(outline = new Image(GFX.Game["objects/limboRefill/outline"]));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        outline.CenterOrigin();
        outline.Visible = false;
        Remove(wiggler);
        Add(wiggler = Wiggler.Create(1f, 4f, v => { sprite.Scale = Vector2.One * (1f + v * 0.2f); }));
        UpdateY();
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (Scene is not Level level) { RemoveSelf(); return; }
        this.level = level;
    }
    public LimboRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) { }

    public override void Render()
    {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void CustomOnPlayer(Player player)
    {
        if (LimboTimer <= 0f)
        {
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(NewRefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }
    public IEnumerator NewRefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        sprite.Visible = false;
        if (!oneUse) outline.Visible = true;
        LimboTimer = InitialLimboLength;
        player.RefillDash();
        player.RefillStamina();
        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse) RemoveSelf();
    }

    [Command("givelimbo", "Gives the player a limbo refill.")]
    private static void GiveLimbo() => LimboTimer = 2f;


    [OnLoad]
    public static void LoadHooks()
    {
        On.Celeste.Player.CreateTrail += Player_CreateTrail;
        On.Celeste.Player.Update += Player_Update;
        On.Celeste.Player.Render += Player_Render;
        On.Celeste.Level.Reload += Level_Reload;
        On.Celeste.LevelLoader.StartLevel += LevelLoader_StartLevel;

        On.Celeste.PlayerCollider.Check += OnPlayerColliderCheck;
        On.Celeste.Actor.MoveH += OnActorMoveH;
        On.Celeste.Actor.MoveV += OnActorMoveV;
    }
    [OnUnload]
    public static void UnloadHooks()
    {
        On.Celeste.Player.CreateTrail -= Player_CreateTrail;
        On.Celeste.Player.Update -= Player_Update;
        On.Celeste.Player.Render -= Player_Render;
        On.Celeste.Level.Reload -= Level_Reload;
        On.Celeste.LevelLoader.StartLevel -= LevelLoader_StartLevel;

        On.Celeste.PlayerCollider.Check -= OnPlayerColliderCheck;
        On.Celeste.Actor.MoveH -= OnActorMoveH;
        On.Celeste.Actor.MoveV -= OnActorMoveV;
    }
    
    private static bool OnActorMoveH(On.Celeste.Actor.orig_MoveH orig, Actor self, float move, Collision onCollide, Solid pusher)
    {
        if (self is Player player && LimboTimer > 0f) {
            LimboColliderList collList;
            player.Collider = collList = new LimboColliderList(player.Collider);
            var res = orig(self, move, onCollide, pusher);
            player.Collider = collList.OriginalCollider;
            return res;
        } else { return orig(self, move, onCollide, pusher); }
    }

    private static bool OnActorMoveV(On.Celeste.Actor.orig_MoveV orig, Actor self, float move, Collision onCollide, Solid pusher)
    {
        if (self is Player player && LimboTimer > 0f) {
            LimboColliderList collList;
            player.Collider = collList = new LimboColliderList(player.Collider);
            var res = orig(self, move, onCollide, pusher);
            player.Collider = collList.OriginalCollider;
            return res;
        } else { return orig(self, move, onCollide, pusher); }
    }

    internal static bool DenyEntityCollisions(Entity ent)
        =>  (LimboTimer > 0f) && !(ent is Platform or Trigger or InvisibleBarrier or RefillField);

    private static bool OnPlayerColliderCheck(On.Celeste.PlayerCollider.orig_Check orig, PlayerCollider self, Player player)
        => !DenyEntityCollisions(self.Entity) && orig(self, player);

    private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
    {
        if (!(LimboTimer > 0))
            orig(self);
    }

    public static float LimboTimer { get; internal set; }

    private static void CreateTrail(Player player)
    {
        Vector2 scale = new(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y);
        TrailManager.Add(player, scale, player.Hair.GetHairColor(0) * (0.4f + Math.Clamp(1.0f - LimboTimer / RefreshLimboLength, 0.0f, 1.0f) * 0.6f));
    }

    private static void Player_CreateTrail(On.Celeste.Player.orig_CreateTrail orig, Player player)
    {
        if (LimboTimer > 0)
            CreateTrail(player);
        else
            orig(player);
    }
    static float oldTimer;

    private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (LimboTimer > 0 && self.Scene.OnInterval(0.05f))
            CreateTrail(self);

        if (LimboTimer > 0 && (Input.MoveX != 0 || Input.MoveY != 0 || Input.Jump.Check || Input.Dash.Check || Input.Grab.Check || Input.CrouchDash.Check))
        {
            LimboTimer = Math.Max(LimboTimer, RefreshLimboLength);
        }
        LimboTimer -= Engine.DeltaTime;
        if (oldTimer > 0 && LimboTimer <= 0)
        {
            self.Play("event:/game/06_reflection/feather_state_end");
        }
        oldTimer = LimboTimer;
    }

    private static void Level_Reload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        LimboTimer = 0f;
        orig(self);
    }

    private static void LevelLoader_StartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self)
    {
        LimboTimer = 0f;
        orig(self);
    }
}

internal class LimboColliderList: ColliderList
{
    public Collider OriginalCollider { get => colliders[0]; }

    public LimboColliderList(Collider collider) => colliders = [collider];

    public override bool Collide(Circle o) => base.Collide(o) && !LimboRefill.DenyEntityCollisions(o.Entity);
    public override bool Collide(Hitbox o) => base.Collide(o) && !LimboRefill.DenyEntityCollisions(o.Entity);
    public override bool Collide(ColliderList o) => base.Collide(o) && !LimboRefill.DenyEntityCollisions(o.Entity);
    public override bool Collide(Grid o) => base.Collide(o) && !LimboRefill.DenyEntityCollisions(o.Entity);
}
#nullable restore
