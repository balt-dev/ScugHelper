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
    public readonly float YOffset;
    public readonly float PortalHeight;
    public readonly bool Silent;
    bool ShouldPlaySound;
    private float SoundTimer;
    private const float SoundCooldown = 0.1f;

    public MidStepPortals(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = -150000;
        var endPos = data.FirstNodeNullable(offset) ?? Position;
        StartX = Position.X;
        EndX = endPos.X;
        YOffset = endPos.Y - Y;
        PortalHeight = data.Height;
        Silent = data.Bool("Silent", false);
        Visible = !data.Bool("Invisible", false);
    }

    public override void Update()
    {
        base.Update();
        SoundTimer -= Engine.RawDeltaTime;
        if (ShouldPlaySound && SoundTimer <= 0f)
        {
            ShouldPlaySound = false;
            SoundTimer = SoundCooldown;
            Audio.Play("event:/char/badeline/disappear");
        }
        if (Visible)
        {
            var particlePos = Y + Calc.Random.NextFloat() * PortalHeight + 1;
            SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(StartX + 1, particlePos), Vector2.Zero);
            var particlePos2 = Y + YOffset + Calc.Random.NextFloat() * PortalHeight + 1;
            SceneAs<Level>().ParticlesFG.Emit(TeleportGate.ParticleType, 1, new(EndX, particlePos2), Vector2.Zero);
        }
    }
    
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(new(StartX + 1, Y), new(StartX + 1, Y + PortalHeight), Color.Green);
            Draw.Line(new(EndX, Y + YOffset), new(EndX, Y + PortalHeight + YOffset), Color.Yellow);
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
        => self.Scene.Tracker.GetEntity<MidStepPortals>() is null
            ? orig(self, moveH, onCollide, pusher)
            : ClobberedMoveHExact(self, moveH, onCollide, pusher);

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
                if (
                    moveH < 0
                    && self.Left > portals.StartX && self.Left + moveDir <= portals.StartX
                    && !(self.Bottom > portals.Position.Y + portals.PortalHeight || self.Top < portals.Position.Y)
                )
                {
                    self.Position.X = portals.EndX - self.Width / 2;
                    self.MoveV(portals.YOffset);
                    portals.OnTeleport();
                }
                else if (
                    moveH > 0
                    && self.Right < portals.EndX && self.Right + moveDir >= portals.EndX
                    && !(self.Bottom > portals.Position.Y + portals.PortalHeight + portals.YOffset || self.Top < portals.Position.Y + portals.YOffset)
                )
                {
                    self.Position.X = portals.StartX + self.Width / 2;
                    self.MoveV(-portals.YOffset);
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
