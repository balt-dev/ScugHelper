using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable

[Tracked(false)]
[RegisterStrawberry(true, false)]
[CustomEntity("ScugHelper/ActionWingedBerry")]
public class ActionWingedBerry : Strawberry, IStrawberry {
    readonly string[] Groups;
    public ActionWingedBerry(EntityData data, Vector2 offset, EntityID gid)
        : base(data, offset, gid)
    {
        Golden = false;
        Winged = true;
        Groups = IAction.GetGroups(data);
        Components.RemoveAll<DashListener>();
        Add(new ActionListener(Groups, OnAlert));
    }

    private void OnAlert(Level level) => OnDash(Vector2.UnitX);
}
#nullable restore
