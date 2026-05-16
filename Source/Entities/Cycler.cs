using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Utils;
using MonoMod.Cil;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/Cycler")]
public class Cycler(Vector2 position, float radius, float rpm, float phase, int entID, bool keepX, bool keepY) : Entity(position)
{
    public float Radius { get; protected set; } = radius;
    public float RPM { get; protected set; } = rpm;
    public float Phase { get; protected set; } = phase;
    protected int AttachedEntityID = entID;
    protected Entity AttachedEntity;
    protected bool KeepX = keepX;
    protected bool KeepY = keepY;

    public Cycler(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Float("Radius"), data.Float("RPM"), data.Float("Phase"), data.Int("AttachedEntityID"), data.Bool("KeepX"), data.Bool("KeepY"))
    {
        Depth = 1000000;
    }

    public override void Update()
    {
        base.Update();
        if (AttachedEntity is Cycler child)
            frozen = child.frozen;
        var factor = 60.0f / RPM;
        if (!frozen) Phase += Engine.DeltaTime / factor;
        Phase %= 1.0f;

        Vector2 offsetVec = new Vector2((float)Math.Cos(Math.Tau * Phase), (float)Math.Sin(Math.Tau * Phase)) * Radius;
        Vector2 targetPosition = Position + offsetVec;
        if (AttachedEntity is Bumper bumper) SetPosition(ref bumper.anchor, targetPosition);
        if (AttachedEntity is Booster booster)
        {
            SetPosition(ref booster.outline.Position, targetPosition);
            Player player = SceneAs<Level>()?.Tracker?.GetEntity<Player>();
            frozen = player != null && player.CurrentBooster != null && player.CurrentBooster == booster;
        }
        if (AttachedEntity is ZipMover mover)
        {
            SetPosition(ref mover.target, targetPosition + mover.target - mover.start);
            SetPosition(ref mover.start, targetPosition);
            SetPosition(ref mover.pathRenderer.from, targetPosition + mover.Center - mover.Position);
            SetPosition(ref mover.pathRenderer.to, targetPosition + mover.Center - mover.Position + mover.target - mover.start);
            targetPosition = Vector2.Lerp(mover.start, mover.target, mover.percent);
        }
        if (AttachedEntity is SwapBlock block)
        {
            SetPosition(ref block.end, targetPosition + block.end - block.start);
            SetPosition(ref block.start, targetPosition);
            block.maxForwardSpeed = 360f / Vector2.Distance(block.start, block.end);
            block.maxBackwardSpeed = block.maxForwardSpeed * 0.4f;
            block.Direction.X = Math.Sign(block.end.X - block.start.X);
            block.Direction.Y = Math.Sign(block.end.Y - block.start.Y);
            int left = (int)MathHelper.Min(block.start.X, block.end.X);
            int top = (int)MathHelper.Min(block.start.Y, block.end.Y);
            int right = (int)MathHelper.Max(block.start.X + block.Width, block.end.X + block.Width);
            int bottom = (int)MathHelper.Max(block.start.Y + block.Height, block.end.Y + block.Height);
            block.moveRect = new Rectangle(left, top, (int)(MathF.Round((right - left) / 8f) * 8f), (int)(MathF.Round((bottom - top) / 8f) * 8f));
            if (block.lerp == 1) targetPosition = block.end;
            else if (block.lerp > 0) return;
        }
        if (AttachedEntity is Puffer puffer)
        {
            frozen = puffer.state == Puffer.States.Gone;
            if (frozen) return;
            SetPosition(ref puffer.startPosition, targetPosition);
        }
        if (AttachedEntity is MoveBlock moveBlock)
        {
            frozen = moveBlock.state != MoveBlock.MovementState.Idling;
            if (frozen) return;
        }
        if (AttachedEntity is CrushBlock kevin)
        {
            frozen = kevin.returnStack.Count > 0;
            if (frozen) return;
        }
        if (AttachedEntity is HangRail rail)
        {
            SetPosition(ref rail.Start, targetPosition + rail.Start - rail.Position);
            SetPosition(ref rail.End, targetPosition + rail.End - rail.Position);
            SetPosition(ref rail.Position, targetPosition);
        }
        else if (AttachedEntity is Platform AttachedPlatform)
        {
            if (!KeepX) AttachedPlatform.MoveToX(targetPosition.X);
            if (!KeepY) AttachedPlatform.MoveToY(targetPosition.Y);
        }
        else if (AttachedEntity is Actor AttachedActor)
        {
            if (!KeepX) AttachedActor.MoveToX(targetPosition.X);
            if (!KeepY) AttachedActor.MoveToY(targetPosition.Y);
        }
        else SetPosition(ref AttachedEntity.Position, targetPosition);
    }

    private void SetPosition(ref Vector2 anchor, Vector2 targetPosition)
    {
        if (!KeepX)
            anchor.X = targetPosition.X;
        if (!KeepY)
            anchor.Y = targetPosition.Y;
    }

    public override void Render()
    { base.Render(); }

    public override void DebugRender(Camera camera)
    {
        base.DebugRender(camera);

        Vector2 offsetVec = new Vector2((float)Math.Cos(Math.Tau * Phase), (float)Math.Sin(Math.Tau * Phase)) * Radius;
        Vector2 targetPosition = Position + offsetVec;
        Vector2 realPosition = AttachedEntity.Position;

        Draw.Circle(Position, Radius, Color.Purple, 32);
        Draw.Line(Position, targetPosition, Color.Lime);
        Draw.Circle(Position, 3.0f, Color.Red, 8);
        Draw.Circle(targetPosition, 3.0f, Color.Yellow, 8);
        Draw.Line(realPosition, targetPosition, Color.Cyan);
    }

    // go play awake by butcherberries it is a masterpiece
    public override void Awake(Scene scene)
    {
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




    private static readonly MethodInfo ZipMoverSequence = typeof(ZipMover).GetMethod("Sequence", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly MethodInfo ZipMoverSequenceTarget = ZipMoverSequence.GetStateMachineTarget();
    private static readonly Type ZipMoverSequenceType = ZipMoverSequenceTarget.DeclaringType;
    private static readonly FieldInfo startField = ZipMoverSequenceType
        .GetField("<start>5__2", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    private static ILHook ZipMoverSequenceHook = null;
    private bool frozen;

    [OnLoad]
    internal static void LoadHooks()
    {
        ZipMoverSequenceHook = new(ZipMoverSequenceTarget, ZipMoverFix);
        On.Celeste.SwapBlock.Update += OnSwapBlockUpdate;
    }
    [OnUnload]
    internal static void UnloadHooks()
    {
        ZipMoverSequenceHook?.Dispose();
        On.Celeste.SwapBlock.Update -= OnSwapBlockUpdate;
    }

    private static void OnSwapBlockUpdate(On.Celeste.SwapBlock.orig_Update orig, SwapBlock self)
    {
        float oldLerp = self.lerp;
        orig(self);
        if (self.lerp != oldLerp && self.lerp <= 0f)
        {
            Audio.SetParameter(self.returnSfx, "end", 1f);
            Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", self.Center);
        }
    }

    private static void ZipMoverFix(ILContext il)
    {
        ILCursor cur = new(il);
        ILLabel[] labels = [];
        if (!cur.TryGotoNext(MoveType.Before,
            instr => instr.MatchSwitch(out labels)
        )) throw new Exception("Cycler failed to match IL for fixing Zip Movers.");
        for (int i = 1; i < labels.Length; i++)
        {
            cur.GotoLabel(labels[i]);
            cur.EmitLdarg0();
            cur.EmitLdloc1();
            cur.EmitDelegate(static (ZipMover mover) => mover.pathRenderer.from - mover.Center + mover.Position);
            cur.EmitStfld(startField);
        }
    }
}
