using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using Celeste.Mod.ScugHelper;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

internal abstract class StringPart { internal abstract string Format(Level level, Player? player); }
internal class RawStringPart(string value) : StringPart
{
    public string Value = value.Replace("\\\\", "\0").Replace("\\{", "{").Replace("\\}", "}").Replace("\\n", "\n").Replace("\0", "\\");
    internal override string Format(Level level, Player? player) => Value;
}
internal class LocalizedStringPart(string key) : StringPart
{
    public string Key = key;
    internal override string Format(Level level, Player? player) => Dialog.Clean(Key);
}
internal class FlagStringPart(string name) : StringPart
{
    public string Name = name;
    internal override string Format(Level level, Player? player) => level.Session.GetFlag(Name).ToString();
}
internal class CounterStringPart(string name) : StringPart
{
    public string Name = name;
    internal override string Format(Level level, Player? player) => level.Session.GetCounter(Name).ToString();
}
internal class SliderStringPart(string name) : StringPart
{
    public string Name = name;
    internal override string Format(Level level, Player? player) => $"{(long) (level.Session.GetSlider(Name) * 100)}%";
}
internal class PlayerXStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ((long?)player?.X).ToString() ?? "?";
}
internal class PlayerYStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ((long?)player?.Y).ToString() ?? "?";
}
internal class PlayerXSubpixelStringPart : StringPart
{
    internal override string Format(Level level, Player? player) {
        if (player == null) return "?";
        return $"{player?.ExactPosition.X % 1.0:0.000}";
    }
}
internal class PlayerYSubpixelStringPart : StringPart
{
    internal override string Format(Level level, Player? player) {
        if (player == null) return "?";
        return $"{player?.ExactPosition.Y % 1.0:0.000}";
    }
}
internal class PlayerSpeedStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ((long?)player?.Speed.Length()).ToString() ?? "?";
}
internal class PlayerXSpeedStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ((long?)player?.Speed.X).ToString() ?? "?";
}
internal class PlayerYSpeedStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ((long?)player?.Speed.Y).ToString() ?? "?";
}
internal class PlayerStaminaStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ((long?)player?.Stamina).ToString() ?? "?";
}
internal class PlayerDashesStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ((long?)player?.Dashes).ToString() ?? "?";
}
internal class PlayerNameStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => SaveData.Instance.Name;
}
internal class DashTotalStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => level.Session.Dashes.ToString();
}
internal class DeathsStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => level.Session.Deaths.ToString();
}
internal class DeathsHereStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => level.Session.DeathsInCurrentLevel.ToString();
}
internal class TimeStringPart : StringPart
{
    internal override string Format(Level level, Player? player)
    {
        var totalTicks = level.Session.Time;
        var totalMsecs = totalTicks / 10000;
        var msecs = totalMsecs % 1000;
        var secs = (totalMsecs / 1000) % 60;
        var mins = (totalMsecs / (60 * 1000)) % 60;
        var hours = (totalMsecs / (60 * 60 * 1000));
        return $"{hours:00}:{mins:00}:{secs:00}.{msecs:000}";
    }
}
internal class SessionExpressionStringPart : StringPart
{
    public SessionExpressionStringPart(String raw) {
        if (!FrostHelperImports.TryCreateSessionExpression(raw, out expr))
            throw new Exception($"Session expression is invalid. {raw}");
    }
    public object? expr;
    internal override string Format(Level level, Player? player)
        => FrostHelperImports.GetSessionExpressionValue(expr, level.Session).ToString() ?? "";
}

[Tracked]
[CustomEntity("ScugHelper/Text")]
public partial class Text : Entity
{

    internal static MTexture[] glyphTexturesSmall;
    internal static MTexture[] glyphTexturesTiny;
    static Text()
    {
        var textAtlas = GFX.Game["smallFont"];
        var glyphList = new List<MTexture>();
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                glyphList.Add(new(textAtlas, x * 4, y * 6, 3, 5));
            }
        }
        glyphTexturesSmall = glyphList.ToArray();
        textAtlas = GFX.Game["tinyFont"];
        glyphList = [];
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                glyphList.Add(new(textAtlas, x * 4, y * 5, 3, 4));
            }
        }
        glyphTexturesTiny = glyphList.ToArray();
    }

    private readonly Color InfillColor;
    private readonly Color OutlineColor;
    private readonly bool DrawOutline;
    public readonly string FormatString;
    public readonly bool RequiresUpdate;
    private StringPart[]? parts;
    private string? renderedString;

    public Text(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = data.Int("Depth", 10);
        InfillColor = data.HexColor("Infill", Color.White);
        OutlineColor = data.HexColor("Outline", Color.Black);
        DrawOutline = data.Bool("DrawOutline", false);
        FormatString = data.String("Value", "<string unset>");
        RequiresUpdate = data.Bool("RequiresUpdate", false);
        CompileStringParts();
    }

    [GeneratedRegex(@"(?<!\\)\{(?:([^:}]*):)?([^\}]*)\}")]
    private static partial Regex StringPartRegex();
    private static readonly Regex stringPartRegex = StringPartRegex();

    public override void Awake(Scene scene) {
        if (scene is not Level level) { RemoveSelf(); return; }
        ConstructString(level);
    }

    private void CompileStringParts()
    {
        List<StringPart> partList = [];
        var span = FormatString.AsSpan();
        var matches = stringPartRegex.Matches(FormatString).AsEnumerable();
        int cursor = 0;
        foreach (var match in matches) {
            var rawSpan = span[cursor..match.Index];
            if (rawSpan.Length > 0) partList.Add(new RawStringPart(rawSpan.ToString()));
            cursor = match.Index + match.Length;
            var groups = match.Groups;
            var qualifier = groups[1]?.Value;
            var key = groups[2].Value;
            StringPart newPart = qualifier switch
            {
                null or "" => new LocalizedStringPart(key),
                "flag" => new FlagStringPart(key),
                "counter" => new CounterStringPart(key),
                "slider" => new SliderStringPart(key),
                "playerX" => new PlayerXStringPart(),
                "playerY" => new PlayerYStringPart(),
                "playerSubpixelX" => new PlayerXSubpixelStringPart(),
                "playerSubpixelY" => new PlayerYSubpixelStringPart(),
                "playerSpeed" => new PlayerSpeedStringPart(),
                "playerSpeedX" => new PlayerXSpeedStringPart(),
                "playerSpeedY" => new PlayerYSpeedStringPart(),
                "playerStamina" => new PlayerStaminaStringPart(),
                "playerDashes" => new PlayerDashesStringPart(),
                "playerName" => new PlayerNameStringPart(),
                "dashCount" => new DashTotalStringPart(),
                "deathCount" => new DeathsStringPart(),
                "deathRoomCount" => new DeathsHereStringPart(),
                "time" => new TimeStringPart(),
                "expr" when FrostHelperImports.IsLoaded => new SessionExpressionStringPart(key),
                _ => new RawStringPart($"{{{qualifier}:{key}}}")
            };
            partList.Add(newPart);
        }
        var lastSpan = span[cursor..];
        if (lastSpan.Length > 0) partList.Add(new RawStringPart(lastSpan.ToString()));
        parts = partList.ToArray();
    }
    private void ConstructString(Level level)
    {
        if (parts == null) return;
        Player? player = level.Tracker.GetEntity<Player>();
        renderedString = parts.Select(part => part.Format(level, player)).Aggregate((a, b) => a + b);
    }

    public override void Update()
    {
        base.Update();
        if (RequiresUpdate) ConstructString(SceneAs<Level>());
    }

    public override void Render()
    {
        base.Render();
        if (DrawOutline)
        {
            RenderText(new Vector2(-1, -1), OutlineColor);
            RenderText(new Vector2(-1, 0), OutlineColor);
            RenderText(new Vector2(-1, 1), OutlineColor);
            RenderText(new Vector2(0, -1), OutlineColor);
            RenderText(new Vector2(0, 1), OutlineColor);
            RenderText(new Vector2(1, -1), OutlineColor);
            RenderText(new Vector2(1, 0), OutlineColor);
            RenderText(new Vector2(1, 1), OutlineColor);
        }
        RenderText(Vector2.Zero, InfillColor);
    }

    internal void RenderText(Vector2 offset, Color color) {
        RenderText(renderedString, Position + offset, color);
    }

    internal static void RenderText(string renderedString, Vector2 offset, Color color) {
        if (renderedString == null) return;
        Vector2 printHead = Vector2.Zero;
        foreach (var chr in renderedString.AsEnumerable()) {
            if (chr == '\n') { printHead.X = 0; printHead.Y += ScugHelperModule.Settings.AlternativeFont ? 6 : 5; continue; }
            int codepoint = chr;
            if (codepoint < 32) { continue; }
            var index = Math.Clamp(codepoint, 32, 127) - 32;
            var tex = (ScugHelperModule.Settings.AlternativeFont ? glyphTexturesSmall : glyphTexturesTiny)[index];
            tex.Draw(offset + printHead, Vector2.Zero, color);
            printHead.X += 4;
        }
    }
}

#nullable restore
