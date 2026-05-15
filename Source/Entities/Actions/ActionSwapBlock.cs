using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using MonoMod;
using System.Linq;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities.Actions;

[TrackedAs(typeof(SwapBlock))]
[CustomEntity("ScugHelper/ActionSwapBlock")]
public class ActionSwapBlock : SwapBlock
{

    internal class CustomPathRenderer : PathRenderer
    {
        public CustomPathRenderer(ActionSwapBlock block)
            : base(block) => pathTexture = GFX.Game[block.SpritePath + "/path" + ((block.start.X == block.end.X) ? "V" : "H")];
    }

    readonly string[] Groups;
    readonly string ReturnSound;
    readonly string ReturnEndSound;
    readonly string MoveSound;
    readonly string MoveEndSound;
    readonly bool Toggle;
    readonly bool Slippery;
    readonly bool Particles;
    readonly string SpritePath;
    readonly new float ReturnTime;

    private static readonly MTexture transparent = new(VirtualContent.CreateTexture("transparent", 24, 24, Color.Transparent));

    public ActionSwapBlock(EntityData data, Vector2 offset)
        : base(
            data.Position + offset, data.Width, data.Height,
            data.FirstNodeNullable(offset) ?? throw new Exception("Swap block must have end node."),
            data.Bool("HidePath") ? Themes.Moon : Themes.Normal
        )
    {
        SurfaceSoundIndex = data.Int("SurfaceSoundIndex", 8);
        ReturnSound = data.String("ReturnSound", "event:/game/05_mirror_temple/swapblock_return");
        ReturnEndSound = data.String("ReturnEndSound", "event:/game/05_mirror_temple/swapblock_return_end");
        MoveSound = data.String("MoveSound", "event:/game/05_mirror_temple/swapblock_move");
        MoveEndSound = data.String("MoveEndSound", "event:/game/05_mirror_temple/swapblock_move_end");
        Particles = data.Bool("Particles");
        Toggle = data.Bool("Toggle");
        Slippery = data.Bool("Slippery");
        Groups = IAction.GetGroups(data.String("Groups", "#PlayerDash"));
        Add(new ActionListener(Groups, OnAlert));

        Components.RemoveAll<DashListener>();
        Components.RemoveAll<Sprite>();

        SpritePath = data.String("SpriteDirectory", "objects/swapblock");
        ReturnTime = data.Float("ReturnTime", 0.8f);

        var inactive = GFX.Game[SpritePath + "/block"];
        var active = GFX.Game[SpritePath + "/blockRed"];
        var background =
            data.Bool("HideBackground")
            ? transparent
            : GFX.Game[SpritePath + "/target"];

        nineSliceGreen = new MTexture[3, 3];
        nineSliceRed = new MTexture[3, 3];
        nineSliceTarget = new MTexture[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSliceGreen[i, j] = inactive.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceRed[i, j] = active.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                nineSliceTarget[i, j] = background.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
            }
        }

        if (data.Bool("HideMiddle"))
        {
            middleGreen = null;
            middleRed = null;
        }
        else
        {
            middleGreen.Reset(GFX.Game, SpritePath + '/');
            middleRed.Reset(GFX.Game, SpritePath + '/');
            middleGreen.AddLoop("idle", "midBlock", 0.08f, [0, 1, 2, 3]);
            middleRed.AddLoop("idle", "midBlockRed", 0.08f, [0, 1, 2, 3]);
            middleGreen.Play("idle", true);
            middleRed.Play("idle", true);
        }

        maxForwardSpeed = data.Float("MovementSpeed", 360) / Vector2.Distance(start, end);
        maxBackwardSpeed = maxForwardSpeed * data.Float("ReturnSpeedMultiplier", 0.4f);
    }

    private void OnAlert(Level level)
    {
        returnTimer = ReturnTime;
        Swapping = Toggle || lerp < 1f;
        target = Toggle ? 1 - target : 1;
        if (middleGreen != null) burst = (Scene as Level).Displacement.AddBurst(Center, 0.2f, 0f, 16f);
        var absLerp = target == 0 ? 1 - lerp : lerp;
        speed = absLerp >= 0.2 ? maxForwardSpeed : MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, absLerp / 0.2f);

        Audio.Stop(returnSfx);
        Audio.Stop(moveSfx);
        if (!Swapping)
            Audio.Play(MoveEndSound, Center);
        else
            moveSfx = Audio.Play(MoveSound, Center);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        path.RemoveSelf();
        scene.Add(path = new CustomPathRenderer(this));
    }

    [MonoModLinkTo("Celeste.Solid", "System.Void Update")]
    private void SolidUpdate() { }

    public override void Update()
    {
        SolidUpdate();

        if (returnTimer > 0f && !Toggle)
        {
            returnTimer -= Engine.DeltaTime;
            if (returnTimer <= 0f)
            {
                target = 1 - target;
                speed = 0f;
                Audio.Stop(returnSfx);
                returnSfx = Audio.Play(ReturnSound, Center);
            }
        }

        burst?.Position = Center;

        redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
        if (target == lerp && (Toggle || lerp == 0))
        {
            middleRed?.SetAnimationFrame(0);
            middleGreen?.SetAnimationFrame(0);
        }

        if (Toggle || target == 1)
            speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
        else
            speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
        float num = lerp;
        lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
        if (lerp != num)
        {
            Vector2 liftSpeed = (end - start) * speed;

            if (lerp < num)
                liftSpeed *= -1f;

            if (Particles && (Toggle || target == 1) && Scene.OnInterval(0.02f))
                MoveParticles(end - start);

            if (Slippery && CollideFirstOutside<Player>(Vector2.Lerp(start, end, lerp)) is Player player)
            {
                player.Speed.X = MathF.MaxMagnitude(player.Speed.X, liftSpeed.X);
                player.Speed.Y = MathF.MaxMagnitude(player.Speed.Y, liftSpeed.Y);
                player.LiftSpeed = player.Speed;
                if (player.StateMachine.State is Player.StDash or Player.StRedDash)
                    player.StateMachine.State = Player.StNormal;
            }
            if (Slippery)
            {
                GetRiders();
            }

            MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);

            if (Slippery)
            {
                foreach (Actor rider in riders)
                {
                    if (SpeedAccessor.For(rider) is not SpeedAccessor accessor) continue;
                    var actorSpeed = accessor.Speed;
                    accessor.Speed = new(liftSpeed.X, actorSpeed.Y);
                    rider.LiftSpeed = liftSpeed;
                }
                riders.Clear();
            }
            Audio.Position(moveSfx, Center);
            Audio.Position(returnSfx, Center);


            if (Toggle)
            {
                if (lerp <= 0 || lerp >= 1)
                {
                    Audio.SetParameter(returnSfx, "end", 1f);
                    Audio.Play(ReturnEndSound, Center);
                }
            }
            else
            {
                if (lerp <= 0 && target == 0)
                {
                    Audio.SetParameter(returnSfx, "end", 1f);
                    Audio.Play(ReturnEndSound, Center);
                }
                else if (lerp >= 1 && target == 1)
                    Audio.Play(MoveEndSound, Center);
            }
        }

        if (Swapping && (lerp >= 1 || lerp <= 0))
            Swapping = false;

        StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
    }
}
