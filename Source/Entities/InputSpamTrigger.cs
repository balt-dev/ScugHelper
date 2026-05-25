using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using System;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/InputSpamTrigger")]
public class InputSpamTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    enum SpamInput { Dash, Jump, Grab }

    public VirtualButton Button = data.Enum("Input", SpamInput.Dash) switch {
        SpamInput.Dash => Input.Dash,
        SpamInput.Jump => Input.Jump,
        SpamInput.Grab => Input.Grab,
        _ => throw new NotImplementedException()
    };

    public override void OnEnter(Player player) => DynamicData.For(Button).Set("ScugHelper_ForceSpam", true);
    public override void OnLeave(Player player) => DynamicData.For(Button).Set("ScugHelper_ForceSpam", false);

    static Hook HookOnVirtualButtonGetCheck;
    static Hook HookOnVirtualButtonGetPressed;

    private static bool OnVirtualButtonGetCheckPressed(Func<VirtualButton, bool> orig, VirtualButton self)
        => DynamicData.For(self).Get("ScugHelper_ForceSpam") is true || orig(self);

    [OnLoad]
    internal static void LoadHooks() {
        HookOnVirtualButtonGetCheck = new(typeof(VirtualButton).GetProperty(nameof(VirtualButton.Check)).GetGetMethod(), OnVirtualButtonGetCheckPressed);
        HookOnVirtualButtonGetPressed = new(typeof(VirtualButton).GetProperty(nameof(VirtualButton.Pressed)).GetGetMethod(), OnVirtualButtonGetCheckPressed);
    }

    [OnUnload]
    internal static void UnloadHooks() {
        HookOnVirtualButtonGetCheck?.Dispose();
        HookOnVirtualButtonGetPressed?.Dispose();
    }

}
