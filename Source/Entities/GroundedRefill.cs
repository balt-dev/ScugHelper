using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using Celeste.Mod.ScugHelper;
using Celeste.Mod;

#nullable enable
[TrackedAs(typeof(Refill))]
[CustomEntity("ScugHelper/GroundedRefill")]
public class GroundedRefill : Refill {
    private readonly Image underline;

    public GroundedRefill(Vector2 position, bool twoDashes) : base(position, twoDashes, false) {
        Add(underline = new Image(GFX.Game["objects/groundedRefill/underline"]));
        underline.CenterOrigin();
    }
    public GroundedRefill(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("twoDash")) { }

    public override void Update()
    {
        respawnTimer = 100f;
        Player? player = SceneAs<Level>().Tracker.GetEntity<Player>();
        if (player != null && player.OnSafeGround)
            Respawn();
        base.Update();
        underline.Y = Collidable ? flash.Y : outline.Position.Y;
    }
    public override void Render()
    {
        if (Collidable)
            underline.DrawOutline();
        base.Render();
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        LastRefillSound = 0f;
    }
    public static float LastRefillSound = 0f;
    private static readonly float RefillSoundCooldown = 0.02f;
    public static void LoadHooks() {
        IL.Celeste.Refill.Respawn += RespawnHook;
    }

    public static void UnloadHooks() {
        IL.Celeste.Refill.Respawn -= RespawnHook;
    }

    private static void RespawnHook(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Refill>("twoDashes")
        )) throw new InvalidOperationException("Grounded refills failed to match IL code for the Respawn hook.");
        var label = cur.DefineLabel();
        cur.EmitLdarg0();
        static bool Delegate(Refill refill) {
            var delta = refill.Scene.TimeActive - LastRefillSound;
            if (delta > RefillSoundCooldown) {
                LastRefillSound = refill.Scene.TimeActive;
                return true;
            }
            return false;
        }
        cur.EmitDelegate(Delegate);
        cur.EmitBrfalse(label);
        if (!cur.TryGotoNext(MoveType.After,
            instr => instr.MatchCall(typeof(Audio), nameof(Audio.Play)),
            instr => instr.MatchPop()
        )) throw new InvalidOperationException("Grounded refills failed to match IL code for the Respawn hook.");
        cur.MarkLabel(label);
    }
}
#nullable restore
