namespace Celeste.Mod.ScugHelper;

public class ScugHelperModuleSettings : EverestModuleSettings {
    [SettingNumberInput(allowNegatives: false, maxLength: 5)]
    public float PinballBumperSquash { get; set; } = 1.45f;
    public bool AllBoostersBounce { get; set; } = false;
}