using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Monocle;
using MonoMod.Utils;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using System.Linq;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[CustomEntity("ScugHelper/CassetteFlagController")]
public partial class CassetteFlagController : Entity
{
    public partial struct FlagSpan(string flag, int start, int end)
    {
        public string? Flag = flag;
        public int Start = start;
        public int End = end;
        public readonly bool InSpan(int beat) => beat >= Start && beat <= End;

        [GeneratedRegex(@"^(\S+)\s+([0-9]+)-([0-9]+)$")]
        private static partial Regex FlagSpanRegex();
        static readonly Regex regex = FlagSpanRegex();
        public static FlagSpan Parse(string str)
        {
            var match = regex.Match(str);
            if (!match.Success) throw new FormatException($"Failed to parse flag span {str}: Invalid string format");
            string flag = match.Groups[1].Value;
            if (!int.TryParse(match.Groups[2].Value, out int start)) throw new FormatException($"Failed to parse flag span {str}: Start {match.Groups[2].Value} is not a valid 32-bit integer");
            if (!int.TryParse(match.Groups[3].Value, out int end)) throw new FormatException($"Failed to parse flag span {str}: End {match.Groups[3].Value} is not a valid 32-bit integer");
            if (start >= end) throw new FormatException($"Failed to parse flag span {str}: Start ({start}) must be less than end ({end})");
            return new FlagSpan(flag, start, end);
        }
    }

    public readonly List<FlagSpan> Spans;
    public readonly int Length;

    CassetteBlockManager? manager;

    public CassetteFlagController(EntityData data, Vector2 _) : base()
    {
        Spans = data.String("Spans", "")
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(FlagSpan.Parse)
        .ToList();
        Length = data.Int("Length", 16);
        if (Length <= 0) throw new Exception("Length for cassette flag controller must be greater than than 0.");
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if ((manager = scene.Tracker.GetEntity<CassetteBlockManager>()) is null)
            scene.Add(manager = []);
    }

    public override void Update() {
        base.Update();
        Level level = SceneAs<Level>();
        if (manager is not CassetteBlockManager man) return;
        foreach (FlagSpan span in Spans) {
            level.Session.SetFlag(span.Flag, span.InSpan((man.beatIndex + man.beatIndexOffset) % Length + 1));
        }
    }
}
