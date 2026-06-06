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

    public struct SpriteSettings() {
        public string? Prefix;
        public bool HFlip = false;
        public bool VFlip = false;
        public bool Rot90 = false;
    }

    public static SpriteSettings GetVectorAnglePrefix(Vector2 vec) {
        float rawAngle = (vec.Angle().ToDeg() + 90 + 360) % 360;
        SpriteSettings settings = new() { Prefix = "", HFlip = false, VFlip = false, Rot90 = false};
        if ((rawAngle % 180) >= 45 && ((rawAngle < 180 && rawAngle % 180 < 90) || (rawAngle > 180 && rawAngle % 180 <= 90))) {
            settings.VFlip = true;
            settings.HFlip = true;
        }
        if (rawAngle >= 180) {
            settings.HFlip = !settings.HFlip;
            rawAngle = 360 - rawAngle;
        }
        if (rawAngle >= 90) {
            settings.VFlip = !settings.VFlip;
            rawAngle = 180 - rawAngle;
        }
        if (rawAngle >= 45) {
            settings.VFlip = !settings.VFlip;
            settings.Rot90 = true;
            rawAngle = 90 - rawAngle;
        }
        if (rawAngle < 7.5) settings.Prefix = "rot0";
        else if (rawAngle < 22.5) settings.Prefix = "rot15";
        else if (rawAngle < 37.5) settings.Prefix = "rot30";
        else settings.Prefix = "rot45";
        return settings;
    }

    public SpeedRefill(Vector2 position, Vector2 speed, bool oneUse) : base(position, false, oneUse) {
        Depth = -100;
        SpeedToSet = speed;
        Remove(outline);
        Remove(sprite);
        Remove(flash);

        var settings = GetVectorAnglePrefix(speed);
        Add(sprite = new Sprite(GFX.Game, $"objects/ScugHelper/speedRefill/{settings.Prefix}_"));
        Add(outline = new Image(GFX.Game[$"objects/ScugHelper/speedRefill/{settings.Prefix}_outline"]));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        outline.CenterOrigin();
        if (settings.HFlip) { sprite.FlipX = true; outline.FlipX = true; }
        if (settings.VFlip) { sprite.FlipY = true; outline.FlipY = true; }
        if (settings.Rot90) { sprite.Rotation = MathF.PI / 2; outline.Rotation = MathF.PI / 2; }
        Remove(wiggler);
        Add(wiggler = Wiggler.Create(1f, 4f, v => { sprite.Scale = Vector2.One * (1f + v * 0.2f); }));
        UpdateY();
    }
    public override void Added(Scene scene) {
        base.Added(scene);
        if (Scene is not Level level) { RemoveSelf(); return; }
        this.level = level;
    }

    public override void Render() {
        outline.Visible = true;
        if (sprite.Visible) {
            outline.Position = sprite.Position;
            outline.DrawOutline(Color.Black);
            outline.Render();
            sprite.Render();
        }
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
        Depth = 8999;
        player.SetAdjustedSpeed(SpeedToSet);
        var positionTarget = Center + (player.Position - player.Center);
        player.X = MathF.Round(positionTarget.X);
        player.Y = MathF.Round(positionTarget.Y);
        if (!(player.LastBooster is PinballBooster pinball && pinball.BoostingPlayer && pinball.ConsumeBounce())) player.StateMachine.State = Player.StLaunch;
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
