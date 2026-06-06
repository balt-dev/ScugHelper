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

    static Hook? HookOnVirtualButtonGetPressed;

    private static bool OnVirtualButtonGetPressed(Func<VirtualButton, bool> orig, VirtualButton self)
    {
        if (DynamicData.For(self).Get("ScugHelper_ForceSpam") is true) {
            self.consumed = false;
            var res = self.Check;
            return res;
        }
        return orig(self);
    }

    [OnLoad]
    internal static void LoadHooks() {
        HookOnVirtualButtonGetPressed = new(typeof(VirtualButton).GetProperty(nameof(VirtualButton.Pressed))!.GetGetMethod()!, OnVirtualButtonGetPressed);
        On.Celeste.LevelLoader.StartLevel += OnLevelLoaderStartLevel;
        On.Celeste.Level.LoadLevel += OnLevelLoadLevel;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        HookOnVirtualButtonGetPressed?.Dispose();
        On.Celeste.LevelLoader.StartLevel -= OnLevelLoaderStartLevel;
        On.Celeste.Level.LoadLevel -= OnLevelLoadLevel;
    }

    private static void OnLevelLoaderStartLevel(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self)
    {
        DynamicData.For(Input.Dash).Set("ScugHelper_ForceSpam", false);
        DynamicData.For(Input.Jump).Set("ScugHelper_ForceSpam", false);
        DynamicData.For(Input.Grab).Set("ScugHelper_ForceSpam", false);
        orig(self);
    }

    private static void OnLevelLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        DynamicData.For(Input.Dash).Set("ScugHelper_ForceSpam", false);
        DynamicData.For(Input.Jump).Set("ScugHelper_ForceSpam", false);
        DynamicData.For(Input.Grab).Set("ScugHelper_ForceSpam", false);
        orig(self, playerIntro, isFromLoader);
    }
}
