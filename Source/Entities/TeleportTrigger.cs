using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/TeleportTrigger")]
public class TeleportTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    private Vector2 TeleportPosition = data.FirstNodeNullable(offset) ?? throw new NullReferenceException("Teleport gate does not have a teleport position node.");
    private readonly bool Silent = data.Bool("Silent", false);
    private readonly string Flag = data.String("Flag");
    private readonly bool KeepX = data.Bool("KeepX", false);
    private readonly bool KeepY = data.Bool("KeepY", false);
    private readonly bool TeleportCamera = data.Bool("TeleportCamera", true);

    public override void OnEnter(Player player)
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
