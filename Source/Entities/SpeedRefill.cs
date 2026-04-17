using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/SpeedRefill")]
public class SpeedRefill : Refill, ICustomRefill
{
    private readonly Vector2 SpeedToSet;

    public SpeedRefill(EntityData data, Vector2 offset) : this(data.Position + offset, new(data.Float("SpeedX"), data.Float("SpeedY")), data.Bool("oneUse")) { }

    public SpeedRefill(Vector2 position, Vector2 speed, bool oneUse) : base(position, false, oneUse)
    {
        Depth = -100;
        SpeedToSet = speed;
        Remove(outline);
        Remove(sprite);
        Remove(flash);
        float rawAngle = (speed.Angle().ToDeg() + 90 + 360) % 360;
        string prefix;
        bool hFlip = false;
        bool vFlip = false;
        bool rot90 = false;
        if ((rawAngle % 180) >= 45 && ((rawAngle < 180 && rawAngle % 180 < 90) || (rawAngle > 180 && rawAngle % 180 <= 90)))
        {
            vFlip = !vFlip;
            hFlip = !hFlip;
        }
        if (rawAngle >= 180) {
            hFlip = !hFlip;
            rawAngle = 360 - rawAngle;
        }
        if (rawAngle >= 90) {
            vFlip = !vFlip;
            rawAngle = 180 - rawAngle;
        }
        if (rawAngle >= 45)
        {
            vFlip = !vFlip;
            rot90 = true;
            rawAngle = 90 - rawAngle;
        }
        if (rawAngle < 7.5) prefix = "rot0_";
        else if (rawAngle < 22.5) prefix = "rot15_";
        else if (rawAngle < 37.5) prefix = "rot30_";
        else prefix = "rot45_";

        Add(sprite = new Sprite(GFX.Game, $"objects/speedRefill/{prefix}"));
        Add(outline = new Image(GFX.Game[$"objects/speedRefill/{prefix}outline"]));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        outline.CenterOrigin();
        if (hFlip) { sprite.FlipX = true; outline.FlipX = true; }
        if (vFlip) { sprite.FlipY = true; outline.FlipY = true; }
        if (rot90) { sprite.Rotation = MathF.PI / 2; outline.Rotation = MathF.PI / 2; }
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

    public override void Render()
    {
        outline.Visible = true;
        if (sprite.Visible) {
            outline.Position = sprite.Position;
            outline.DrawOutline(Color.Black);
            outline.Render();
            sprite.Render();
        }
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
        Celeste.Freeze(0.05f);
        yield return null;
        sprite.Visible = false;
        Depth = 8999;
        player.Speed = SpeedToSet;
        player.Position = Center + (player.Position - player.Center);
        if (!(player.LastBooster is PinballBooster pinball && pinball.BoostingPlayer)) player.StateMachine.State = Player.StLaunch;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse) RemoveSelf();
    }

    [Command("speedrefilltest", "Spawns speed refills around the player at every angle.")]
    internal static void SpeedRefillTest() {
        if (Engine.Scene is not Level level) return;
        Player? maybePlayer = level.Tracker.GetEntity<Player>();
        if (maybePlayer is not Player player) return;
        Vector2 pos = player.Position - Vector2.UnitY * 80f;
        for (float i = 0; i < 360; i += 3) {
            float angle = ((float)i).ToRad();
            Vector2 unitVec = new(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 deltaVec = unitVec * (60f - i % 8 * 6f);
            Vector2 offsetPosition = pos + deltaVec;
            SpeedRefill refill;
            level.Add(refill = new SpeedRefill(offsetPosition, deltaVec, false));
            refill.Collidable = false;
            refill.sprite.Visible = false;
            refill.respawnTimer = float.PositiveInfinity;
            refill.light.RemoveSelf();
            refill.bloom.RemoveSelf();
        }
    }
}
#nullable restore
