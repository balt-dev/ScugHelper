using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using MonoMod;
using System.Linq;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities.Actions;

[TrackedAs(typeof(SwapBlock))]
[CustomEntity("ScugHelper/SliderSwapBlock")]
public class SliderSwapBlock : SwapBlock {
    internal class CustomPathRenderer : PathRenderer {
        public CustomPathRenderer(SliderSwapBlock block)
            : base(block) => pathTexture = GFX.Game[block.SpritePath + "/path" + ((block.start.X == block.end.X) ? "V" : "H")];
    }

    readonly string Slider;
    readonly bool Slippery;
    readonly bool Particles;
    readonly bool ClampToBounds;
    readonly string SpritePath;
    readonly string MoveSound;
    readonly string MoveEndSound;
    readonly float MoveSpeed;

    bool MovedLastUpdate;
    float OldProgress;
    Vector2 LastPosition;

    private static readonly MTexture transparent = new(VirtualContent.CreateTexture("transparent", 24, 24, Color.Transparent));

    public SliderSwapBlock(EntityData data, Vector2 offset)
        : base(
            data.Position + offset, data.Width, data.Height,
            data.FirstNodeNullable(offset) ?? throw new Exception("Swap block must have end node."),
            data.Bool("HidePath") ? Themes.Moon : Themes.Normal
        ) {
        this.DisableInterpolation();
        MoveSound = data.String("MoveSound", "event:/game/05_mirror_temple/swapblock_move");
        MoveEndSound = data.String("MoveEndSound", "event:/game/05_mirror_temple/swapblock_move_end");
        SurfaceSoundIndex = data.Int("SurfaceSoundIndex", 8);
        Slippery = data.Bool("Slippery", false);
        Particles = data.Bool("Particles", true);
        ClampToBounds = data.Bool("ClampToBounds", true);
        Slider = data.String("Slider", "");
        MoveSpeed = data.Float("SpeedLimit", -1f) / (end - start).Length();

        Components.RemoveAll<DashListener>();
        Components.RemoveAll<Sprite>();

        SpritePath = data.String("SpriteDirectory", "objects/swapblock");

        var inactive = GFX.Game[SpritePath + "/block"];
        var active = GFX.Game[SpritePath + "/blockRed"];
        var background =
            data.Bool("HideBackground")
            ? transparent
            : GFX.Game[SpritePath + "/target"];

        nineSliceGreen = new MTexture[3, 3];
        nineSliceRed = new MTexture[3, 3];
        nineSliceTarget = new MTexture[3, 3];
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                nineSliceGreen[i, j] = inactive.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceRed[i, j] = active.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceTarget[i, j] = background.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }

        if (data.Bool("HideMiddle")) {
            middleGreen = null;
            middleRed = null;
        } else {
            middleGreen.Reset(GFX.Game, SpritePath + '/');
            middleRed.Reset(GFX.Game, SpritePath + '/');
            middleGreen.AddLoop("idle", "midBlock", 0.08f, [0, 1, 2, 3]);
            middleRed.AddLoop("idle", "midBlockRed", 0.08f, [0, 1, 2, 3]);
            middleGreen.Play("idle", true);
            middleRed.Play("idle", true);
        }
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (Scene is not Level level) return;
        path.RemoveSelf();
        scene.Add(path = new CustomPathRenderer(this));
        float rawSlider = level.Session.GetSlider(Slider);
        OldProgress = lerp = ClampToBounds ? Math.Clamp(rawSlider, 0, 1) : rawSlider;
        Vector2 newPos = Vector2.Lerp(start, end, OldProgress);
        LastPosition = newPos;
        MoveTo(newPos, Vector2.Zero);
    }

    [MonoModLinkTo("Celeste.Solid", "System.Void Update")]
    private void SolidUpdate() { }

    public override void Update() {
        SolidUpdate();
        if (Scene is not Level level) return;

        burst?.Position = Center;
        
        float rawSlider = level.Session.GetSlider(Slider);
        float target = ClampToBounds ? Math.Clamp(rawSlider, 0, 1) : rawSlider;
        lerp = MoveSpeed <= 0 ? target : Calc.Approach(lerp, target, MoveSpeed * Engine.DeltaTime);
        float delta = lerp - OldProgress;

        LastPosition = Position;
        Vector2 newPos = Vector2.Lerp(start, end, lerp);
        
        if (lerp is >= 1 or <= 0) {
            middleRed?.SetAnimationFrame(0);
            middleGreen?.SetAnimationFrame(0);
        }
        if (delta != 0) {
            if (!MovedLastUpdate) {
                Audio.Stop(moveSfx);
                moveSfx = Audio.Play(MoveSound, Center);
            }
            MovedLastUpdate = true;
            Vector2 liftSpeed = (newPos - LastPosition) / Engine.DeltaTime;

            if (Particles && Scene.OnInterval(0.02f))
                MoveParticles(delta > 0 ? end - start : start - end);
            if (Slippery) {
                GetRiders();
            }

            MoveTo(newPos, liftSpeed);

            if (Slippery) {
                foreach (Actor rider in riders) {
                    if (rider is Player) continue;
                    if (SpeedAccessor.For(rider) is not SpeedAccessor accessor) continue;
                    accessor.Speed += liftSpeed;
                    rider.LiftSpeed = Vector2.Zero;
                }
                riders.Clear();
                if (CollideFirstOutside<Player>(Vector2.Lerp(start, end, lerp)) is Player player) {
                    if (player.StateMachine.State is Player.StClimb) {
                        player.Speed.Y += liftSpeed.Y;
                        player.LiftSpeed = new(liftSpeed.X, 0);
                    } else {
                        player.Speed.X += liftSpeed.X;
                        player.Speed.Y += liftSpeed.Y;
                        player.LiftSpeed = Vector2.Zero;
                    }
                    if (player.StateMachine.State is Player.StDash or Player.StRedDash)
                        player.StateMachine.State = Player.StNormal;
                }
            }
        } else {
            LiftSpeed = Vector2.Zero;
            if (MovedLastUpdate) {
                Audio.Play(MoveEndSound, Center);
            }
            MovedLastUpdate = false;
        }
        redAlpha = MovedLastUpdate ? 0 : 1;
        OldProgress = lerp;

        StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
    }
    
    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        Draw.Line(LastPosition, Position, Color.Lime);
    } 
}
