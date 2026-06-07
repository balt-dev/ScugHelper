using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using Celeste.Mod.ScugHelper;
using Celeste.Mod;
using System.Collections;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/BoostRefill")]
public class BoostRefill : Refill, ICustomRefill
{
    public readonly float BoostAmount;
    public BoostRefill(Vector2 position, bool oneUse, float boostAmount) : base(position, false, oneUse) {
        BoostAmount = boostAmount;
        Depth = -100;
        Remove(outline);
        Remove(sprite);
        Remove(flash);
        Add(sprite = new Sprite(GFX.Game, "objects/ScugHelper/boostRefill/idle"));
        Add(outline = new Image(GFX.Game["objects/ScugHelper/boostRefill/outline"]));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        outline.CenterOrigin();
        outline.Visible = false;
        Remove(wiggler);
        Add(wiggler = Wiggler.Create(1f, 4f, v => { sprite.Scale = Vector2.One * (1f + v * 0.2f); }));
        UpdateY();
    }
    public override void Added(Scene scene) {
        base.Added(scene);
        if (Scene is not Level level) { RemoveSelf(); return; }
        this.level = level;
    }
    public BoostRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse"), data.Float("BoostAmount", 1.75f)) { }

    public override void Render() {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void CustomOnPlayer(Player player) {
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(NewRefillRoutine(player)));
        respawnTimer = 2.5f;
    }
    public IEnumerator NewRefillRoutine(Player player) {
        Celeste.Freeze(0.05f);
        yield return null;
        sprite.Visible = false;
        if (!oneUse) outline.Visible = true;
        Depth = 8999;
        yield return 0.05f;
        player.Speed *= BoostAmount;
        player.LaunchedBoostCheck();
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse) RemoveSelf();
    }
}
#nullable restore
