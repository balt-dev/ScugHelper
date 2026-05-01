using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using Celeste.Mod.ScugHelper;
using Celeste.Mod;
using System.Collections;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using System.Reflection;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
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
    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Player.CreateTrail += Player_CreateTrail;
        On.Celeste.Player.Update += Player_Update;
        On.Celeste.Player.Render += Player_Render;
        On.Celeste.Level.Reload += Level_Reload;
        On.Celeste.LevelLoader.StartLevel += LevelLoader_StartLevel;

        if (!HookUtils.TryDisableInlining(typeof(Collider).GetMethod("Collide", [typeof(Entity)])))
            throw new Exception("Failed to disable inlining for collision for limbo refills.");
        On.Monocle.Collider.Collide_Entity += OnCollide_HACK;
    }
    [OnUnload]
    public static void UnloadHooks()
    {
        On.Celeste.Player.CreateTrail -= Player_CreateTrail;
        On.Celeste.Player.Update -= Player_Update;
        On.Celeste.Player.Render -= Player_Render;
        On.Celeste.Level.Reload -= Level_Reload;
        On.Celeste.LevelLoader.StartLevel -= LevelLoader_StartLevel;
        On.Monocle.Collider.Collide_Entity -= OnCollide_HACK;
    }

    // WARNING
    // DO NOT DO THIS.
    // I normally would not do this, however a ColliderList doesn't work here.
    private static bool OnCollide_HACK(On.Monocle.Collider.orig_Collide_Entity orig, Collider self, Entity entity)
    {
        return (!(self.Entity is Player && LimboTimer > 0f) || entity is Platform or Trigger or InvisibleBarrier or RefillField) && orig(self, entity);
    }

    private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
    {
        if (!(LimboTimer > 0))
            orig(self);
    }

    public static float LimboTimer { get; internal set; }

    private static void CreateTrail(Player player) {
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
    [Command("givelimbo", "Gives the player a limbo refill.")]
    private static void GiveLimbo() {
        LimboTimer = 2f;
    }
}
#nullable restore
