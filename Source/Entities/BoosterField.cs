using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;

[Tracked]
[CustomEntity("ScugHelper/BoosterField")]
public class BoosterField : Solid
{

    protected float[] speeds = [12f, 20f, 40f];
    protected List<Vector2> particles = [];
    private bool BouncedBooster;
    private bool Invisible;
    private float BounceTimer;
    private static readonly float BouncePulseLength = 0.8f;

    public BoosterField(Vector2 position, float width, float height, bool invis) : base(position, width, height, false)
    {
        Collidable = true;
        Invisible = invis;
        for (int i = 0; i < Width * Height / 24f; i++)
            particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
    }

    public BoosterField(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Bool("invisible"))
    { }
    
    private static readonly float SineMovement = 2.0f;

    public override void Render()
    {
        if (!Invisible)
        {
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.Coral * 0.3f);
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * (BounceTimer / BouncePulseLength * 0.4f));
            WobblyHelper.RenderOutline(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.5f);
            foreach (Vector2 particle in particles)
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, Color.White * 0.7f);
        }

        base.Render();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Add(new CustomBloom(OnRenderBloom));
    }

    private float Elapsed = 0;
    public override void Update()
    {
        Elapsed += Engine.DeltaTime;
        if (BouncedBooster) {
            BounceTimer = BouncePulseLength;
            BouncedBooster = false;
        } else {
            BounceTimer = Math.Max(0.0f, BounceTimer - Engine.DeltaTime);
        }
        int num = speeds.Length;
        float height = Height;
        int i = 0;
        for (int count = particles.Count; i < count; i++)
        {
            Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
            value.Y %= height - 1f;
            particles[i] = value;
        }
        base.Update();
    }

    public static void LoadHooks()
    {
        On.Monocle.Collide.Check_Entity_Entity += BoosterCollide;
    }
    public static void UnloadHooks()
    {
        On.Monocle.Collide.Check_Entity_Entity -= BoosterCollide;
    }

    private static bool BoosterCollide(On.Monocle.Collide.orig_Check_Entity_Entity orig, Entity a, Entity b)
    {
        if (a is BoosterField field)
        {
            if (b is not Player player) return false;
            if (player.LastBooster?.BoostingPlayer ?? false) {
                var result = orig(a, b);
                field.BouncedBooster |= result;
                return result;
            }
            return false;
        }
        else if (b is BoosterField)
        {
            return BoosterCollide(orig, b, a);
        }
        return orig(a, b);
    }
    
    public void OnRenderBloom()
    {
        if (Visible && !Invisible) // lol
            WobblyHelper.RenderFill(Collider.Bounds, Elapsed, SineMovement, 2f * (1 - (BounceTimer / BouncePulseLength)), Color.White * 0.3f);
    }
}
