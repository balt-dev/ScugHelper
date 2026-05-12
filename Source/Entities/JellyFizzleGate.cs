using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/GliderFizzleGate")]
public class GliderFizzleGate : AbstractGate
{
    private static readonly ParticleType pType = new(Player.P_DashA)
    {
        Color = Color.White,
        Color2 = Color.Transparent,
        FadeMode = ParticleType.FadeModes.Linear,
        SpeedMin = 0f,
        SpeedMax = 0f,
        Acceleration = Vector2.Zero,
        LifeMin = 0.5f,
        LifeMax = 1f,
    };

    public GliderFizzleGate(EntityData data, Vector2 offset) : base(data, offset)
    {
        Add(new CustomBloom(RenderBloom));
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.Line(Position - lineDir * Size / 2, Position + lineDir * Size / 2, Color.Cyan);
    }

    private bool CheckLine(Vector2 a, Vector2 b)
    {
        var prevPos = a;
        var delta = b - prevPos;
        var d1 = Vector2.Dot(prevPos - Position, lineNorm);
        var d2 = Vector2.Dot(prevPos + delta - Position, lineNorm);
        if (d1 * d2 > 0)
            return false;
        var t = d1 / (d1 - d2);
        var crossPoint = prevPos + t * delta;
        var proj = Vector2.Dot(crossPoint - Position, lineDir);
        return Math.Abs(proj) <= Size / 2;
    }

    private static void Fizzle(Glider self)
    {
        self.destroyed = true;
        self.Collidable = false;
        if (self.Hold.IsHeld)
        {
            Vector2 speed = self.Hold.Holder.Speed;
            self.Hold.Holder.Drop();
            self.Speed = speed * 0.333f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }

        self.Add(new Coroutine(self.DestroyAnimationRoutine()));
    }

    [OnLoad]
    public static void LoadHooks()
    {
        On.Celeste.Glider.Update += OnGliderUpdate;
    }

    [OnUnload]
    public static void UnloadHooks()
    {
        On.Celeste.Glider.Update -= OnGliderUpdate;
    }

    // We do this here instead of in our own update to heavily reduce the amount of entities we need to check.
    // It's still O(n^2), but the constant factor is massively reduced, since Gliders aren't marked as [Tracked].
    private static void OnGliderUpdate(On.Celeste.Glider.orig_Update orig, Glider self)
    {
        var oldPos = self.Position;
        orig(self);
        var newPos = self.Position;
        if (self.destroyed) return;

        foreach (GliderFizzleGate gate in self.Scene.Tracker.GetEntities<GliderFizzleGate>())
        {
            if (gate.CheckLine(oldPos, newPos))
            { Fizzle(self); break; }
            else
            {
                if (self.IsInverted())
                {
                    if (gate.CheckLine(oldPos + self.BottomCenter - newPos, self.BottomCenter))
                    { Fizzle(self); break; }
                }
                else
                {
                    if (gate.CheckLine(oldPos + self.TopCenter - newPos, self.TopCenter))
                    { Fizzle(self); break; }
                }
            }
        }
    }

    public override void Update()
    {
        base.Update();

        if (Scene.OnInterval(0.02f))
        {
            for (int i = 0; i < Size / 64; i++)
            {
                var startPos = Position - lineDir * Size / 2;
                var endPos = Position + lineDir * Size / 2;
                var particlePos = startPos + Calc.Random.NextFloat() * (endPos - startPos);
                SceneAs<Level>().ParticlesFG.Emit(pType, 1, new(MathF.Floor(particlePos.X), MathF.Floor(particlePos.Y)), Vector2.One * 2);
            }
        }
    }

    public override void Render()
    {
        var startPos = Position - lineDir * Size / 2;
        var endPos = Position + lineDir * Size / 2;
        Draw.Line(startPos, endPos, pType.Color * 0.2f, 3);
    }
    
    public void RenderBloom()
    {
        var startPos = Position - lineDir * Size / 2;
        var endPos = Position + lineDir * Size / 2;
        Draw.Line(startPos, endPos, pType.Color, 3);
    }

    public override void OnTrigger(Player player)
    {
        if (player.Holding?.Entity is Glider glider) Fizzle(glider);
    }
}
