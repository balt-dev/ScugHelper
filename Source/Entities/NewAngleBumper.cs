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
using MonoMod.Cil;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/NewAngleBumper")]
public class NewAngleBumper : Bumper
{
    readonly float Angle;
    readonly float LaunchSpeed = 280f;
    readonly Sprite middleSprite;
    readonly Sprite edgeSprite;
    readonly int DashCount;
    readonly bool RefillStamina = true;
    readonly bool EmitParticles = true;

    public NewAngleBumper(EntityData data, Vector2 offset) : base(data.Position + offset, null) {
        var nodePosition = data.FirstNodeNullable(offset) ?? throw new FormatException("Angle bumper must have a target node.");

        var angleVector = (nodePosition - Position).SafeNormalize();
        Angle = angleVector.Angle();

        Remove(sprite);
        sprite.Visible = false;
        hitWiggler.RemoveSelf();

        var spritePath = data.String("SpritePath", "objects/ScugHelper/newAngleBumper");
        bool flipX = false, flipY = false;
        if (angleVector.X < 0) { flipX = true; angleVector.X *= -1; }
        if (angleVector.Y > 0) { flipY = true; } else { angleVector.Y *= -1; }
        var displayAngle = Math.Abs(Calc.RadToDeg * Math.Acos(angleVector.X)) switch {
            < 10 => 0,
            < 40 => 30,
            < 80 => 60,
            _ => 90,
        };
        Position = anchor;
        
        middleSprite = new Sprite(GFX.Game, spritePath + "/middle");
        edgeSprite = new Sprite(GFX.Game, spritePath + $"/angles/{displayAngle}_") { FlipX = flipX, FlipY = flipY };

        middleSprite.Add("on", "", 0.06f, "idle", [42, 43, 44]);
        middleSprite.AddLoop("idle", "", 0.06f, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33]);
        middleSprite.Add("hit", "", 0.06f, "off", [34, 35, 36, 37, 38, 39, 40, 41, 42]);
        middleSprite.AddLoop("off", "", 0.06f, [42]);

        edgeSprite.Add("on", "", 0.06f, "idle", [42, 43, 44]);
        edgeSprite.AddLoop("idle", "", 0.06f, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33]);
        edgeSprite.Add("hit", "", 0.06f, "off", [34, 35, 36, 37, 38, 39, 40, 41, 42]);
        edgeSprite.AddLoop("off", "", 0.06f, [42]);
        
        middleSprite.CenterOrigin();
        edgeSprite.CenterOrigin();
        
        Add(middleSprite);
        Add(edgeSprite);
        
        edgeSprite.Play("idle");
        middleSprite.Play("idle");

        DashCount = data.Int("DashCount", 1);
        LaunchSpeed = data.Float("LaunchSpeed", 280f);
        RefillStamina = data.Bool("RefillStamina", true);
        EmitParticles = data.Bool("EmitParticles", true);

        Add(hitWiggler = Wiggler.Create(1.2f, 2f, (v) => { spriteEvil.Position = hitDir * hitWiggler.Value * 8f; }));
        Components.RemoveAll<PlayerCollider>();
        Components.RemoveAll<CoreModeListener>();
        Add(new PlayerCollider(NewOnPlayer));
    }

    public override void Update() {
        fireMode = false;
        middleSprite.Visible = true;
        edgeSprite.Visible = true;
        sprite.Visible = false;
        spriteEvil.Visible = false;
        var oldRespawnTimer = respawnTimer;
        base.Update();
        if (respawnTimer <= 0f && oldRespawnTimer > 0f) {
            light.Visible = true;
            bloom.Visible = true;
            middleSprite.Play("on");
            edgeSprite.Play("on");
            Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
        }
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        Draw.LineAngle(Position, Angle, LaunchSpeed / 16f, Color.Cyan);
    }

    private void NewOnPlayer(Player player) {
        if (respawnTimer <= 0f) {
            Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);

            respawnTimer = 0.6f;

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);
            
            Vector2 SpeedAngle = Calc.AngleToVector(Angle, 1f);
            Vector2 desiredPosition = Center + SpeedAngle * 12f - (player.Center - player.Position);
            player.Position = new(MathF.Round(desiredPosition.X), MathF.Round(desiredPosition.Y));
            player.Speed = SpeedAngle * LaunchSpeed;
            if (SpeedAngle.Y <= 50f / 280f) {
                player.AutoJump = true;
            }
            if (player.Speed.X != 0f) {
                if (Input.MoveX.Value == Math.Sign(player.Speed.X)) {
                    player.explodeLaunchBoostTimer = 0f;
                    player.Speed.X *= 1.2f;
                } else {
                    player.explodeLaunchBoostTimer = 0.01f;
                    player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
                }
            }

            SlashFx.Burst(player.Center, Angle);

            player.dashCooldownTimer = 0.2f;
            player.StateMachine.State = 7;

            player.Dashes = Math.Max(player.Dashes, DashCount);
            if (RefillStamina) player.RefillStamina();
            middleSprite.Play("hit", restart: true);
            edgeSprite.Play("hit", restart: true);
            light.Visible = false;
            bloom.Visible = false;
            SceneAs<Level>().DirectionalShake(SpeedAngle, 0.15f);
            SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
            SceneAs<Level>().Particles.Emit(P_Launch, 12, Center + SpeedAngle * 12f, Vector2.One * 3f, Angle);
        }
    }

    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Bumper.UpdatePosition += OnUpdatePosition;
        IL.Celeste.Bumper.Update += ILUpdate;
    }

    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Bumper.UpdatePosition -= OnUpdatePosition;
        IL.Celeste.Bumper.Update -= ILUpdate;
    }

    private static void ILUpdate(ILContext il) {
        ILCursor cur = new(il);
        ILLabel? label = null;
        if (!cur.TryGotoNextBestFit(MoveType.After,
            static match => match.MatchLdcR4(0.05f),
            static match => match.MatchCallOrCallvirt<Scene>(nameof(Scene.OnInterval)),
            match => match.MatchBrfalse(out label)
        )) throw new Exception("Failed to match particle branching for angle bumpers.");
        static bool BumperCheck(Bumper self) => self is NewAngleBumper angleBumper && !angleBumper.EmitParticles;
        cur.EmitLdarg0();
        cur.EmitDelegate(BumperCheck);
        cur.EmitBrtrue(label!);
    }

    private static void OnUpdatePosition(On.Celeste.Bumper.orig_UpdatePosition orig, Bumper self) {
        if (self is NewAngleBumper)
            self.Position = self.anchor;
        else
            orig(self);
    }
}
