using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;

[TrackedAs(typeof(TempleEye))]
[CustomEntity("ScugHelper/FlagTempleEye")]
public class FlagTempleEye(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    private MTexture eyeTexture;
    private MTexture pupilTexture;
    private Sprite eyelid;
    private Vector2 pupilPosition;
    private Vector2 pupilTarget;
    private float blinkTimer;
    private bool bursting;
    private bool isBG;
    private readonly bool trackPlayer = data.Bool("trackPlayer");
    private readonly string flagToCheck = data.String("flag");

    public override void Added(Scene scene)
    {
        base.Added(scene);
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

        eyelid.AddLoop("opened", "", 0f, [0]);
        eyelid.Add("blink", "", 0.08f, "opened", 0, 1, 1, 2, 3, 0);
        eyelid.AddLoop("closed", "", 0f, [1]);
        eyelid.Add("open", "", 0.08f, "opened", 1, 2, 3, 0);
        eyelid.Add("close", "", 0.08f, "closed", 0, 3, 2, 1);
        if (SceneAs<Level>().Session.Flags.Contains(flagToCheck))
            eyelid.Play("opened");
        else
            eyelid.Play("closed");

        eyelid.CenterOrigin();
        SetBlinkTimer();
    }

    private void SetBlinkTimer()
    {
        blinkTimer = Calc.Random.Range(1f, 15f);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
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
            if (SceneAs<Level>().Session.Flags.Contains(flagToCheck) && eyelid.CurrentAnimationID == "closed")
                eyelid.Play("open");
            else if (!SceneAs<Level>().Session.Flags.Contains(flagToCheck) && (eyelid.CurrentAnimationID == "opened" || eyelid.CurrentAnimationID == "blink"))
                eyelid.Play("close");

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

        base.Update();
    }

    private void TryBlink()
    {
        if (eyelid.CurrentAnimationID == "opened") eyelid.Play("blink");
    }

    public void Burst()
    {
        bursting = true;
        Sprite sprite = new(GFX.Game, isBG ? "scenery/temple/eye/bg_burst" : "scenery/temple/eye/fg_burst");
        sprite.Add("burst", "", 0.08f);
        sprite.Play("burst");
        sprite.OnLastFrame = (_) => RemoveSelf();
        sprite.CenterOrigin();
        Add(sprite);
        Remove(eyelid);
    }

    public override void Render()
    {
        if (!bursting)
        {
            eyeTexture.DrawCentered(Position);
            pupilTexture.DrawCentered(Position + pupilPosition);
        }

        base.Render();
    }
}
