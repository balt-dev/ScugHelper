using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/TeleportTrigger")]
public class TeleportTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    private Vector2 TeleportPosition = data.FirstNodeNullable(offset) ?? throw new NullReferenceException("Teleport gate does not have a teleport position node.");
    private readonly float Delay = data.Float("Delay", 0.0f);
    private readonly bool ReloadRoom = data.Bool("ReloadRoom", false);
    private readonly bool Silent = data.Bool("Silent", false);
    private readonly string Flag = data.String("Flag");
    private readonly bool KeepX = data.Bool("KeepX", false);
    private readonly bool KeepY = data.Bool("KeepY", false);
    private readonly bool FlipFacing = data.Bool("FlipFacing", false);
    private readonly bool TeleportCamera = data.Bool("TeleportCamera", true);

    public override void OnEnter(Player player) {
        Level level = player.SceneAs<Level>();
        if (Flag is string flag && !level.Session.GetFlag(flag)) return;
        Add(new Coroutine(TeleportPlayer(player, TeleportPosition, Silent, KeepX, KeepY, TeleportCamera, FlipFacing, Delay, ReloadRoom)));
    }

    internal static IEnumerator TeleportPlayer(Player player, Vector2 TeleportPosition, bool Silent, bool KeepX, bool KeepY, bool TeleportCamera, bool FlipFacing, float Delay, bool ReloadRoom) {
        Level level = player.SceneAs<Level>();
        string? LevelTPName = null;
        foreach (LevelData levelData in level.Session.MapData.Levels) {
            if (levelData.Bounds.Contains(new Point((int)TeleportPosition.X, (int)TeleportPosition.Y))) {
                LevelTPName = levelData.Name;
                break;
            }
        }
        LevelTPName ??= level.Session.LevelData.Name;

        if (Delay > 0.0f) yield return Delay;

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
        if (!KeepY || crossedLevels) {
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
        if (TeleportCamera && !crossedLevels) {
            level.Camera.Position = cameraPos;
            level.Camera.X = Math.Clamp(level.Camera.X, level.Bounds.Left, level.Bounds.Right - (level.Camera.Right - level.Camera.Left));
            level.Camera.Y = Math.Clamp(level.Camera.Y, level.Bounds.Top, level.Bounds.Bottom - (level.Camera.Bottom - level.Camera.Top));
        }
        if (!Silent)
            Audio.Play("event:/char/badeline/disappear");
        if (FlipFacing) {
            player.Facing = (Facings)(-(int)player.Facing);
            player.Speed.X *= -1;
        }

        if (crossedLevels || ReloadRoom) {
            player.PreviousPosition = player.Position = oldPos;
            string name = LevelTPName;
            foreach (Follower follower in player.Leader.Followers) {
                follower.Entity.AddTag(Tags.Global);
				level.Session.DoNotLoad.Add(follower.ParentEntityID);
            }
           	
            level.OnEndOfFrame += () => {
                player.CleanUpTriggers();
                List<Follower> ents = [];
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = name;
                level.Session.RespawnPoint = level.GetSpawnPoint(TeleportPosition);
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.Transition);
                if (!KeepX) level.Camera.Position = new(TeleportPosition.X - (level.Camera.Right - level.Camera.Left) / 2, level.Camera.Position.Y);
                if (!KeepY) level.Camera.Position = new(level.Camera.Position.X, TeleportPosition.Y - (level.Camera.Bottom - level.Camera.Top) / 2);
                level.Camera.X = Math.Clamp(level.Camera.X, level.Bounds.Left, level.Bounds.Right - (level.Camera.Right - level.Camera.Left));
                level.Camera.Y = Math.Clamp(level.Camera.Y, level.Bounds.Top, level.Bounds.Bottom - (level.Camera.Bottom - level.Camera.Top));
                level.Add(player);
                if (!KeepX) player.Position.X = TeleportPosition.X;
                if (!KeepY) player.Position.Y = TeleportPosition.Y;
                foreach (Follower follower in player.Leader.Followers) {
    				follower.Entity.Position = player.Position;
    				follower.Entity.RemoveTag(Tags.Global);
    				level.Session.DoNotLoad.Remove(follower.ParentEntityID);
                }
                for (int i = 0; i < player.Leader.PastPoints.Count; i++)
              		player.Leader.PastPoints[i] = player.Position;
                player.Leader.TransferFollowers();
                level.Wipe?.Cancel();
                crossedLevels = true;
            };
        }
    }
}
