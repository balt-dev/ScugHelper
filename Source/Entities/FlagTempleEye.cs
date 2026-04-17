using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
namespace Celeste.Mod.ScugHelper.Entities;

[TrackedAs(typeof(TempleEye))]
[CustomEntity("ScugHelper/FlagTempleEye")]
public class FlagTempleEye(EntityData data, Vector2 offset) : TempleEye(data, offset)
{
    private readonly bool trackPlayer = data.Bool("trackPlayer");
    private readonly string flagToCheck = data.String("flag");

    public override void Added(Scene scene)
    {
        Scene = scene;
        if (Components != null)
            foreach (Component component in Components)
                component.EntityAdded(scene);
        scene.SetActualDepth(this);

        isBG = !scene.CollideCheck<Solid>(Position);
        if (isBG)
        {
            eyeTexture = GFX.Game["scenery/temple/eye/bg_eye"];
            pupilTexture = GFX.Game["scenery/temple/eye/bg_pupil"];
            Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/bg_lid"));
            Depth = 8990;
        }
        else
        {
            eyeTexture = GFX.Game["scenery/temple/eye/fg_eye"];
            pupilTexture = GFX.Game["scenery/temple/eye/fg_pupil"];
            Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/fg_lid"));
            Depth = -10001;
        }

        eyelid.AddLoop("open", "", 0f, [0]);
        eyelid.Add("blink", "", 0.08f, "open", 0, 1, 1, 2, 3, 0);
        eyelid.AddLoop("close", "", 0f, [1]);
        eyelid.Add("toOpen", "", 0.08f, "open", 1, 3, 0);
        eyelid.Add("toClose", "", 0.08f, "close", 0, 3, 1);
        if (SceneAs<Level>().Session.Flags.Contains(flagToCheck))
            eyelid.Play("open");
        else
            eyelid.Play("close");
        eyelid.CenterOrigin();
        SetBlinkTimer();
    }

    public override void Awake(Scene scene)
    {
        foreach (Component component in Components)
            component.EntityAwake();

        Entity entity = trackPlayer ? Scene.Tracker.GetEntity<Player>() : Scene.Tracker.GetEntity<TheoCrystal>();
        if (entity != null)
        {
            pupilTarget = (entity.Center - Position).SafeNormalize();
            pupilPosition = pupilTarget * 3f;
        }
    }

    public override void Update()
    {
        if (!bursting)
        {
            if (SceneAs<Level>().Session.Flags.Contains(flagToCheck) && eyelid.CurrentAnimationID == "close")
                eyelid.Play("toOpen");
            else if (!SceneAs<Level>().Session.Flags.Contains(flagToCheck) && (eyelid.CurrentAnimationID == "open" || eyelid.CurrentAnimationID == "blink"))
                eyelid.Play("toClose");

            pupilPosition = Calc.Approach(pupilPosition, pupilTarget * 3f, Engine.DeltaTime * 16f);
            Entity entity = trackPlayer ? Scene.Tracker.GetEntity<Player>() : Scene.Tracker.GetEntity<TheoCrystal>();
            if (entity != null)
            {
                pupilTarget = (entity.Center - Position).SafeNormalize();
                if (Scene.OnInterval(0.25f) && Calc.Random.Chance(0.01f))
                    TryBlink();
            }

            blinkTimer -= Engine.DeltaTime;
            if (blinkTimer <= 0f)
            {
                SetBlinkTimer();
                TryBlink();
            }
        }
        Components.Update();
    }

    private void TryBlink() {
        if (eyelid.CurrentAnimationID == "open") eyelid.Play("blink");
    }

    public override void Render()
    {
        if (!bursting)
        {
            eyeTexture.DrawCentered(Position);
            pupilTexture.DrawCentered(Position + pupilPosition);
        }
        Components.Render();
    }
}
