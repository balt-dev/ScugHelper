using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.ScugHelper.Entities;


[Tracked]
[CustomEntity("ScugHelper/SeekerRefill")]
public class SeekerRefill : Refill, ICustomRefill
{
    public SeekerRefill(Vector2 position, bool oneUse) : base(position, false, oneUse) {
        Depth = -100;
        Remove(outline);
        Remove(sprite);
        Remove(flash);
        Add(sprite = new Sprite(GFX.Game, "objects/ScugHelper/seekerRefill/idle"));
        Add(outline = new Image(GFX.Game["objects/ScugHelper/seekerRefill/outline"]));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        outline.CenterOrigin();
        outline.Visible = false;
        Remove(wiggler);
        Add(wiggler = Wiggler.Create(1f, 4f, v => { sprite.Scale = Vector2.One * (1f + v * 0.2f); }));
        UpdateY();
        p_glow = new(P_Glow) {
            Color = Calc.HexToColor("7ac533"),
            Color2 = Calc.HexToColor("435e28"),
        };
    }
    public override void Added(Scene scene) {
        base.Added(scene);
        if (Scene is not Level level) { RemoveSelf(); return; }
        this.level = level;
    }
    public SeekerRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) { }

    public override void Render() {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void CustomOnPlayer(Player player) {
        if (player.Get<PlayerSeekerComponent>() is null) {
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(NewRefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }
    public IEnumerator NewRefillRoutine(Player player) {
        Celeste.Freeze(0.05f);
        yield return null;
        sprite.Visible = false;
        if (!oneUse) outline.Visible = true;
        player.Add(new PlayerSeekerComponent());
        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse) RemoveSelf();
    }
}
#nullable restore
