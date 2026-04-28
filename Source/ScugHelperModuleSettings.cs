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
}
