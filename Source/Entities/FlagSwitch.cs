using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/FlagSwitch")]
public class FlagSwitch : Entity {
    private readonly bool AllowRevert;
    private readonly bool State;
    private readonly string Flag;
    private readonly Sprite Sprite;

    private static bool PlaySounds = true;
    private const float Cooldown = 1.0f;
    private float CooldownTimer;
    private bool FlagState;
    private bool Flash;
    private Level level = null!;

    private bool Usable => AllowRevert || level.Session.GetFlag(Flag) != State;

    public FlagSwitch(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        AllowRevert = data.Bool("AllowRevert", true);
        State = data.Bool("State", true);
        Flash = data.Bool("Flash", true);
        Flag = data.String("Flag", "");

        Depth = 2000;
        Collider = new Hitbox(16f, 24f, -8f, -12f);
        Add(new PlayerCollider(OnPlayer));
        Add(Sprite = GFX.SpriteBank.Create("coreFlipSwitch"));
        Sprite.Path = data.String("SpritePath", "objects/coreFlipSwitch") + "/";
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        level = (scene as Level)!;
        FlagState = (scene as Level)!.Session.GetFlag(Flag);
        SetSprite(false);
        PlaySounds = true;
    }

    public void SetSprite(bool animate) {
        if (animate) {
            if (PlaySounds)
                Audio.Play(FlagState ? "event:/game/09_core/switch_to_hot" : "event:/game/09_core/switch_to_cold", Position);

            if (Usable)
                Sprite.Play(FlagState ? "hot" : "ice");
            else {
                if (PlaySounds)
                    Audio.Play("event:/game/09_core/switch_dies", Position);
                Sprite.Play(FlagState ? "hotOff" : "iceOff");
            }
            PlaySounds = false;
        } else if (Usable)
            Sprite.Play(FlagState ? "hotLoop" : "iceLoop");
        else
            Sprite.Play(FlagState ? "hotOffLoop" : "iceOffLoop");
    }

    public void OnPlayer(Player player) {
        if (Usable && CooldownTimer <= 0f) {
            PlaySounds = true;
            Level level = SceneAs<Level>();
            level.Session.SetFlag(Flag, !level.Session.GetFlag(Flag));

            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            if (Flash) level.Flash(Color.White * 0.15f, drawPlayerOver: true);
            Celeste.Freeze(0.05f);
            CooldownTimer = Cooldown;
        }
    }

    private bool LastFlagState;

    public override void Update() {
        PlaySounds = true;
        LastFlagState = FlagState;
        FlagState = level.Session.GetFlag(Flag);
        if (LastFlagState != FlagState) SetSprite(true);
        CooldownTimer -= Engine.DeltaTime;
        base.Update();
    }
}
