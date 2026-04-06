using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections.Generic;
using MonoMod.Cil;

[Tracked]
[CustomEntity("ScugHelper/SquareGem")]
public class SquareCrystal : Entity
{
    public int GemID;
    public EntityID GID;
    public Color InfillColor { get; protected set; }

    private Sprite fillSprite;
    private Sprite glareSprite;
    private Wiggler scaleWiggler;
    private Wiggler moveWiggler;
    private float bounceSfxDelay;
    private Vector2 moveWiggleDir;
    private float moveWiggleStart;
    private static readonly float FRAC_SQRT_2_2 = (float)(Math.Sqrt(2.0) / 2);
    private static readonly float wiggleSettleSpeed = 0.6f;
    private BloomPoint bloom;
    private VertexLight light;

    public SquareCrystal(EntityData data, Vector2 offset, EntityID gid)
    : base(data.Position + offset)
    {
        HeartGem _ = null;
        Depth = -15;
        moveWiggleStart = 0.0f;
        InfillColor = data.HexColor("Color", Color.White);
        Collider = new Hitbox(18f, 18f, -9f, -9f);

        Add(scaleWiggler = Wiggler.Create(0.5f, 4f, f => {
            fillSprite.Scale = Vector2.One * (1f + f * 0.3f);
            glareSprite.Scale = Vector2.One * (1f + f * 0.3f);
        }));
        moveWiggler = Wiggler.Create(0.8f, 2f);
        moveWiggler.StartZero = true;
        Add(moveWiggler);
        Add(new PlayerCollider(OnPlayer));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        fillSprite = GFX.SpriteBank.Create("squareGemFill");
        fillSprite.Color = InfillColor;
        Add(fillSprite);
        glareSprite = GFX.SpriteBank.Create("squareGemOutline");
        glareSprite.Color = Color.White;
        Add(glareSprite);
        fillSprite.Play("spin");
        glareSprite.Play("spin");
        fillSprite.CenterOrigin();
        glareSprite.CenterOrigin();
        Add(bloom = new BloomPoint(0.35f, 24f));
        Add(light = new VertexLight(InfillColor, 0.3f, 32, 64));
    }

    public void OnPlayer(Player player)
    {
        Vector2 deltaDir = (player.Center - Center).SafeNormalize(Vector2.UnitY);

        if (Math.Abs(deltaDir.X) > FRAC_SQRT_2_2)
            player.PointBounce(new(Center.X, player.Y));
        else
        {
            player.PointBounce(new(player.X, Center.Y));
            player.Speed.X = 0;
        }

        moveWiggler.Start();
        scaleWiggler.Start();
        fillSprite.Play("spin", restart: true);
        glareSprite.Play("spin", restart: true);
        moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
        moveWiggleStart = Scene.TimeActive;
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        if (bounceSfxDelay <= 0f)
        {
            Audio.Play("event:/game/general/crystalheart_bounce", Position);
            bounceSfxDelay = 0.1f;
        }
    }

    public override void Update() {
        base.Update();
        bounceSfxDelay -= Engine.DeltaTime;
        float wiggleFac = wiggleSettleSpeed / (1f + (Scene.TimeActive - moveWiggleStart));
        fillSprite.Position = wiggleFac * moveWiggleDir * moveWiggler.Value * -8f;
        glareSprite.Position = wiggleFac * moveWiggleDir * moveWiggler.Value * -8f;
    }
}
