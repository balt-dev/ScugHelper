using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using Celeste.Mod.ScugHelper;
using Celeste.Mod;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
namespace Celeste.Mod.ScugHelper.Entities;

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
    }
    public static long TimeSince = 0;
    private static readonly long RefillSoundCooldown = 2;
    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Level.Update += LevelUpdateHook;
        IL.Celeste.Refill.Respawn += RespawnHook;
    }

    private static void LevelUpdateHook(On.Celeste.Level.orig_Update orig, Level self)
    {
        TimeSince += 1;
        orig(self);
    }
    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Level.Update -= LevelUpdateHook;
        IL.Celeste.Refill.Respawn -= RespawnHook;
    }

    private static void RespawnHook(ILContext il) {
        ILCursor cur = new(il);
        if (!cur.TryGotoNextBestFit(MoveType.Before, 16,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Refill>("twoDashes")
        )) throw new InvalidOperationException("Grounded refills failed to match IL code for the Respawn hook.");
        var label = cur.DefineLabel();
        cur.EmitLdarg0();
        static bool Delegate(Refill refill) {
            if (TimeSince > RefillSoundCooldown) {
                TimeSince = 0;
                return true;
            }
            return false;
        }
        cur.EmitDelegate(Delegate);
        cur.EmitBrfalse(label);
        if (!cur.TryGotoNextBestFit(MoveType.After, 16,
            instr => instr.MatchCall(typeof(Audio), nameof(Audio.Play)),
            instr => instr.MatchPop()
        )) throw new InvalidOperationException("Grounded refills failed to match IL code for the Respawn hook.");
        cur.MarkLabel(label);
    }
}
#nullable restore
