using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/LevelEndAction")]
public class LevelEndAction(EntityData data, Vector2 _) : Entity(), IAction
{
    public bool showCompleteScreen = data.Bool("ShowCompleteScreen", true);
    public bool spotlightWipe = data.Bool("ShowSpotlight", true);
    public bool screenWipe = data.Bool("ScreenWipe", true);
    
    public void Alert(Level level) {
        level.CompleteArea(spotlightWipe: spotlightWipe, skipScreenWipe: !screenWipe, skipCompleteScreen: !showCompleteScreen);
    }
}