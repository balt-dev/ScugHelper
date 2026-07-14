using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Celeste.Mod.Helpers;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.ScugHelper;

[Tracked]
public class SolidComponent(Collider collider, bool safe, int surface): Component(false, false) {
    public readonly Collider Collider = collider;
    public readonly bool Safe = safe;
    public readonly int SurfaceSoundIndex = surface;
}