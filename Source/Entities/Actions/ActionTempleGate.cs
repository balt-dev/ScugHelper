using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable
[TrackedAs(typeof(TempleGate))]
[CustomEntity("ScugHelper/ActionTempleGate")]
public class ActionTempleGate : TempleGate
{
    readonly string[] OpenGroups;
    readonly string[] CloseGroups;
    public static readonly Types ActionType = (Types)(-0x5C068197);
    public ActionTempleGate(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset, data.Height, ActionType, data.Attr("sprite", "default"), id.Level)
    {
        if (data.Bool("startOpen", false))
        {
            drawHeight = Height;
            SetHeight(0);
            open = true;
        }
        OpenGroups = IAction.GetGroups(data.String("OpenGroups"));
        CloseGroups = IAction.GetGroups(data.String("CloseGroups"));
        Add(new ActionListener(OpenGroups, (level) => { if (!open) Open(); }));
        Add(new ActionListener(CloseGroups, (level) => { if (open) Close(); }));
    }
}
#nullable restore
