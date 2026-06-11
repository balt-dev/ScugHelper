using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.ScugHelper;
using MonoMod.Cil;
using System;
using Celeste.Mod;
using System.Linq;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/RefillGate")]
public class RefillGate(EntityData data, Vector2 offset) : AbstractGate(data, offset)
{
    private Refill? refill;
    private ParticleType pType = new(Player.P_DashA) {
        Color = data.HexColor("lineColor", Color.White) * 0.6f,
        Color2 = Color.Transparent,
        FadeMode = ParticleType.FadeModes.Linear,
        SpeedMin = 0f,
        SpeedMax = 0f,
        Acceleration = Vector2.Zero,
        LifeMin = 0.7f,
        LifeMax = 2f,
    };

    public override void Awake(Scene scene) {
        Collider = new Hitbox(32, 32, -16, -16);
        // Refills aren't Tracked.
        Refill? closestRefill = null;
        foreach (Entity entity in scene.Entities)
            if (entity is Refill refill && CollideCheck(refill) && (closestRefill is null || (closestRefill.Center - Center).LengthSquared() < (refill.Center - Center).LengthSquared()))
                closestRefill = refill;
        Collider = null;
        if (closestRefill == null) {
            Logger.Warn(nameof(ScugHelper), "No refill found! Deleting refill gate...");
            RemoveSelf();
            return;
        }
        closestRefill.Position = Position + closestRefill.Center - closestRefill.Position;
        closestRefill.Collider = new Hitbox(0, 0);
        refill = closestRefill;
    }

    public override void OnTrigger(Player player) {
        if (refill == null) return;
        if (refill.respawnTimer > 0f) return;
        foreach (PlayerCollider collider in refill.Components.GetAll<PlayerCollider>().ToArray())
            collider.OnCollide(player);
        if (refill.Scene == null) RemoveSelf();
    }

    public override void Update() {
        base.Update();
        if (refill == null) return;
        if (refill.respawnTimer > 0f) return;
        if (!Scene.OnInterval(0.1f)) return;
        var startPos = Position - lineDir * Size / 2;
        var endPos = Position + lineDir * Size / 2;
        var particlePos = startPos + Calc.Random.NextFloat() * (endPos - startPos);
        SceneAs<Level>().ParticlesFG.Emit(pType, 1, particlePos, Vector2.Zero);
    }

    public override void Render() {
        base.Render();
        if (refill == null) return;
        if (refill.respawnTimer > 0f) return;
        var startPos = Position - lineDir * Size / 2;
        var endPos = Position + lineDir * Size / 2;
        Draw.Line(startPos, endPos, pType.Color * 0.1f);
    }
}
