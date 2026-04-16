using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

#nullable enable

[CustomEntity("ScugHelper/LevelEndController")]
public class LevelEndController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    public string flagToCheck = data.String("FlagToCheck", "");
    public bool showCompleteScreen = data.Bool("ShowCompleteScreen", true);
    public bool spotlightWipe = data.Bool("ShowSpotlight", true);
    public bool screenWipe = data.Bool("ScreenWipe", true);

    public override void Update()
    {
        base.Update();
        if (!SceneAs<Level>().Session.GetFlag(flagToCheck)) return;
        EndLevel();
        RemoveSelf();
    }

    private void EndLevel() {
        Level level = SceneAs<Level>();
        level.CompleteArea(spotlightWipe: spotlightWipe, skipScreenWipe: !screenWipe, skipCompleteScreen: !showCompleteScreen);
    }
}

#nullable restore
