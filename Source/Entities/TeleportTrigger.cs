using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/TeleportTrigger")]
public class TeleportTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    private Vector2 TeleportPosition = data.FirstNodeNullable(offset) ?? throw new NullReferenceException("Teleport gate does not have a teleport position node.");
    private readonly bool Silent = data.Bool("Silent", false);
    private readonly string Flag = data.String("Flag");
    private readonly bool KeepX = data.Bool("KeepX", false);
    private readonly bool KeepY = data.Bool("KeepY", false);
    private readonly bool FlipFacing = data.Bool("FlipFacing", false);
    private readonly bool TeleportCamera = data.Bool("TeleportCamera", true);
    private string? LevelTPName;

    public override void OnEnter(Player player)
    {
        Level level = player.SceneAs<Level>();
        if (Flag is string flag && !level.Session.GetFlag(flag)) return;
        TeleportPlayer(player, ref LevelTPName, TeleportPosition, Silent, KeepX, KeepY, TeleportCamera, FlipFacing);
    }

    internal static void TeleportPlayer(Player player, ref string? LevelTPName, Vector2 TeleportPosition, bool Silent, bool KeepX, bool KeepY, bool TeleportCamera, bool FlipFacing)
    {
        Level level = player.SceneAs<Level>();
        if (LevelTPName == null)
        {
            foreach (LevelData levelData in level.Session.MapData.Levels)
            {
                if (levelData.Bounds.Contains(new Point((int)TeleportPosition.X, (int)TeleportPosition.Y)))
                {
                    LevelTPName = levelData.Name;
                    break;
                }
            }
            if (LevelTPName == null) { player.Die(Vector2.Zero); return; }
        }
        bool crossedLevels = level.Session.LevelData.Name != LevelTPName;

        Vector2 oldPos = player.Position;

        Vector2 cameraPos = level.Camera.Position;
        if (!KeepX || crossedLevels) {
            cameraPos.X += TeleportPosition.X - player.Position.X;
            float delta = TeleportPosition.X - player.Position.X;
            player.PreviousPosition.X = player.Position.X = TeleportPosition.X;
            for (int i = 0; i < player.Hair.Nodes.Count; i++)
                player.Hair.Nodes[i] = new(player.Hair.Nodes[i].X + delta, player.Hair.Nodes[i].Y);
            foreach (Follower follower in player.Leader.Followers)
                follower.Entity.Position = new(follower.Entity.Position.X + delta, follower.Entity.Position.Y);
            for (int i = 0; i < player.Leader.PastPoints.Count; i++)
                player.Leader.PastPoints[i] = new(player.Leader.PastPoints[i].X + delta, player.Leader.PastPoints[i].Y);
        }
        if (!KeepY || crossedLevels)
        {
            cameraPos.Y += TeleportPosition.Y - player.Position.Y;
            float delta = TeleportPosition.Y - player.Position.Y;
            player.PreviousPosition.Y = player.Position.Y = TeleportPosition.Y;
            for (int i = 0; i < player.Hair.Nodes.Count; i++)
                player.Hair.Nodes[i] = new(player.Hair.Nodes[i].X, player.Hair.Nodes[i].Y + delta);
            foreach (Follower follower in player.Leader.Followers)
                follower.Entity.Position = new(follower.Entity.Position.X, follower.Entity.Position.Y + delta);
            for (int i = 0; i < player.Leader.PastPoints.Count; i++)
                player.Leader.PastPoints[i] = new(player.Leader.PastPoints[i].X, player.Leader.PastPoints[i].Y + delta);
        }
        if (TeleportCamera || crossedLevels)
        {
            level.Camera.Position = cameraPos;
            level.Camera.X = Math.Clamp(level.Camera.X, level.Bounds.Left, level.Bounds.Right - (level.Camera.Right - level.Camera.Left));
            level.Camera.Y = Math.Clamp(level.Camera.Y, level.Bounds.Top, level.Bounds.Bottom - (level.Camera.Bottom - level.Camera.Top));
        }
        if (!Silent)
            Audio.Play("event:/char/badeline/disappear");
        if (FlipFacing)
        {
            player.Facing = (Facings)(-(int)player.Facing);
            player.Speed.X *= -1;
        }

        if (crossedLevels) {
            player.PreviousPosition = player.Position = oldPos;
            string name = LevelTPName;
            level.OnEndOfFrame += () =>
            {
                List<Follower> ents = [];
                foreach (Follower follower in player.Leader.Followers)
                {
                    level.Remove(follower.Entity);
                    ents.Add(follower);
                }
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = name;
                level.Session.RespawnPoint = level.GetSpawnPoint(TeleportPosition);
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.Transition);
                if (!KeepX) level.Camera.Position = new(TeleportPosition.X, level.Camera.Position.Y);
                if (!KeepY) level.Camera.Position = new(level.Camera.Position.X, TeleportPosition.Y);
                level.Camera.X = Math.Clamp(level.Camera.X, level.Bounds.Left, level.Bounds.Right - (level.Camera.Right - level.Camera.Left));
                level.Camera.Y = Math.Clamp(level.Camera.Y, level.Bounds.Top, level.Bounds.Bottom - (level.Camera.Bottom - level.Camera.Top));
                level.Add(player);
                foreach (Follower follower in ents) {
                    level.Add(follower.Entity);
                }
                if (!KeepX) player.Position.X = TeleportPosition.X;
                if (!KeepY) player.Position.Y = TeleportPosition.Y;
                level.Wipe?.Cancel();
                crossedLevels = true;
            };
        }
    }
}
