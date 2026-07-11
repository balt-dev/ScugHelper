using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/DiscreteMovementController")]
public class DiscreteMovementController(EntityData data, Vector2 _) : Entity() {
    const int MaxTeleportCount = 128;
    readonly int steps = Math.Max(data.Int("Steps"), 1);
    internal bool InScene = false;

    static int Steps = 0; // There should never be more than one in a room
    static int HookSemaphore = 0;

    public override void Added(Scene scene) {
        base.Added(scene);
        Steps = steps;
        AcquireHookSemaphore();
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        ReleaseHookSemaphore();
    }
    public override void SceneEnd(Scene scene) {
        base.SceneEnd(scene);
        ReleaseHookSemaphore();
    }
    ~DiscreteMovementController() {
        ReleaseHookSemaphore();
    }

    void AcquireHookSemaphore() {
        if (InScene) return;
        if (HookSemaphore == 0) LoadLazyHooks();
        HookSemaphore++;
        InScene = true;
    }
    void ReleaseHookSemaphore() {
        if (!InScene) return;
        HookSemaphore--;
        if (HookSemaphore == 0) UnloadLazyHooks();
        InScene = false;
    }

    internal static void LoadLazyHooks() {
        On.Celeste.Actor.MoveHExact += OnMoveHExact;
        On.Celeste.Actor.MoveVExact += OnMoveVExact;
    }

    internal static void UnloadLazyHooks() {
        On.Celeste.Actor.MoveHExact -= OnMoveHExact;
        On.Celeste.Actor.MoveVExact -= OnMoveVExact;
    }

    private static bool OnMoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
        if (self.Scene is null) return false;
        if (MathF.Abs(self.movementCounter.X) > 0.5) self.movementCounter.X = Math.Sign(self.movementCounter.X) * 0.5f;
        Vector2 startPosition = self.Position;
        var allPortals = self.Scene.Tracker.GetEntities<MidStepPortals>();
        for (int i = 1; i <= Steps; i++) {
            Vector2 targetPosition = startPosition + Vector2.UnitX * (moveH * i / Steps);
            for (int j = 0; j < allPortals.Count; j++) {
                MidStepPortals portals = (MidStepPortals)allPortals[j];
                float dist = portals.EndPos.X - portals.StartPos.X;
                float step = dist - self.Width;
                if (portals.Orient != MidStepPortals.Orientation.Horizontal) continue;
                if (self.Top < portals.StartPos.Y + portals.PortalSize && self.Bottom > portals.StartPos.Y && portals.StartPos.X <= self.Left) {
                    int iters = 0;
                    while (portals.StartPos.X >= targetPosition.X - self.Width / 2) {
                        if (++iters > MaxTeleportCount) {
                            self.X = ((startPosition.X - self.Width / 2 + moveH - portals.StartPos.X) % step + step) % step + portals.StartPos.X + self.Width / 2;
                            return false;
                        }
                        self.X += step;
                        targetPosition.X += step;
                        self.NaiveMove(Vector2.UnitY * (portals.EndPos.Y - portals.StartPos.Y));
                        targetPosition.Y += portals.EndPos.Y - portals.StartPos.Y;
                        portals.OnTeleport();
                    }
                }
                if (self.Top < portals.EndPos.Y + portals.PortalSize && self.Bottom > portals.EndPos.Y && portals.EndPos.X >= self.Right) {
                    int iters = 0;
                    while (portals.EndPos.X <= targetPosition.X + self.Width / 2) {
                        if (++iters > MaxTeleportCount) {
                            self.X = ((startPosition.X - self.Width / 2 + moveH - portals.StartPos.X) % step + step) % step + portals.StartPos.X + self.Width / 2;
                            return false;
                        }
                        self.X -= step;
                        targetPosition.X -= step;
                        self.NaiveMove(Vector2.UnitY * (portals.StartPos.Y - portals.EndPos.Y));
                        targetPosition.Y += portals.StartPos.Y - portals.EndPos.Y;
                        portals.OnTeleport();
                    }
                }
            }
            Solid solid = self.CollideFirst<Solid>(targetPosition);
            if (solid != null) return orig(self, moveH / (Steps - i + 1), onCollide, pusher);
            self.Position = targetPosition;
        }

        return false;
    }

    private static bool OnMoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher) {
        if (self.Scene is null) return false;
        if (MathF.Abs(self.movementCounter.Y) > 0.5) self.movementCounter.Y = Math.Sign(self.movementCounter.Y) * 0.5f;
        Vector2 startPosition = self.Position;
        int moveSign = Math.Sign(moveV);
        var allPortals = self.Scene.Tracker.GetEntities<MidStepPortals>();
        for (int i = 1; i <= Steps; i++) {
            Vector2 targetPosition = startPosition + Vector2.UnitY * (moveV * i / Steps);
            for (int j = 0; j < allPortals.Count; j++) {
                MidStepPortals portals = (MidStepPortals)allPortals[j];
                float dist = portals.EndPos.Y - portals.StartPos.Y;
                float step = dist - self.Height;
                if (portals.Orient != MidStepPortals.Orientation.Vertical) continue;
                if (self.Left < portals.StartPos.X + portals.PortalSize && self.Right > portals.StartPos.X && portals.StartPos.Y <= self.Top) {
                    int iters = 0;
                    while (portals.StartPos.Y >= targetPosition.Y - self.Height / 2) {
                        if (++iters > MaxTeleportCount) {
                            self.Y = ((startPosition.Y - self.Height / 2 + moveV - portals.StartPos.Y) % step + step) % step + portals.StartPos.Y + self.Height / 2;
                            return false;
                        }
                        self.Y += step;
                        targetPosition.Y += step;
                        self.NaiveMove(Vector2.UnitX * (portals.EndPos.X - portals.StartPos.X));
                        targetPosition.X += portals.EndPos.X - portals.StartPos.X;
                        portals.OnTeleport();
                    }
                }
                if (self.Left < portals.EndPos.X + portals.PortalSize && self.Right > portals.EndPos.X && portals.EndPos.Y >= self.Bottom) {
                    int iters = 0;
                    while (portals.EndPos.Y <= targetPosition.Y + self.Height / 2) {
                        if (++iters > MaxTeleportCount) {
                            self.Y = ((startPosition.Y - self.Height / 2 + moveV - portals.StartPos.Y) % step + step) % step + portals.StartPos.Y + self.Height / 2;
                            return false;
                        }
                        self.Y -= step;
                        targetPosition.Y -= step;
                        self.NaiveMove(Vector2.UnitX * (portals.StartPos.X - portals.EndPos.X));
                        targetPosition.X += portals.StartPos.X - portals.EndPos.X;
                        portals.OnTeleport();
                    }
                }
            }
            Platform? platform = self.CollideFirst<Solid>(targetPosition);
            platform ??= self.CollideFirstOutside<JumpThru>(targetPosition + Vector2.UnitY * moveSign);
            if (platform != null) return orig(self, moveV / (Steps - i + 1), onCollide, pusher);
            self.Position = targetPosition;
        }

        return false;
    }
}
