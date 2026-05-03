using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.ScugHelper.Entities;


[TrackedAs(typeof(SeekerBarrier))]
[CustomEntity("ScugHelper/MovingSeekerBarrier")]
public class MovingSeekerBarrier : SeekerBarrier
{
    readonly Vector2 Start;
    readonly Vector2 End;
    readonly float Period;

    public MovingSeekerBarrier(EntityData data, Vector2 offset) : base(data, offset)
    {
        Start = data.Position + offset;
        End = data.NodesOffset(offset)[0];
        Period = data.Float("Period", 2f);
        Add(new SineWave(1f / Period) { OnUpdate = OnSineUpdate });
    }

    private void OnSineUpdate(float sine)
    {
        float fac = (1 + sine) / 2;
        Vector2 pos = Vector2.Lerp(Start, End, fac);
        bool pleeker = Scene.Tracker.GetEntity<Player>()?.Get<PlayerSeekerComponent>() is not null;
        if (pleeker) Collidable = true;
        MoveToX(pos.X);
        MoveToY(pos.Y);
        Collidable = false;
    }
}
#nullable restore
