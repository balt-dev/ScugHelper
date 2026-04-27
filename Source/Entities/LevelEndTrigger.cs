using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/LevelEndTrigger")]
public class LevelEndTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    public bool showCompleteScreen = data.Bool("ShowCompleteScreen", true);
    public bool spotlightWipe = data.Bool("ShowSpotlight", true);
    public bool screenWipe = data.Bool("ScreenWipe", true);

    private bool Lock = false;

    public override void OnEnter(Player player) {
        if (Lock) return;
        EndLevel();
        Lock = true;
    }

    private void EndLevel() {
        Level level = SceneAs<Level>();
        level.CompleteArea(spotlightWipe: spotlightWipe, skipScreenWipe: !screenWipe, skipCompleteScreen: !showCompleteScreen);
    }
}
