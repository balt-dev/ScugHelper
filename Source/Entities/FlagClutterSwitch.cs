using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
using System.Collections;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/FlagClutterSwitch")]
public class FlagClutterSwitch : ClutterSwitch
{
    public readonly string Flag;
    public readonly bool TargetState;

    public FlagClutterSwitch(EntityData data, Vector2 offset) : base(data.Position + offset, (ClutterBlock.Colors)(-1))
    {
        OnDashCollide = NewOnDashed;
        Flag = data.String("Flag", "");
        TargetState = data.Bool("TargetState", true);
        Remove(icon);
        Add(icon = new Image(GFX.Game[data.String("Icon", "objects/resortclutter/icon_lightning")]));
        icon.CenterOrigin();
        icon.Position = new Vector2(16f, 8f);
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (SceneAs<Level>().Session.GetFlag(Flag) == TargetState) BePressed();
    }

    public void BeUnpressed()
    {
        pressed = false;
        sprite.Scale.X = 1f;
        atY -= 10f;
        MoveV(-10);
        sprite.Y -= 2f;
        sprite.Play("idle");
        Add(icon);
        vertexLight.StartRadius = 32;
        vertexLight.EndRadius = 64;
    }

    public override void Update()
    {
        base.Update();
        if (SceneAs<Level>().Session.GetFlag(Flag) == TargetState && !pressed) BePressed();
        else if (SceneAs<Level>().Session.GetFlag(Flag) != TargetState && pressed) BeUnpressed();
    }

    public DashCollisionResults NewOnDashed(Player player, Vector2 direction)
    {
        if (!pressed && direction == Vector2.UnitY)
        {
            Celeste.Freeze(0.2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Level level = player.level;
            level.Session.SetFlag(Flag, TargetState);
            vertexLight.StartRadius = 64f;
            vertexLight.EndRadius = 128f;
            level.DirectionalShake(Vector2.UnitY, 0.6f);
            level.Particles.Emit(P_Pressed, 20, TopCenter - Vector2.UnitY * 10f, positionRange: new Vector2(16f, 8f));
            BePressed();
            sprite.Scale.X = 1.5f;
            Add(new Coroutine(FlagAbsorbRoutine(player)));
        }

        return DashCollisionResults.NormalCollision;
    }

    public IEnumerator FlagAbsorbRoutine(Player player)
    {
        Add(cutsceneSfx = new SoundSource());
        float duration = 0.9f;
        cutsceneSfx.Play("event:/game/03_resort/clutterswitch_books");
        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => cutsceneSfx.Stop(true), 0.9f, start: true));

        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => Audio.Play("event:/game/03_resort/clutterswitch_finish", Position), duration, start: true));
        player.StateMachine.State = Player.StDummy;
        sprite.Play("break");
        Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);

        yield return 0.8f;
        player.StateMachine.State = 0;
    }
}
#nullable restore
