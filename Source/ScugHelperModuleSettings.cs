using Celeste.Mod.ScugHelper.Entities;
using Microsoft.Xna.Framework.Input;
using Monocle;

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


    [SettingName("Focus Minimap")]
    [DefaultButtonBinding([], [Keys.M])]
    public ButtonBinding MinimapBind { get; set; } = new();
    [SettingName("Zoom Minimap In")]
    [DefaultButtonBinding([], [Keys.OemMinus])]
    public ButtonBinding MinimapZoomIn { get; set; } = new();
    [SettingName("Zoom Minimap Out")]
    [DefaultButtonBinding([], [Keys.OemPlus])]
    public ButtonBinding MinimapZoomOut { get; set; } = new();

    public MinimapMenu Minimap { get; set; } = new();
    public bool AlwaysCenterCameraX { get; set; } = false;
    public bool AlwaysCenterCameraY { get; set; } = false;
}

[SettingSubMenu]
public class MinimapMenu {
    [SettingSubHeader("Minimap")]
    [SettingSubText("Whether to show the minimap.")]
    public bool Minimap { get; set; } = false;

    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public float UnfocusedOpacity { get; set; } = 0.3f;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public float FocusedOpacity { get; set; } = 1f;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapWidth { get; set; } = 640;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapHeight { get; set; } = 360;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapX { get; set; } = 1920 - 640 - 10;
    [SettingNumberInput(allowNegatives: false, maxLength: 4)]
    public int MinimapY { get; set; } = 10;
}
