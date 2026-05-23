

using Celeste;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

internal interface ICustomRefill
{
    public abstract void CustomOnPlayer(Player player);

    public static void OnPlayerHook(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player) {
        if (self is ICustomRefill refill)
            refill.CustomOnPlayer(player);
        else
            orig(self, player);
    }
    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Refill.OnPlayer += OnPlayerHook;
    }
    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Refill.OnPlayer -= OnPlayerHook;
    }
}
