using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod.Helpers;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/AngleBumper")]
public class AngleBumper : Bumper
{
    readonly float Angle;
    readonly float LaunchSpeed = 280f;
    readonly Image middleDecal;
    readonly int DashCount;
    readonly bool RefillStamina = true;

    public AngleBumper(EntityData data, Vector2 offset) : base(data, offset)
    {
        Angle = -Calc.DegToRad * data.Float("Angle", 0f);
        sprite.RemoveSelf();
        hitWiggler.RemoveSelf();
        Add(sprite = GFX.SpriteBank.Create(data.String("Sprite", "bumper")));
        DashCount = data.Int("DashCount", 1);
        LaunchSpeed = data.Float("LaunchSpeed", 280f);
        RefillStamina = data.Bool("RefillStamina", true);
        if (data.String("MiddleDecal") is string middleString) {
            var settings = SpeedRefill.GetVectorAnglePrefix(Calc.AngleToVector(Angle, 1f));
            Add(middleDecal = new Image(GFX.Game[middleString + "_" + settings.Prefix]));
            middleDecal.CenterOrigin();
            if (settings.HFlip) { middleDecal.FlipX = true; }
            if (settings.VFlip) { middleDecal.FlipY = true; }
            if (settings.Rot90) { middleDecal.Rotation = MathF.PI / 2; }
        }
        Add(hitWiggler = Wiggler.Create(1.2f, 2f, (v) => { spriteEvil.Position = hitDir * hitWiggler.Value * 8f; }));
        Components.RemoveAll<PlayerCollider>();
        Components.RemoveAll<CoreModeListener>();
        Add(new PlayerCollider(NewOnPlayer));
    }

    public override void Update() {
        fireMode = false;
        sprite.Visible = true;
        spriteEvil.Visible = false;
        base.Update();
    }

    private void NewOnPlayer(Player player)
    {
        if (respawnTimer <= 0f)
        {
            Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);

            respawnTimer = 0.6f;

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);

            Vector2 SpeedAngle = Calc.AngleToVector(Angle, 1f);
            int dashes = player.Dashes;
            player.Speed = SpeedAngle * LaunchSpeed;
            if (SpeedAngle.Y <= 50f / 280f)
            {
                player.AutoJump = true;
            }
            if (player.Speed.X != 0f) {
                if (Input.MoveX.Value == Math.Sign(player.Speed.X))
                {
                    player.explodeLaunchBoostTimer = 0f;
                    player.Speed.X *= 1.2f;
                }
                else
                {
                    player.explodeLaunchBoostTimer = 0.01f;
                    player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
                }
            }

            SlashFx.Burst(player.Center, Angle);

            player.dashCooldownTimer = 0.2f;
            player.StateMachine.State = 7;

            player.Dashes = Math.Max(player.Dashes, DashCount);

            if (RefillStamina) player.RefillStamina();
            sprite.Play("hit", restart: true);
            spriteEvil.Play("hit", restart: true);
            light.Visible = false;
            bloom.Visible = false;
            SceneAs<Level>().DirectionalShake(SpeedAngle, 0.15f);
            SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
            SceneAs<Level>().Particles.Emit(P_Launch, 12, Center + SpeedAngle * 12f, Vector2.One * 3f, Angle);
        }
    }

    [OnLoad]
    public static void LoadHooks() => On.Celeste.Bumper.UpdatePosition += OnUpdatePosition;
    [OnUnload]
    public static void UnloadHooks() => On.Celeste.Bumper.UpdatePosition -= OnUpdatePosition;

    private static void OnUpdatePosition(On.Celeste.Bumper.orig_UpdatePosition orig, Bumper self)
    {
        if (self is AngleBumper)
            self.Position = self.anchor;
        else
            orig(self);
    }
}
