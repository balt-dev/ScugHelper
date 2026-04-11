

using Celeste;

internal interface ICustomRefill
{
    public abstract void CustomOnPlayer(Player player);

    public static void OnPlayerHook(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player)
    {
        if (self is ICustomRefill refill)
            refill.CustomOnPlayer(player);
        else
            orig(self, player);
    }
    
    public static void LoadHooks() {
        On.Celeste.Refill.OnPlayer += OnPlayerHook;
    }

    public static void UnloadHooks() {
        On.Celeste.Refill.OnPlayer -= OnPlayerHook;
    }
}