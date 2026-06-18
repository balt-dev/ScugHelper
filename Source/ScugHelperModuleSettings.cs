using Celeste.Mod.ScugHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using YamlDotNet.Serialization;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Celeste.Mod.ScugHelper;

public class ScugHelperModuleSettings : EverestModuleSettings
{
    [SettingInGame(false)]
    [YamlIgnore]
    public TextMenuExt.SubMenu ShowcaseMaps { get; set; } = null!;

    public MinimapMenu Minimap { get; set; } = new();

    public void CreateShowcaseMapsEntry(TextMenu menu, bool inGame) {
        ShowcaseMaps = new(Dialog.Clean("ScugHelper_ShowcaseMaps"), false);
        foreach (string mapSID in (string[]) ["1-ScugHelperTest", "2-Showcase2", "100-Actions", "101-BrassBerryTest"]) {
            string fullSID = $"ScugHelper/ScugHelperTest/{mapSID}";
            AreaData data = AreaData.Get(fullSID);
            AreaKey area = data.ToKey(AreaMode.Normal);
            var button = new TextMenu.Button(Dialog.Clean(fullSID)) {
                OnPressed = () => {
                    SaveData.InitializeDebugMode();
                    SaveData.Instance.LastArea_Safe = area;
                    Audio.SetMusic(null);
                    Audio.SetAmbience(null);
                    var session = new Session(area);
                    LevelEnter.Go(session, false);
                },
                Disabled = inGame
            };
            ShowcaseMaps.Add(button);
        }
        ShowcaseMaps.Disabled = inGame;
        menu.Add(ShowcaseMaps);
    }

    [SettingSubText("The maximum amount of time any given Lua script execution can take, in seconds.")]
    [SettingRange(1, 20)]
    public int LuaTimeLimit { get; set; } = 3;

    [SettingSubText("The maximum amount of memory any given Lua script execution can use, in megabytes.")]
    [SettingRange(1, 256, largeRange: true)]
    public int LuaMemoryLimit { get; set; } = 16;

    [SettingSubText("Makes all tiling fully deterministic.\nThis might get you rejected from certain lists due to creating pixel lineups that usually don't exist.")]
    public bool DeterministicAutotiling { get; set; } = false;

    [SettingSubText("Allows player seekers to hit dash switches.\nThis is not vanilla behavior, but is enabled by default,\nas it is likely an oversight in vanilla Celeste.\nTurn this off if need be.\nThis will be turned into a Controller in a later update.")]
    public bool PlayerSeekerDashSwitchFix { get; set; } = true;

    [SettingSubText("Replaces all vanilla spinners with ScugHelper GPU spinners.\nGood for performance, but might break some maps.\nTurn this off if making / playing back a TAS.")]
    public bool ReplaceVanillaSpinners { get; set; } = false;

    [SettingSubText("Enables sideflipping everywhere. Can also be enabled with the ScugHelper-AllowSideflipping flag.")]
    public bool SideflippingEverywhere { get; set; } = false;

    [SettingSubText("Enables booster bouncing for every booster.")]
    public bool AllBoostersBounce { get; set; } = false;

    [SettingSubText("Forces the Overcharge Refill's effect permanently.")]
    public bool AlwaysOvercharges { get; set; } = false;

    [SettingNumberInput(allowNegatives: false, maxLength: 5)]
    [SettingSubText("Adjusts how much squash and stretch pinball boosters have.")]
    public float PinballBoosterSquash { get; set; } = 1.45f;

    [SettingSubText("Replaces all spinners with seekers. Good luck :)")]
    public SpinnerSeekerState SpinnersAreSeekers { get; set; } = SpinnerSeekerState.Off;

    [SettingName("Focus Minimap")]
    [DefaultButtonBinding([], [Keys.M])]
    public ButtonBinding MinimapBind { get; set; } = new();
    [SettingName("Hide Minimap")]
    [DefaultButtonBinding([], [Keys.N])]
    public ButtonBinding MinimapVisibleBind { get; set; } = new();
    [SettingName("Zoom Minimap In")]
    [DefaultButtonBinding([], [Keys.OemMinus])]
    public ButtonBinding MinimapZoomIn { get; set; } = new();
    [SettingName("Zoom Minimap Out")]
    [DefaultButtonBinding([], [Keys.OemPlus])]
    public ButtonBinding MinimapZoomOut { get; set; } = new();

    public bool AlwaysCenterCameraX { get; set; } = false;
    public bool AlwaysCenterCameraY { get; set; } = false;

    [SettingSubText("Makes all room transitions instant. Increases IGT by 0.68 seconds on transition to compensate.")]
    public bool InstantRoomTransitions { get; set; } = false;

    [YamlIgnore]
    [SettingSubText("Please don't. Requires a restart to fix.")]
    public bool GladelineApocalypse { get; set; } = false;
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
    [SettingRange(0, 1920, largeRange: true)]
    public int MinimapWidth { get; set; } = 640;
    [SettingRange(0, 1080, largeRange: true)]
    public int MinimapHeight { get; set; } = 360;
    [SettingRange(0, 1920, largeRange: true)]
    public int MinimapX { get; set; } = 1920 - 640 - 10;
    [SettingRange(0, 1080, largeRange: true)]
    public int MinimapY { get; set; } = 10;
}
