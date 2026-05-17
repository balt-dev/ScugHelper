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
    public readonly float StartX;
    public readonly float EndX;
    public readonly float PortalHeight;
    public readonly bool Silent;
    bool ShouldPlaySound;
    private float SoundTimer;
    private const float SoundCooldown = 0.1f;

    public MidStepPortals(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        StartX = Position.X;
        EndX = (data.FirstNodeNullable(offset) ?? Position).X;
        PortalHeight = data.Height;
        Silent = data.Bool("Silent", false);
        Visible = !data.Bool("Invisible", false);
    }

    public override void Update()
    {
        base.Update();
        SoundTimer -= Engine.RawDeltaTime;
        if (ShouldPlaySound && SoundTimer <= 0f) {
            ShouldPlaySound = false;
            SoundTimer = SoundCooldown;
            Audio.Play("event:/char/badeline/disappear");
        }
        if (Visible)
        {
            var particlePos = Y + Calc.Random.NextFloat() * PortalHeight;
            SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(StartX + 1, particlePos), Vector2.Zero);
            var particlePos2 = Y + Calc.Random.NextFloat() * PortalHeight;
            SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(EndX + 1, particlePos2), Vector2.Zero);
        }
    }

    [OnLoad]
    internal static void LoadHooks()
    {
        On.Celeste.Actor.MoveHExact += OnMoveHExact;
    }
    [OnUnload]
    internal static void UnloadHooks()
    {
        On.Celeste.Actor.MoveHExact -= OnMoveHExact;
    }

    private static bool OnMoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
    {
        var anyPortal = self.Scene.Tracker.GetEntity<MidStepPortals>();
        if (anyPortal is null) return orig(self, moveH, onCollide, pusher);
        return ClobberedMoveHExact(self, moveH, onCollide, pusher);
    }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);
        Draw.Line(Position, Position + Vector2.UnitY * Height, Color.Red);
        Draw.Line(new(EndX, Position.Y), new Vector2(EndX, Position.Y) + Vector2.UnitY * Height, Color.Yellow);
    }

    public static bool ClobberedMoveHExact(Actor self, int moveH, Collision? onCollide = null, Solid? pusher = null)
    {

        Vector2 targetPosition = self.Position + Vector2.UnitX * moveH;
        int moveDir = Math.Sign(moveH);
        int moveAmount = 0;
        var allPortals = self.Scene.Tracker.GetEntities<MidStepPortals>();
        while (moveH != 0)
        {
            for (int i = 0; i < allPortals.Count; i++)
            {
                MidStepPortals portals = (MidStepPortals)allPortals[i];
                if (self.Bottom > portals.Position.Y + portals.PortalHeight || self.Top < portals.Position.Y) continue;
                if (moveH < 0 && self.Left > portals.StartX && self.Left + moveDir <= portals.StartX)
                {
                    self.Position.X = portals.EndX - self.Width / 2;
                    portals.OnTeleport();
                }
                else if (moveH > 0 && self.Right < portals.EndX && self.Right + moveDir >= portals.EndX)
                {
                    self.Position.X = portals.StartX + self.Width / 2;
                    portals.OnTeleport();
                }
            }

            Solid solid = self.CollideFirst<Solid>(self.Position + Vector2.UnitX * moveDir);
            if (solid != null)
            {
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

    private void OnTeleport()
    {
        if (!Silent) ShouldPlaySound = true;
    }
}

#nullable restore
