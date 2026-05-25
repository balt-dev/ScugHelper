using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/RegenerateProceduralTilemapsTrigger")]
public class RegenerateProceduralTilemapsTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {
    public readonly bool OnlyOnce = data.Bool("OnlyOnce");
    public readonly bool GlitchEffect = data.Bool("GlitchEffect", true);
    public override void OnEnter(Player player) {
        var arrayCopy = player.Scene.Tracker.GetEntities<ProceduralTilemap>().ToArray(); // so we don't get bonked with collection modified
        foreach (ProceduralTilemap tilemap in arrayCopy) tilemap.RegenerateTiles();
        if (GlitchEffect) {
            IEnumerator DoGlitchEffect() {
                Audio.Play("event:/new_content/game/10_farewell/glitch_short");
                Glitch.Value = 0.35f;
                while (Glitch.Value > 0f) {
                    Glitch.Value = Calc.Approach(Glitch.Value, 0f, Engine.RawDeltaTime * 2f);
                    player.level.Shake();
                    yield return null;
                }
            }
            player.Add(new Coroutine(DoGlitchEffect()));
        }
        if (OnlyOnce) RemoveSelf();
    }
}
