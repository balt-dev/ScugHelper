using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[TrackedAs(typeof(Water))]
[CustomEntity("ScugHelper/HeightControllableWater")]
public class HeightControllableWater : Water {
    internal readonly string Slider;
    internal readonly float OriginalY;
    internal readonly float OriginalHeight;

    public HeightControllableWater(EntityData data, Vector2 offset) : base(data, offset) {
        Slider = data.String("Slider", "");
        OriginalY = data.Position.Y + offset.Y;
        OriginalHeight = data.Height;
        this.DisableInterpolation();

        Components.RemoveAll<DisplacementRenderHook>();
        Add(new DisplacementRenderHook(CustomRenderDisplacement));
    }

    public override void Update() {
        if (Scene is not Level level) { base.Update(); return; }
        
        float minHeight = 0f;
        if (TopSurface is not null) minHeight += 8f;
        if (BottomSurface is not null) minHeight += 8f;
        minHeight = Math.Max(minHeight, 1f);
        var newHeight = MathF.Floor(Math.Max(OriginalHeight * level.Session.GetSlider(Slider), minHeight));
        float fillY = OriginalHeight - newHeight;
        float fillHeight = newHeight;
        float deltaY = fillY - Collider.Position.Y;

        if (Scene.Tracker.GetEntity<Player>() is Player player) {
        }
        
        var origCollider = Collider;
        HashSet<Entity> seen = [];
        foreach (WaterInteraction component in Scene.Tracker.GetComponents<WaterInteraction>()) {
            // Just in case something has more than one of these for some god forsaken reason
            Entity entity = component.Entity;
            if (seen.Contains(entity)) continue;
            seen.Add(entity);

            if (entity.Right > Left && entity.Left < Right && Math.Abs(entity.CenterY - Top) < 4f) {
                if (entity is Actor actor) actor.MoveV(deltaY);
                else entity.Y += deltaY;
            }
        }

        Collider = new Hitbox(Width, newHeight, 0, fillY);
        if (TopSurface is not null) {
            TopSurface.Position = Position + Collider.Position.Floor() + new Vector2(Width / 2f, 8);
            fillY += 8;
            fillHeight -= 8f;
        }
        if (BottomSurface is not null) {
            BottomSurface.Position = Position + Collider.Position.Floor() + new Vector2(Width / 2f, Height - 8);
            fillHeight -= 8f;
        }
        fill = new Rectangle(0, (int)fillY, fill.Width, (int)fillHeight);
        
        base.Update();
    }


    private void CustomRenderDisplacement() {
        Draw.Rect(X + fill.X, Y + fill.Y, fill.Width, fill.Height, new Color(0.5f, 0.5f, 0.25f, 1f));
    }
}