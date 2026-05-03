using Celeste.Mod.ScugHelper.Entities;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.ScugHelper;

public class ScugHelperModuleSettings : EverestModuleSettings
{

    [SettingNumberInput(allowNegatives: false, maxLength: 5)]
    [SettingSubText("Adjusts how much squash and stretch pinball boosters have.")]
    public float PinballBoosterSquash { get; set; } = 1.45f;

    [SettingSubText("Uses an alternative font for text entities.")]
    public bool AlternativeFont { get; set; } = false;

    [SettingSubText("Allows player seekers to hit dash switches.\nThis is not vanilla behavior, but is enabled by default,\nas it is likely an oversight in vanilla Celeste.\nTurn this off if need be.")]
    public bool PlayerSeekerDashSwitchFix { get; set; } = true;

    [SettingSubText("Enables sideflipping everywhere. Can also be enabled with the ScugHelper-AllowSideflipping flag.")]
    public bool SideflippingEverywhere { get; set; } = false;

    [SettingSubText("Enables booster bouncing for every booster.")]
    public bool AllBoostersBounce { get; set; } = false;

    [SettingSubText("Replaces all spinners with seekers. Good luck :)")]
    public SpinnerSeekerState SpinnersAreSeekers { get; set; } = SpinnerSeekerState.Off;

    public MinimapMenu Minimap { get; set; } = new();
}

[SettingSubMenu]
public class MinimapMenu {
    [SettingSubHeader("Minimap")]
    [SettingSubText("Whether to show the minimap.")]
    public bool Minimap { get; set; } = false;

    [DefaultButtonBinding(button: Buttons.TouchPadEXT, key: Keys.M)]
    [SettingName("Focus Minimap")]
    public ButtonBinding MinimapBind { get; set; }

    public MinimapBindBehavior ButtonBehavior { get; set; } = MinimapBindBehavior.Toggle;

    public float UnfocusedOpacity { get; set; } = 0.3f;
    public float FocusedOpacity { get; set; } = 1f;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapWidth { get; set; } = 180;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapHeight { get; set; } = 120;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapX { get; set; } = 180;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapY { get; set; } = 120;
}
