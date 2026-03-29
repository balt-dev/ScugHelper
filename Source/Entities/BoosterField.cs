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
    private static readonly float BouncePulseLength = 0.2f;

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

    public override void Render()
    {
        if (!Invisible) {
            Draw.Rect(X, Y, Width, Height, Color.White * (BounceTimer / BouncePulseLength * 0.4f));
            Draw.Rect(X, Y, Width, Height, Color.Coral * 0.3f);
            foreach (Vector2 particle in particles)
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, Color.White * 0.7f);
            Draw.HollowRect(X, Y, Width, Height, Color.White * 0.5f);
        }

        base.Render();
    }


    public override void Added(Scene scene)
    {
        base.Added(scene);
        Add(new CustomBloom(OnRenderBloom));
    }

    public override void Update()
    {
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
            Draw.Rect(X, Y, Width, Height, Color.Coral * 0.3f);
    }
}
