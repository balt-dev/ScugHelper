using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using System;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/MidStepPortals")]
public class MidStepPortals : Entity
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public readonly Vector2 StartPos;
    public readonly Vector2 EndPos;
    public readonly Orientation Orient;
    public readonly float PortalSize;
    public readonly bool Silent;
    bool ShouldPlaySound;
    private float SoundTimer;
    private const float SoundCooldown = 0.1f;

    public MidStepPortals(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Depth = -150000;
        StartPos = Position;
        Orient = data.Enum("Orientation", Orientation.Horizontal);
        var endOffset = data.Int("Offset", 0);
        EndPos = Position
            + (Orient == Orientation.Horizontal ? new Vector2(data.Width, endOffset) : new Vector2(endOffset, data.Height));
        PortalSize = Orient == Orientation.Horizontal ? data.Height : data.Width;
        Logger.Log(nameof(ScugHelper), $"Portal size: {PortalSize}");
        Silent = data.Bool("Silent", false);
        Visible = !data.Bool("Invisible", false);
    }

    public override void Update() {
        base.Update();
        SoundTimer -= Engine.RawDeltaTime;
        if (ShouldPlaySound && SoundTimer <= 0f) {
            ShouldPlaySound = false;
            SoundTimer = SoundCooldown;
            Audio.Play("event:/char/badeline/disappear");
        }
        if (Visible) {
            if (Orient == Orientation.Horizontal) {
                var particlePos = StartPos.Y + Calc.Random.NextFloat() * PortalSize + 1;
                SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(StartPos.X + 1, particlePos), Vector2.Zero);
                var particlePos2 = EndPos.Y + Calc.Random.NextFloat() * PortalSize + 1;
                SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(EndPos.X, particlePos2), Vector2.Zero);
            } else {
                var particlePos = StartPos.X + Calc.Random.NextFloat() * PortalSize + 1;
                SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(particlePos, StartPos.Y + 1), Vector2.Zero);
                var particlePos2 = EndPos.X + Calc.Random.NextFloat() * PortalSize + 1;
                SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(particlePos2, EndPos.Y), Vector2.Zero);
            }
        }
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        if (Orient == Orientation.Horizontal) {
            Draw.Line(StartPos + Vector2.UnitX, StartPos + new Vector2(1, PortalSize), Color.Green);
            Draw.Line(EndPos, EndPos + new Vector2(0, PortalSize), Color.Yellow);
        } else {
            Draw.Line(StartPos, StartPos + new Vector2(PortalSize, 0), Color.Green);
            Draw.Line(EndPos - Vector2.UnitY, EndPos + new Vector2(PortalSize, -1), Color.Yellow);
        }
    }

    private void OnTeleport() {
        if (!Silent) ShouldPlaySound = true;
    }

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Actor.MoveHExact += OnMoveHExact;
        On.Celeste.Actor.MoveVExact += OnMoveVExact;
    }
    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Actor.MoveHExact -= OnMoveHExact;
        On.Celeste.Actor.MoveVExact -= OnMoveVExact;
    }


    private static bool OnMoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher)
        => self.Scene.Tracker.GetEntity<MidStepPortals>() is null
            ? orig(self, moveV, onCollide, pusher)
            : ClobberedMoveVExact(self, moveV, onCollide, pusher);

    private static bool OnMoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
        => self.Scene.Tracker.GetEntity<MidStepPortals>() is null
            ? orig(self, moveH, onCollide, pusher)
            : ClobberedMoveHExact(self, moveH, onCollide, pusher);

    public static bool ClobberedMoveHExact(Actor self, int moveH, Collision? onCollide = null, Solid? pusher = null) {

        Vector2 targetPosition = self.Position + Vector2.UnitX * moveH;
        int moveDir = Math.Sign(moveH);
        int moveAmount = 0;
        var allPortals = self.Scene.Tracker.GetEntities<MidStepPortals>();
        while (moveH != 0) {
            for (int i = 0; i < allPortals.Count; i++) {
                MidStepPortals portals = (MidStepPortals)allPortals[i];
                if (portals.Orient != Orientation.Horizontal) continue;
                if (
                    moveH < 0
                    && self.Left > portals.StartPos.X && self.Left + moveDir <= portals.StartPos.X
                    && !(self.Top >= portals.StartPos.Y + portals.PortalSize || self.Bottom <= portals.StartPos.Y)
                ) {
                    self.Position.X = portals.EndPos.X - self.Width / 2;
                    self.MoveV(portals.EndPos.Y - portals.StartPos.Y);
                    portals.OnTeleport();
                } else if (
                    moveH > 0
                    && self.Right < portals.EndPos.X && self.Right + moveDir >= portals.EndPos.X
                    && !(self.Top >= portals.EndPos.Y + portals.PortalSize || self.Bottom <= portals.EndPos.Y)
                ) {
                    self.Position.X = portals.StartPos.X + self.Width / 2;
                    self.MoveV(portals.StartPos.Y - portals.EndPos.Y);
                    portals.OnTeleport();
                }
            }

            Solid solid = self.CollideFirst<Solid>(self.Position + Vector2.UnitX * moveDir);
            if (solid != null) {
                self.movementCounter.X = 0f;
                onCollide?.Invoke(new CollisionData
                {
                    Direction = Vector2.UnitX * moveDir,
                    Moved = Vector2.UnitX * moveAmount,
                    TargetPosition = targetPosition,
                    Hit = solid,
                    Pusher = pusher
                });
                return true;
            }

            moveAmount += moveDir;
            moveH -= moveDir;
            self.X += moveDir;
        }

        return false;
    }

    public static bool ClobberedMoveVExact(Actor self, int moveV, Collision? onCollide = null, Solid? pusher = null) {
        Vector2 targetPosition = self.Position + Vector2.UnitY * moveV;
        int moveDir = Math.Sign(moveV);
        int moveAmount = 0;
        var allPortals = self.Scene.Tracker.GetEntities<MidStepPortals>();
        while (moveV != 0) {
            for (int i = 0; i < allPortals.Count; i++) {
                MidStepPortals portals = (MidStepPortals)allPortals[i];
                if (portals.Orient != Orientation.Vertical) continue;
                if (
                    moveV < 0
                    && self.Top > portals.StartPos.Y && self.Top + moveDir <= portals.StartPos.Y
                    && !(self.Left >= portals.StartPos.X + portals.PortalSize || self.Right <= portals.StartPos.X)
                ) {
                    self.Position.Y = portals.EndPos.Y;
                    self.MoveH(portals.EndPos.X - portals.StartPos.X);
                    portals.OnTeleport();
                } else if (
                    moveV > 0
                    && self.Bottom < portals.EndPos.Y && self.Bottom + moveDir >= portals.EndPos.Y
                    && !(self.Left >= portals.EndPos.X + portals.PortalSize || self.Right <= portals.EndPos.X)
                ) {
                    self.Position.Y = portals.StartPos.Y + self.Height + 1;
                    self.MoveH(portals.StartPos.X - portals.EndPos.X);
                    portals.OnTeleport();
                }
            }
            Platform platform = self.CollideFirst<Solid>(self.Position + Vector2.UnitY * moveDir);
            if (platform != null) {
                self.movementCounter.Y = 0f;
                onCollide?.Invoke(new CollisionData
                {
                    Direction = Vector2.UnitY * moveDir,
                    Moved = Vector2.UnitY * moveAmount,
                    TargetPosition = targetPosition,
                    Hit = platform,
                    Pusher = pusher
                });
                return true;
            }

            if (moveV > 0 && !self.IgnoreJumpThrus) {
                platform = self.CollideFirstOutside<JumpThru>(self.Position + Vector2.UnitY * moveDir);
                if (platform != null) {
                    self.movementCounter.Y = 0f;
                    onCollide?.Invoke(new CollisionData
                    {
                        Direction = Vector2.UnitY * moveDir,
                        Moved = Vector2.UnitY * moveAmount,
                        TargetPosition = targetPosition,
                        Hit = platform,
                        Pusher = pusher
                    });
                    return true;
                }
            }

            moveAmount += moveDir;
            moveV -= moveDir;
            self.Y += moveDir;
        }

        return false;
    }
}

#nullable restore
