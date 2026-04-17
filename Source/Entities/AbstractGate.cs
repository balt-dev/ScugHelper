using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.ScugHelper;
using MonoMod.Cil;
using System;
using Celeste.Mod;

namespace Celeste.Mod.ScugHelper.Entities;

/// <summary>
/// Abstract class for all Gate entities. These gates run a callback when crossed by the player, using line intersection collision between their previous and current position.
/// </summary>
[Tracked(true)]
public abstract class AbstractGate : Entity
{
    /// <summary>
    /// The angle at which the gate points, in radians.
    /// </summary>
    public float Angle;
    /// <summary>
    /// The diameter of the gate, in units.
    /// </summary>
    public float Size;

    protected Vector2 lineDir;
    protected Vector2 lineNorm;

    public AbstractGate(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Angle = (float)(data.Float("Angle") * Math.PI / 180);
        Size = data.Float("Size");
        lineDir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
        lineNorm = new Vector2((float)-Math.Sin(Angle), (float)Math.Cos(Angle));
    }

    /// <summary>
    /// The method to call when this gate is crossed.
    /// </summary>
    public abstract void OnTrigger(Player player);

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

    public override void Update() {
        base.Update();
        Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
        if (player == null) return;
        if (CheckLine(player.PreviousPosition, player.Position))
            OnTrigger(player);
        else if (CheckLine(player.PreviousPosition + player.TopCenter - player.Position, player.TopCenter))
            OnTrigger(player);
    }
}
