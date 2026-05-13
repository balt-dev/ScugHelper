using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[CustomEntity("ScugHelper/SetInventoryAction")]
public class SetInventoryAction(EntityData data, Vector2 _) : Entity(), IAction
{
    public int dashes = data.Int("Dashes", 1);
    public bool dreamDash = data.Bool("DreamDash", true);
    public bool backpack = data.Bool("Backpack", true);
    public bool noRefills = data.Bool("NoRefills", false);
    
    public void Alert(Level level)
    {
        level.Session.Inventory = new PlayerInventory()
        {
            Dashes = dashes,
            DreamDash = dreamDash,
            Backpack = backpack,
            NoRefills = noRefills
        };
        if (level.Tracker.GetEntity<Player>() is Player player)
            player.Dashes = Math.Min(player.Dashes, dashes);
    }
}