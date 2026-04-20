using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/TeleportGate")]
public class TeleportGate(EntityData data, Vector2 offset) : AbstractGate(data, offset) {
    private Vector2 TeleportPosition = data.FirstNodeNullable(offset) ?? throw new NullReferenceException("Teleport gate does not have a teleport position node.");
    private readonly bool Silent = data.Bool("Silent", false);
    private readonly bool Invisible = data.Bool("Invisible", false);
    private readonly bool KeepX = data.Bool("KeepX", false);
    private readonly bool KeepY = data.Bool("KeepY", false);
    private readonly bool TeleportCamera = data.Bool("TeleportCamera", true);
    private readonly string Flag = data.String("Flag");
    private static readonly ParticleType pType = new(Player.P_DashA)
    {
        Color = Color.White,
        Color2 = Color.Transparent,
        FadeMode = ParticleType.FadeModes.Linear,
        SpeedMin = 0f,
        SpeedMax = 0f,
        Acceleration = Vector2.Zero,
        LifeMin = 1f,
        LifeMax = 3f,
    };

    public override void Update() {
        base.Update();
        if (Invisible) return;
        var startPos = Position - lineDir * Size / 2;
        var endPos = Position + lineDir * Size / 2;
        var particlePos = startPos + Calc.Random.NextFloat() * (endPos - startPos);
        SceneAs<Level>().ParticlesFG.Emit(pType, 1, particlePos, Vector2.Zero);
    }

    public override void OnTrigger(Player player)
    {
        Level level = SceneAs<Level>();
        if (Flag is string flag && !level.Session.GetFlag(flag)) return;
        Vector2 cameraPos = level.Camera.Position;
        if (!KeepX) {
            cameraPos.X += TeleportPosition.X - player.Position.X;
            player.PreviousPosition.X = player.Position.X = TeleportPosition.X;
        }
        if (!KeepY)
        {
            cameraPos.Y += TeleportPosition.Y - player.Position.Y;
            player.PreviousPosition.Y = player.Position.Y = TeleportPosition.Y;
        }
        if (TeleportCamera) {
            level.Camera.Position = cameraPos;
            level.Camera.X = Math.Clamp(level.Camera.X, level.Bounds.Left, level.Bounds.Right - (level.Camera.Right - level.Camera.Left));
            level.Camera.Y = Math.Clamp(level.Camera.Y, level.Bounds.Top, level.Bounds.Bottom - (level.Camera.Bottom - level.Camera.Top));
        }
        if (!Silent)
            Audio.Play("event:/char/badeline/disappear");
    }
}
