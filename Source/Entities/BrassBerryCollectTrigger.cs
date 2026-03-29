using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;

[CustomEntity("ScugHelper/BrassBerryCollectTrigger")]
[Tracked(false)]
public class BrassBerryCollectTrigger(EntityData e, Vector2 offset) : Trigger(e, offset) {}
