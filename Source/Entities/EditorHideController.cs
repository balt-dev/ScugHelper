using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Monocle;
using MonoMod.Utils;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
using System.Reflection;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/EditorHideController")]
public class EditorHideController() : Entity()
{

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Editor.MapEditor.ctor += OnEditorCtor;
        On.Celeste.Editor.LevelTemplate.ctor_LevelData += OnCtorLevelData;
        On.Celeste.Editor.LevelTemplate.RenderContents += OnRenderContents;
        On.Celeste.Editor.LevelTemplate.RenderOutline += OnRenderOutline;
        On.Celeste.Editor.LevelTemplate.RenderHighlight += OnRenderHighlight;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Editor.MapEditor.ctor -= OnEditorCtor;
        On.Celeste.Editor.LevelTemplate.ctor_LevelData -= OnCtorLevelData;
        On.Celeste.Editor.LevelTemplate.RenderContents -= OnRenderContents;
        On.Celeste.Editor.LevelTemplate.RenderOutline -= OnRenderOutline;
        On.Celeste.Editor.LevelTemplate.RenderHighlight -= OnRenderHighlight;
    }

    private static void OnEditorCtor(On.Celeste.Editor.MapEditor.orig_ctor orig, Editor.MapEditor self, AreaKey area, bool reloadMapData) {
        orig(self, area, reloadMapData);
        self.levels.RemoveAll((temp) => (DynamicData.For(temp).Get("ScugHelper_HasHidden") as bool?) ?? false);
    }

    private static void OnCtorLevelData(On.Celeste.Editor.LevelTemplate.orig_ctor_LevelData orig, Editor.LevelTemplate self, LevelData data) {
        orig(self, data);
        bool isHidden = false;
        foreach (EntityData entData in data.Entities)
            if (entData.Name == "ScugHelper/EditorHideController")
                isHidden = true;
        var dynData = DynamicData.For(self);
        dynData.Set("ScugHelper_HasHidden", isHidden);
    }

    private static void OnRenderHighlight(On.Celeste.Editor.LevelTemplate.orig_RenderHighlight orig, Editor.LevelTemplate self, Camera camera, bool hovered, bool selected) {
        var data = DynamicData.For(self);
        if (data.Get("ScugHelper_HasHidden") is true) return;
        orig(self, camera, hovered, selected);
    }

    private static void OnRenderOutline(On.Celeste.Editor.LevelTemplate.orig_RenderOutline orig, Editor.LevelTemplate self, Camera camera) {
        var data = DynamicData.For(self);
        if (data.Get("ScugHelper_HasHidden") is true) return;
        orig(self, camera);
    }

    private static void OnRenderContents(On.Celeste.Editor.LevelTemplate.orig_RenderContents orig, Editor.LevelTemplate self, Camera camera, List<Editor.LevelTemplate> allLevels) {
        var data = DynamicData.For(self);
        if (data.Get("ScugHelper_HasHidden") is true) return;
        orig(self, camera, allLevels);
    }
}
