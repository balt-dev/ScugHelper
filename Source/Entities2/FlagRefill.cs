using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/FlagRefill")]
public class FlagRefill : Refill, ICustomRefill {
    internal readonly string Flag;
    internal readonly bool FlagState;
    internal readonly float RespawnTimer;
    public FlagRefill(EntityData data, Vector2 offset) : base(data.Position + offset, false, data.Bool("oneUse")) {
        Depth = -100;
        Remove(outline);
        Remove(sprite);
        string SpritePath = data.String("SpritePath", "objects/ScugHelper/flagRefill");
        Add(sprite = new Sprite(GFX.Game, SpritePath + "/idle"));
        Add(outline = new Image(GFX.Game[SpritePath + "/outline"]));
        Flag = data.String("Flag", "");
        FlagState = data.Bool("FlagState", true);
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        outline.CenterOrigin();
        outline.Visible = false;
        Remove(wiggler);
        Add(wiggler = Wiggler.Create(1f, 4f, v => { sprite.Scale = Vector2.One * (1f + v * 0.2f); }));
        UpdateY();
        p_glow = new(P_Glow) {
            Color = data.HexColor("ParticleColor1"),
            Color2 = data.HexColor("ParticleColor2"),
        };
        RespawnTimer = data.Float("RespawnTimer", 2.5f);
    }
    public override void Added(Scene scene) {
        base.Added(scene);
        if (Scene is not Level level) { RemoveSelf(); return; }
        level.Session.SetFlag(Flag, !FlagState);
        this.level = level;
    }

    public override void Render() {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void CustomOnPlayer(Player player) {
        if (player.level.Session.GetFlag(Flag) == FlagState) return;
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(FlagRefillRoutine(player)));
        respawnTimer = 2.5f;
    }
    public IEnumerator FlagRefillRoutine(Player player) {
        Celeste.Freeze(0.05f);
        yield return null;
        sprite.Visible = false;
        if (!oneUse) outline.Visible = true;
        Depth = 8999;
        yield return 0.05f;
        player.Add(new FlagRefillComponent(player, Flag, FlagState, p_glow));
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse) RemoveSelf();
    }

    [OnLoad]
    internal static void LoadHooks() => On.Celeste.Player.DashEnd += OnDashEnd;
    
    [OnUnload]
    internal static void UnloadHooks() => On.Celeste.Player.DashEnd -= OnDashEnd;

    private static void OnDashEnd(On.Celeste.Player.orig_DashEnd orig, Player self) {
        self.Components.RemoveAll<FlagRefillComponent>();
        orig(self);
    }

    [Tracked]
    private class FlagRefillComponent(Player player, string flag, bool flagState, ParticleType pType) : Component(true, false) {
        public override void Added(Entity entity) {
            if (player.level is null || player.level.Session is null) return;
            player.level.Session.SetFlag(flag, flagState);
        }
        public override void Update() {
            if (player.level is null || player.level.Session is null) return;
            player.level.Session.SetFlag(flag, flagState);
            if (player.level.OnInterval(0.1f))
                player.level.ParticlesBG.Emit(pType, 5, player.Center, Vector2.One * 12f);
        }
        public override void Removed(Entity entity) {
            if (player.level is null || player.level.Session is null) return;
            player.level.Session.SetFlag(flag, !flagState);
        }
        public override void SceneEnd(Scene scene) {
            (scene as Level)?.Session.SetFlag(flag, !flagState);
        }
    }
}
