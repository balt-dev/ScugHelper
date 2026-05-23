using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.ScugHelper.Entities;

internal class NameTracker() : Component(true, true)
{
    public override void Render() {
        base.Render();
        if (Entity is null) return;
        string key = Entity.GetType().FullName ?? "null";
        int keyLen = key.Length * 4 + 1;

        Vector2 renderPos = Entity.TopCenter - new Vector2(keyLen / 2, 16);
        Text.RenderText(key, renderPos - Vector2.UnitY, Color.Black);
        Text.RenderText(key, renderPos + Vector2.UnitY, Color.Black);
        Text.RenderText(key, renderPos - Vector2.UnitX, Color.Black);
        Text.RenderText(key, renderPos + Vector2.UnitX, Color.Black);
        Text.RenderText(key, renderPos, Color.White);
    }

    [Command("tracknamespace", "Attaches a visual namespaced type display to every entity in the scene.")]
    internal static void TrackNamespace() {
        Scene scene = Engine.Instance.scene;
        foreach (Entity entity in scene.Entities)
            if (entity.Components.Get<NameTracker>() == null)
                entity.Add(new NameTracker());
    }
}
