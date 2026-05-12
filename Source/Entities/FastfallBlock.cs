using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/FastfallBlock")]
public class FastfallBlock(EntityData data, Vector2 offset, EntityID id) : AbstractBreakableBlock(data, offset, id)
{
    private readonly int SpeedMinimum = data.Int("SpeedMinimum", 0);
    private static ILHook? hook_Player_orig_Update;
    [OnLoad]
    internal static void LoadHooks()
    {
        hook_Player_orig_Update = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance)!, PlayerUpdateHook);
    }
    [OnUnload]
    internal static void UnloadHooks() {
        hook_Player_orig_Update?.Dispose();
    }

    private static void PlayerUpdateHook(ILContext il)
    {
        ILCursor cur = new(il);
        if (!cur.TryGotoNextBestFit(MoveType.After, 16,
            static instr => instr.MatchLdcI4(22),
            static instr => instr.MatchBeq(out _)
        )) throw new Exception("Fastfall blocks failed to match IL for Player orig_Update hook.");
        static void PlayerCheck(Player player) {
            if (
                player.CollideFirst<FastfallBlock>(player.Position + player.AdjustedSpeed() * Engine.DeltaTime) is FastfallBlock block
                && (
                    (player.StateMachine.State == Player.StNormal && Input.MoveY.Value == 1 && player.Speed.Y >= block.SpeedMinimum) ||
                    (player.StateMachine.State == Player.StTempleFall) ||
                    (player.StateMachine.State == Player.StReflectionFall)
                )
            )
                block.Break(player.Position);
        }
        cur.EmitLdarg0();
        cur.EmitDelegate(PlayerCheck);
    }
}
#nullable restore
