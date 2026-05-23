using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/RegenerateProceduralTilemapsTrigger")]
public class RegenerateProceduralTilemapsTrigger(EntityData data, Vector2 offset) : Trigger(data, offset) {

    public override void OnEnter(Player player)
    {
        var arrayCopy = player.Scene.Tracker.GetEntities<ProceduralTilemap>().ToArray(); // so we don't get bonked with collection modified
        foreach (ProceduralTilemap tilemap in arrayCopy) tilemap.RegenerateTiles();
    }
}
