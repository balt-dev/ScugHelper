using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Monocle;
using MonoMod.Utils;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/DebugWall")]
public class DebugWall : Entity
{
    readonly bool Background;
    readonly int MyWidth;
    readonly int MyHeight;

    public DebugWall(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Background = data.Bool("Background");
        MyWidth = data.Width / 8;
        MyHeight = data.Height / 8;
        List<Collider> colliders = [];
        for (int x = 0; x < MyWidth; x++)
            for (int y = 0; y < MyHeight; y++)
                colliders.Add(new Hitbox(8, 8, x * 8, y * 8));
        Collider = new ColliderList([.. colliders]);
    }

    [OnLoad]
    internal static void LoadHooks()
    {
        On.Celeste.Editor.LevelTemplate.ctor_LevelData += OnCtorLevelData;
    }

    [OnUnload]
    internal static void UnloadHooks()
    {
        On.Celeste.Editor.LevelTemplate.ctor_LevelData -= OnCtorLevelData;
    }

    private static void OnCtorLevelData(On.Celeste.Editor.LevelTemplate.orig_ctor_LevelData orig, Editor.LevelTemplate self, LevelData data)
    {
        orig(self, data);
        foreach (EntityData entData in data.Entities)
        {
            if (entData.Name == "ScugHelper/DebugWall")
            {
                Rectangle rect = new((int)entData.Position.X / 8, (int)entData.Position.Y / 8, entData.Width / 8, entData.Height / 8);
                if (entData.Bool("Background")) self.backs.Add(rect);
                else self.solids.Add(rect);
            }
        }
    }
}
