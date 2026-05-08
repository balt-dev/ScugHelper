using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using MonoMod;
namespace Celeste.Mod.ScugHelper.Entities.Actions;

[TrackedAs(typeof(SwapBlock))]
[CustomEntity("ScugHelper/ActionSwapBlock")]
public class ActionSwapBlock : SwapBlock
{

    internal class CustomPathRenderer : PathRenderer
    {
        public CustomPathRenderer(ActionSwapBlock block)
            : base(block) => pathTexture = GFX.Game[block.PathSprite + ((block.start.X == block.end.X) ? "V" : "H")];
    }

    readonly string[] Groups;
    readonly bool Toggle;
    readonly bool Particles;
    readonly string PathSprite;
    readonly new float ReturnTime;

    private static readonly MTexture transparent = new(VirtualContent.CreateTexture("transparent", 24, 24, Color.Transparent));

    public ActionSwapBlock(EntityData data, Vector2 offset)
        : base(
            data.Position + offset, data.Width, data.Height,
            data.FirstNodeNullable(offset) ?? throw new Exception("Swap block must have end node."),
            data.Bool("HidePath") ? Themes.Moon : Themes.Normal
        )
    {
        Particles = data.Bool("Particles");
        Toggle = data.Bool("Toggle");
        Groups = IAction.GetGroups(data.String("Groups", "#PlayerDash"));
        Add(new ActionListener(Groups, OnAlert));

        Components.RemoveAll<DashListener>();
        Components.RemoveAll<Sprite>();

        PathSprite = data.String("PathSprite", "objects/swapblock/path");
        ReturnTime = data.Float("ReturnTime", 0.8f);

        var inactive = GFX.Game[data.String("InactiveBlockSprite", "objects/swapblock/block")];
        var active = GFX.Game[data.String("ActiveBlockSprite", "objects/swapblock/blockRed")];
        var background =
            data.Bool("HideBackground")
            ? transparent
            : GFX.Game[data.String("BackgroundSprite", "objects/swapblock/target")];

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

        middleGreen = null;
        middleRed = null;
        if (!data.Bool("HideMiddle")) {
            Add(middleGreen = GFX.SpriteBank.Create(data.String("InactiveMiddleSprite", "swapBlockLight")));
            Add(middleRed = GFX.SpriteBank.Create(data.String("ActiveMiddleSprite", "swapBlockLightRed")));
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
            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
        else
            moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
    }

    public override void Awake(Scene scene) {
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
                returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", Center);
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

            MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
            Audio.Position(moveSfx, Center);
            Audio.Position(returnSfx, Center);


            if (Toggle)
            {
                if (lerp <= 0 || lerp >= 1)
                {
                    Audio.SetParameter(returnSfx, "end", 1f);
                    Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                }
            }
            else
            {
                if (lerp <= 0 && target == 0)
                {
                    Audio.SetParameter(returnSfx, "end", 1f);
                    Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                }
                else if (lerp >= 1 && target == 1)
                    Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
            }
        }

        if (Swapping && (lerp >= 1 || lerp <= 0))
            Swapping = false;

        StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
    }
}
