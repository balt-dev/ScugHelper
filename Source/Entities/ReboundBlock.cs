using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


[Tracked]
[CustomEntity("ScugHelper/ReboundBlock")]
public class ReboundBlock : Solid
{
    public enum ReboundBlockKind {
        Grey,
        Green,
        Pink
    }

    public ReboundBlockKind Kind { get; protected set; }

    protected bool spikesLeft;

    protected bool spikesRight;

    protected bool spikesUp;

    protected bool spikesDown;

    protected List<Image> renderImages;
    protected Image renderSlot;
    protected Sprite renderRefill;
    protected SoundSource firstHitSfx;
    protected BloomPoint bloom;
    protected VertexLight light;

    internal Vector2 Anchor;
    private static readonly float Displacement = 4.0f;

    public ReboundBlock(Vector2 position, float width, float height, ReboundBlockKind kind)
     : base(position, width, height, safe: true)
    {
        Anchor = position;
        Kind = kind;
        switch (kind)
        {
            case ReboundBlockKind.Grey:
                renderImages = BuildSprite(GFX.Game["objects/reboundBlock/zeroBlock"]);
                renderSlot = new(GFX.Game["objects/reboundBlock/zeroSlot"]);
                renderRefill = null;
                break;
            case ReboundBlockKind.Green:
                renderImages = BuildSprite(GFX.Game["objects/reboundBlock/oneBlock"]);
                renderSlot = new(GFX.Game["objects/reboundBlock/oneSlot"]);
                renderRefill = new Sprite(GFX.Game, "objects/refill/idle");
                break;
            case ReboundBlockKind.Pink:
                renderImages = BuildSprite(GFX.Game["objects/reboundBlock/twoBlock"]);
                renderSlot = new(GFX.Game["objects/reboundBlock/twoSlot"]);
                renderRefill = new Sprite(GFX.Game, "objects/refillTwo/idle");
                break;
        }
        renderRefill?.AddLoop("idle", "", 0.1f);
        renderRefill?.Play("idle");
        renderRefill?.CenterOrigin();

        OnDashCollide = Dashed;
    }

    public override void Render()
    {
        foreach (Image image in renderImages)
            image.DrawSimpleOutline();
        base.Render();
        renderSlot.Render();
        renderRefill?.DrawSimpleOutline();
        renderRefill?.Render();
    }

    public ReboundBlock(EntityData e, Vector2 levelOffset)
     : this(e.Position + levelOffset, e.Width, e.Height, e.Enum<ReboundBlockKind>("kind"))
    {
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        RecenterImages();
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        light.Alpha = 1;
        light.InSolidAlphaMultiplier = 1;
    }

    private List<Image> BuildSprite(MTexture source)
    {
        List<Image> list = [];
        int num = source.Width / 8;
        int num2 = source.Height / 8;
        for (int i = 0; i < Width; i += 8)
            for (int j = 0; j < Height; j += 8)
            {
                int num3 = (i != 0) ? ((!(i >= Width - 8f)) ? Calc.Random.Next(1, num - 1) : (num - 1)) : 0;
                int num4 = (j != 0) ? ((!(j >= Height - 8f)) ? Calc.Random.Next(1, num2 - 1) : (num2 - 1)) : 0;
                Image image = new(source.GetSubtexture(num3 * 8, num4 * 8, 8, 8)) { Position = new Vector2(i, j) };
                list.Add(image);
                Add(image);
            }
        return list;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        spikesUp = CollideCheck<Spikes>(Position - Vector2.UnitY);
        spikesDown = CollideCheck<Spikes>(Position + Vector2.UnitY);
        spikesLeft = CollideCheck<Spikes>(Position - Vector2.UnitX);
        spikesRight = CollideCheck<Spikes>(Position + Vector2.UnitX);
    }

    public DashCollisionResults Dashed(Player player, Vector2 dir)
    {
        if (!SaveData.Instance.Assists.Invincible)
        {
            if (dir == Vector2.UnitX && spikesLeft)
                return DashCollisionResults.NormalCollision;
            if (dir == -Vector2.UnitX && spikesRight)
                return DashCollisionResults.NormalCollision;
            if (dir == Vector2.UnitY && spikesUp)
                return DashCollisionResults.NormalCollision;
            if (dir == -Vector2.UnitY && spikesDown)
                return DashCollisionResults.NormalCollision;
        }
        (Scene as Level).DirectionalShake(dir);
        MoveTo(Position + dir * Displacement);
        SmashParticles(-dir);
        Celeste.Celeste.Freeze(0.1f);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

        switch (Kind) {
            case ReboundBlockKind.Pink:
                Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch", Position);
                player.RefillDash();
                player.Dashes = Math.Max(player.Dashes, 2);
                break;
            case ReboundBlockKind.Green:
                Audio.Play("event:/game/general/diamond_touch", Position);
                player.RefillDash();
                break;
        }
        Add(new Coroutine(SoundRoutine()));
        player.RefillStamina();
        return DashCollisionResults.Rebound;
    }

    private IEnumerator SoundRoutine() {
        var evInstance = Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_1", Position);
		yield return 0.18f;
        evInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    protected void SmashParticles(Vector2 dir)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        int num;
        if (dir == Vector2.UnitX)
        {
            direction = 0f;
            position = CenterRight - Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
            num = (int)(Height / 8f) ;
        }
        else if (dir == -Vector2.UnitX)
        {
            direction = MathF.PI;
            position = CenterLeft + Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
            num = (int)(Height / 8f) ;
        }
        else if (dir == Vector2.UnitY)
        {
            direction = MathF.PI / 2f;
            position = BottomCenter - Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
            num = (int)(Width / 8f) ;
        }
        else
        {
            direction = -MathF.PI / 2f;
            position = TopCenter + Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
            num = (int)(Width / 8f) ;
        }
        num += 2;
        SceneAs<Level>().Particles.Emit(Kind == ReboundBlockKind.Pink ? Refill.P_ShatterTwo : Refill.P_Shatter, num, position, positionRange, direction);
    }

    public override void Update()
    {
        base.Update();
        Vector2 target = Calc.Approach(Position, Anchor, 1);
        light.Position = Center;
        bloom.Position = Center;
        MoveTo(target);
        RecenterImages();
        foreach (Image image in renderImages)
            image.Update();
        renderSlot?.Update();
        renderRefill?.Update();
    }

    private void RecenterImages()
    {
        bloom?.Position = Center;
        light?.Position = Center;
        renderSlot?.Position = Center - new Vector2(renderSlot.Width, renderSlot.Height) / 2;
        renderRefill?.Position = Center;
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.Circle(Anchor, 4, Color.Red, 8);
    }
}
