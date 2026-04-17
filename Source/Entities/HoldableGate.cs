using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.ScugHelper;
using MonoMod.Cil;
using System;
using Celeste.Mod;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/HoldableGate")]
public class HoldableGate(EntityData data, Vector2 offset) : AbstractGate(data, offset) {
    private Holdable holdable;
    private Collider actualCollider;

    public override void Awake(Scene scene) {
        Collider = new Hitbox(32, 32, -16, -16);
        holdable = CollideFirstByComponent<Holdable>();
        Collider = null;
        if (holdable == null) {
            Logger.Warn(nameof(ScugHelperModule), "No holdable found! Deleting holdable gate...");
            RemoveSelf();
            return;
        }
        Hitbox hitbox = holdable.PickupCollider.Clone() as Hitbox;
        hitbox.width = 0;
        hitbox.height = 0;
        actualCollider = holdable.PickupCollider;
        holdable.PickupCollider = hitbox;
    }

    private bool Triggered = false;
    private int updatesSinceTrigger = 999;
    private static readonly int GrabFrameLeniency = 4;

    public override void OnTrigger(Player player)
    {
        updatesSinceTrigger = 0;
    }

    public override void Update()
    {
        updatesSinceTrigger += 1;
        base.Update();
        if (updatesSinceTrigger < GrabFrameLeniency) {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && Input.GrabCheck && player.Holding == null && player.Pickup(holdable)) {
                holdable.Entity.Active = true;
                player.StateMachine.State = Player.StPickup;
                holdable.PickupCollider = actualCollider;
                Triggered = true;
                var startPos = Position - lineDir * Size / 2;
                var endPos = Position + lineDir * Size / 2;
                for (int i = 0; i < Size; i++) {
                    var pos = Vector2.Lerp(startPos, endPos, i / Size);
                    SceneAs<Level>().Particles.Emit(new(Glider.P_Platform) { SpeedMin = 0, SpeedMax = player.Speed.Length() * 1.45f }, pos + PlatformAdd(i), Color.White * (Math.Abs(i - Size / 2) < Math.Max(holdable.Entity.Width, holdable.Entity.Height) ? 0.8f : 0.4f), player.Speed.Angle() + Calc.Random.Range(-0.1f, 0.1f));
                }
                RemoveSelf();
                return;
            }
        }
        if (!Triggered) {
            holdable.Entity.Position = Position;
            holdable.SpeedSetter?.Invoke(Vector2.Zero);
        }
    }

    private Vector2 PlatformAdd(int num)
    {
        return lineNorm * (int)Math.Round(Math.Sin(Scene.TimeActive + num * 0.2) * 1.8f);
    }

    public override void Render()
    {
        if (!Triggered) {
            holdable.Entity.Position = Position + (holdable.Entity.Position - holdable.Entity.Center);
        }
        base.Render();
        var startPos = Position - lineDir * Size / 2;
        var endPos = Position + lineDir * Size / 2;
        for (int i = 0; i < Size; i++)
        {
            var pos = Vector2.Lerp(startPos, endPos, i / Size);
            Draw.Point(pos + PlatformAdd(i), Color.White * (Math.Abs(i - Size / 2) < Math.Max(holdable.Entity.Width, holdable.Entity.Height) ? 0.8f : 0.4f));
        }
    }
}