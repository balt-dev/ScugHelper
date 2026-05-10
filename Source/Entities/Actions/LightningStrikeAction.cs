using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/LightningStrikeAction")]
public class LightningStrikeAction(EntityData data, Vector2 offset, EntityID id) : Entity(), IAction
{
    readonly float BoltHeight = data.Float("BoltHeight");
    readonly Vector2 StrikePos = data.FirstNodeNullable(offset) ?? throw new Exception("Lightning strike action must have nodes.");
    public void Alert(Level level)
    {
        Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
        level.Add(new LightningStrike(StrikePos, id.ID, BoltHeight));
    }
}
