using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/WaterColorController")]
public class WaterColorController : Entity {
    struct WaterColorOverride {
        internal Color FillColor;
        internal Color SurfaceColor;
        internal Color RayTopColor;
    }

    readonly WaterColorOverride WaterColor;
    readonly int ID;

    public WaterColorController(EntityData data, Vector2 _, EntityID id) : base() {
        ID = id.ID;
        WaterColor = new() {
            FillColor = data.HexColor("Fill", Color.LightSkyBlue) * data.Float("FillOpacity", 0.3f),
            SurfaceColor = data.HexColor("Surface", Color.LightSkyBlue) * data.Float("SurfaceOpacity", 0.3f),
            RayTopColor = data.HexColor("RayTop", Color.LightSkyBlue) * data.Float("RayTopOpacity", 0.3f)
        };
    }

    static readonly OrderedDictionary WaterColorOverrides = [];

    public override void Added(Scene scene) {
        base.Added(scene);
        WaterColorOverrides.Add(ID, WaterColor);
        UpdateWaterColors();
    }
    public override void Removed(Scene scene) {
        base.Removed(scene);
        WaterColorOverrides.Remove(ID);
        UpdateWaterColors();
    }
    public override void SceneEnd(Scene scene) {
        base.SceneEnd(scene);
        WaterColorOverrides.Remove(ID);
        UpdateWaterColors();
    }

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Water.Update += OnWaterUpdate;
        On.Celeste.Level.Update += OnLevelUpdate;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Water.Update -= OnWaterUpdate;
        On.Celeste.Level.Update -= OnLevelUpdate;
    }

    private static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
        UpdateWaterColors();
        orig(self);
    }

    private static void UpdateWaterColors() {
        if (WaterColorOverrides.Count > 0) {
            var col = (WaterColorOverride) WaterColorOverrides.Cast<DictionaryEntry>().Last().Value!;
            Water.FillColor = col.FillColor;
            Water.SurfaceColor = col.SurfaceColor;
            Water.RayTopColor = col.RayTopColor;
        } else {
            Water.FillColor = Color.LightSkyBlue * 0.3f;
            Water.SurfaceColor = Color.LightSkyBlue * 0.8f;
            Water.RayTopColor = Color.LightSkyBlue * 0.6f;
        }
    }
    
    private static void OnWaterUpdate(On.Celeste.Water.orig_Update orig, Water self) {
        foreach (var surface in self.Surfaces) {
            for (int i = surface.fillStartIndex; i < surface.rayStartIndex; i++)
                surface.mesh[i].Color = Water.FillColor;
            for (int i = surface.surfaceStartIndex; i < surface.mesh.Length; i++)
                surface.mesh[i].Color = Water.SurfaceColor;
        }
        orig(self);
    }
}
