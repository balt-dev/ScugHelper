using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;

[Tracked]
[CustomEntity("ScugHelper/Cycler")]
public class Cycler(Vector2 position, float radius, float rpm, float phase, int entID) : Entity(position)
{
    public float Radius { get; protected set; } = radius;
    public float RPM { get; protected set; } = rpm;
    public float Phase { get; protected set; } = phase;
    protected int AttachedEntityID = entID;
    protected Entity AttachedEntity;

    public Cycler(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("Radius"), data.Float("RPM"), data.Float("Phase"), data.Int("AttachedEntityID"))
    {}

    public override void Update() {
        base.Update();
        var factor = 60.0f / RPM;
        Phase += Engine.DeltaTime / factor;
        Phase %= 1.0f;

        Vector2 offsetVec = new Vector2((float)Math.Cos(Math.Tau * Phase), (float)Math.Sin(Math.Tau * Phase)) * Radius;
        Vector2 targetPosition = Position + offsetVec;
        if (AttachedEntity is Bumper bumper) bumper.anchor = targetPosition;
        if (AttachedEntity is ReboundBlock reboundBlock) reboundBlock.Anchor = targetPosition;
        if (AttachedEntity is Booster booster)
        {
            booster.outline.Position = targetPosition;
            Player player = SceneAs<Level>()?.Tracker?.GetEntity<Player>();
            if (player != null && player.CurrentBooster != null && player.CurrentBooster == booster) {
                player.MoveH(targetPosition.X - player.X);
                player.MoveV(targetPosition.Y - player.Y);
            }
        }
        if (AttachedEntity is MoveBlock moveBlock)
            if (moveBlock.state != MoveBlock.MovementState.Idling) return;
        if (AttachedEntity is Platform AttachedPlatform) AttachedPlatform.MoveTo(targetPosition);
        else if (AttachedEntity is Actor AttachedActor) {
            AttachedActor.MoveToX(targetPosition.X);
            AttachedActor.MoveToY(targetPosition.Y);
        } else AttachedEntity.Position = targetPosition;
    }

    public override void Render()
    { base.Render(); }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);

        Vector2 offsetVec = new Vector2((float)Math.Cos(Math.Tau * Phase), (float)Math.Sin(Math.Tau * Phase)) * Radius;
        Vector2 targetPosition = Position + offsetVec;

        Draw.Circle(Position, Radius, Color.Purple, 32);
        Draw.Line(Position, targetPosition, Color.Lime);
        Draw.Circle(Position, 3.0f, Color.Red, 8);
        Draw.Circle(targetPosition, 3.0f, Color.Yellow, 8);
    }

    // go play awake by butcherberries it is a masterpiece
    public override void Awake(Scene scene) {
        base.Awake(scene);
        foreach (Entity entity in scene.Entities)
        {
            if (entity.SourceId.ID == AttachedEntityID)
            {
                AttachedEntity = entity;
                return;
            }
        }
        Logger.Error(nameof(ScugHelperModule), $"Cycler entity with id {SourceId} failed to find an entity with ID {AttachedEntityID} to attach.");
        throw new InvalidOperationException($"Cycler entity with id {SourceId} failed to find an entity with ID {AttachedEntityID} to attach.");
    }
}
