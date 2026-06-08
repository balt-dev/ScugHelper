using System;
using Celeste.Mod;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

using Celeste;
using MonoMod.Utils;
using MonoMod.ModInterop;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ScugHelper;

[ModExportName("ScugHelper")]
public static class ScugHelperModInterop {
    public static void AlertActions(string[] groups, Level? level)
        => Entities.Actions.ActionManager.AlertActions(groups, level);

    public static void RegisterActionGroupListener(string name, string[] groups, Action<Level> callback) {
        foreach (string group in groups) {
            var actionMap = Entities.Actions.ActionManager.actionMap;
            if (!actionMap.TryGetValue(group, out var actions))
                actionMap.Add(group, actions = []);
            actions.Add(new(callback, 0f, new EntityData {Name = $"{{Interop Listener: {name}}}"}, immediate: true));
        }
    }

    public static bool GetMidairRefillState() => Entities.MidairRefill.MidairDashCount > 0;
    public static void SetMidairRefillState(bool value) => Entities.MidairRefill.MidairDashCount = value ? 1 : 0;

    public static bool GetOverchargeRefillState() => Entities.OverchargeRefill.OverchargeDashCount > 0;
    public static void SetOverchargeRefillState(bool value) => Entities.OverchargeRefill.OverchargeDashCount = value ? 1 : 0;

    public static float GetLimboRefillTimer() => Entities.LimboRefill.LimboTimer;
    public static void SetLimboRefillTimer(float value) => Entities.LimboRefill.LimboTimer = value;
    
    public static bool IsCustomPlayerSeeker(Player player) => player.Get<PlayerSeekerComponent>() is not null;

    public static object? GetSpeedAccessor(Entity entity) => SpeedAccessor.For(entity);
    public static Vector2 GetSpeedFromAccessor(object accessor) => ((SpeedAccessor) accessor).Speed;
    public static void SetSpeedFromAccessor(object accessor, Vector2 value) => ((SpeedAccessor) accessor).Speed = value;

    /// <summary>
    ///     Callback to be ran when some modded holdable hits a Kevin.
    ///     This should be called on CollideH, for example with `if (data.Hit is CrushBlock block) HandleHoldableHittingKevin(block, data);`.
    ///     This enables compatibility with your holdable and the Custom Kevin Controller's HitByHoldables option.
    /// </summary>
    public static void HandleHoldableHittingKevin(CrushBlock kevin, CollisionData data)
        => Entities.CustomKevinController.HandleHoldableHit(kevin, data);
        
    /// <summary>
    ///     Callback to be ran when the player hits an arbitrary angled spring.
    ///     Will exit immediately before running regular logic if this returns true.
    /// </summary>
    public delegate bool AngledSpringCallback(Vector2 launchDirection, float launchSpeed, bool noRefillDash, bool noRefillStamina);
    public static void RegisterAngledSpringCallback(Player player, AngledSpringCallback callback)
        => player.Add(new Entities.ArbitraryAngleSpring.AngledSpringCallbackComponent(callback));
}
