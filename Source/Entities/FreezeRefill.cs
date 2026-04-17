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
[CustomEntity("ScugHelper/FreezeRefill")]
public class FreezeRefill : Refill, ICustomRefill
{
    public FreezeRefill(Vector2 position, bool oneUse) : base(position, false, oneUse)
    {
        Depth = -100;
        Remove(outline);
        Remove(sprite);
        Remove(flash);
        Add(sprite = new Sprite(GFX.Game, "objects/freezeRefill/idle"));
        Add(outline = new Image(GFX.Game["objects/freezeRefill/outline"]));
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
    public FreezeRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("oneUse")) { }

    public override void Render()
    {
        if (sprite.Visible) sprite.DrawOutline();
        base.Render();
    }
    public void CustomOnPlayer(Player player)
    {
        Audio.Play("event:/game/general/diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(NewRefillRoutine(player)));
        respawnTimer = 2.5f;
    }
    public IEnumerator NewRefillRoutine(Player player)
    {
        float num = player.Speed.Angle();
        Frozen = true;
        sprite.Visible = false;
        if (!oneUse) outline.Visible = true;
        Depth = 8999;
        yield return null;
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse) RemoveSelf();
    }
    public static void LoadHooks() {
        IL.Monocle.Engine.Update += EngineUpdateHook;
    }

    public static void UnloadHooks() {
        IL.Monocle.Engine.Update -= EngineUpdateHook;
    }

    private static bool Frozen = false;
    
    private static void EngineUpdateHook(ILContext il)
    {
        ILCursor cur = new(il);
        ILLabel? label = null;
        if (!cur.TryGotoNext(MoveType.After,
            static instr => instr.MatchLdsfld(typeof(Engine), nameof(Engine.DashAssistFreeze)),
            instr => instr.MatchBrtrue(out label)
        )) throw new InvalidOperationException("Freeze refills failed to match IL code for the EngineUpdate hook.");
        cur.EmitLdarg0();
        Level _;
        cur.EmitDelegate(static (Engine engine) => {
            Frozen &= !(Input.Jump.Pressed || Input.Grab.Pressed || Input.Dash.Pressed || Input.Pause.Pressed || Input.CrouchDash.Pressed);
            if (Frozen) {
                (engine.scene as Level).UpdateTime();
                engine.scene.Entities.UpdateLists();
            }
            return Frozen;
        });
        cur.EmitBrtrue(label!);
    }
}
#nullable restore
