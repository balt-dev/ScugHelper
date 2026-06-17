using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using Celeste.Mod.ScugHelper;
using System.Text.Json;
using System.IO;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

internal abstract class StringPart {
    internal string? CachedValue;
    internal Color? Color = null;
    internal abstract string Format(Level level, Player? player);
}
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
    internal override string Format(Level level, Player? player) => $"{level.Session.GetSlider(Name):0.000}";
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
        return player!.movementCounter.X >= 0 ? $"{player!.movementCounter.X:+0.000}" : $"{player!.movementCounter.X:0.000}";
    }
}
internal class PlayerYSubpixelStringPart : StringPart
{
    internal override string Format(Level level, Player? player) {
        if (player == null) return "?";
        return player!.movementCounter.Y >= 0 ? $"{player!.movementCounter.Y:+0.000}" : $"{player!.movementCounter.Y:0.000}";
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
    internal override string Format(Level level, Player? player) {
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
    public SessionExpressionStringPart(string raw) {
        if (!FrostHelperImports.TryCreateSessionExpression(raw, out expr))
            throw new Exception($"Session expression is invalid. {raw}");
    }
    public object? expr;
    internal override string Format(Level level, Player? player)
        => expr is null ? "" : FrostHelperImports.GetSessionExpressionValue(expr, level.Session).ToString() ?? "";
}
internal class DebugStringPart : StringPart
{
    internal override string Format(Level level, Player? player) => ScugHelperModule.DebugText;
}
internal class ColorStringPart : StringPart {
    public ColorStringPart(string hex) => Color = Calc.HexToColor(hex);
    internal override string Format(Level level, Player? player) => "";
}

[Tracked]
[CustomEntity("ScugHelper/Text")]
public partial class Text : Entity
{
    internal enum OutlineType {
        None,
        Full,
        Edge,
        DropShadow
    }

    internal MTexture[]? glyphTextures;

    private readonly float Opacity;
    private readonly Vector2 Parallax;
    private readonly Vector2 ParallaxOffset;
    private readonly Color InfillColor;
    private readonly string? Flag;
    private readonly bool InvertFlag;
    private readonly Color OutlineColor;
    private readonly OutlineType Outline;
    public readonly string FormatString;
    public readonly float UpdateFrequency;
    private StringPart[]? parts;
    private int GlyphWidth;
    private int GlyphHeight;
    private VirtualRenderTarget? bakedTexture;
    private readonly EntityID ID;
    private int BufferWidth;
    private int BufferHeight;
    private bool WantsBakeTexture = true;
    private readonly bool Persistent;

    public Text(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        ID = id;
        Tag |= Tags.TransitionUpdate | Tags.FrozenUpdate;
        if (Persistent = data.Bool("Persistent", false))
            Tag |= Tags.Persistent;
        Opacity = data.Float("Opacity", 1);
        Parallax = new(data.Float("ParallaxX", 0), data.Float("ParallaxY", 0));
        ParallaxOffset = new(data.Float("ParallaxOffsetX", 0), data.Float("ParallaxOffsetY", 0));
        Depth = data.Int("Depth", 10);
        InfillColor = data.HexColor("Infill", Color.White);
        OutlineColor = data.HexColor("Outline", Color.Black);
        Outline = data.Enum("OutlineType", data.Bool("DrawOutline") ? OutlineType.Full : OutlineType.None);
        FormatString = data.String("Value", "<string unset>");
        UpdateFrequency = data.Bool("RequiresUpdate", false) ? 0.01f : data.Float("UpdateFrequency", 0.1f);
        Flag = data.String("Flag")?.Trim();
        InvertFlag = data.Bool("InvertFlag", false);
        if (Flag is string flag && flag.Length == 0) Flag = null;
        var fontTexture = data.String("FontTexture", "objects/ScugHelper/text/smallFont");
        SliceFont(fontTexture);
        CompileStringParts();
        Add(new BeforeRenderHook(BakeTexture));
    }

    private void BakeTexture() {
        if (WantsBakeTexture) {
            WantsBakeTexture = false;
            var oldTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();

            if (bakedTexture is null || bakedTexture.Width < BufferWidth || bakedTexture.Height < BufferHeight) {
                bakedTexture?.Dispose();
                bakedTexture = VirtualContent.CreateRenderTarget($"bakedText_{ID}", BufferWidth, BufferHeight);
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(bakedTexture);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin();

            if (Outline is OutlineType.Full) RenderText(new Vector2(0, 0), OutlineColor);
            if (Outline is OutlineType.Full or OutlineType.Edge) RenderText(new Vector2(0, 1), OutlineColor);
            if (Outline is OutlineType.Full) RenderText(new Vector2(0, 2), OutlineColor);
            if (Outline is OutlineType.Full or OutlineType.Edge) RenderText(new Vector2(1, 0), OutlineColor);
            if (Outline is OutlineType.Full or OutlineType.Edge) RenderText(new Vector2(1, 2), OutlineColor);
            if (Outline is OutlineType.Full) RenderText(new Vector2(2, 0), OutlineColor);
            if (Outline is OutlineType.Full or OutlineType.Edge) RenderText(new Vector2(2, 1), OutlineColor);
            if (Outline is OutlineType.Full or OutlineType.DropShadow) RenderText(new Vector2(2, 2), OutlineColor);
            RenderText(new Vector2(1, 1), InfillColor);

            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTargets(oldTargets);
        }
    }

    private void SliceFont(string fontTexturePath) {
        var textAtlas = GFX.Game[fontTexturePath];
        GlyphWidth = textAtlas.Width / 16 - 1;
        GlyphHeight = textAtlas.Height / 6 - 1;
        var glyphList = new List<MTexture>();
        for (int y = 0; y < 6; y++) {
            for (int x = 0; x < 16; x++) {
                glyphList.Add(new(textAtlas, x * (GlyphWidth + 1), y * (GlyphHeight + 1), GlyphWidth, GlyphHeight));
            }
        }
        glyphTextures = glyphList.ToArray();
    }

    [GeneratedRegex(@"(?<!\\)\{(?:([^:}]*):)?([^\}]*)\}")]
    private static partial Regex StringPartRegex();
    private static readonly Regex stringPartRegex = StringPartRegex();

    public override void Awake(Scene scene) {
        if (scene is not Level level) { Logger.Warn(nameof(ScugHelper), "Tried to add Text to a non-level. Removing."); RemoveSelf(); return; }
        ConstructString(level);
        if (Persistent)
            level.Session.DoNotLoad.Add(ID);
    }
    
    public override void SceneEnd(Scene scene) {
        if (Persistent)
            (scene as Level)?.Session.DoNotLoad.Remove(ID);
    }

    private void CompileStringParts()
    {
        List<StringPart> partList = [];
        var span = FormatString.AsSpan();
        var matches = stringPartRegex.Matches(FormatString).AsEnumerable();
        int cursor = 0;
        foreach (var match in matches)
        {
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
                "color" => new ColorStringPart(key),
                "__debug" => new DebugStringPart(),
                _ => new RawStringPart($"{{{qualifier}:{key}}}")
            };
            partList.Add(newPart);
        }
        var lastSpan = span[cursor..];
        if (lastSpan.Length > 0) partList.Add(new RawStringPart(lastSpan.ToString()));
        parts = partList.ToArray();
    }

    private void ConstructString(Level level) {
        if (parts == null) return;
        Player? player = level.Tracker.GetEntity<Player>();

        float maxX = 0f;
        Vector2 printHead = Vector2.Zero;
        foreach (StringPart part in parts) {
            string oldValue = part.CachedValue!;
            part.CachedValue = part.Format(level, player);
            if (part.CachedValue != oldValue) WantsBakeTexture = true;

            foreach (char chr in part.CachedValue) {
                if (chr == '\n') { printHead.X = 0; printHead.Y += GlyphHeight + 1; }
                else { printHead.X += GlyphWidth + 1; maxX = MathF.Max(maxX, printHead.X); }
            }
        }
        BufferWidth = (int) maxX + 1;
        BufferHeight = (int) printHead.Y + GlyphHeight + 2;
    }

    public override void Update() {
        base.Update();
        WantsBakeTexture |= bakedTexture is null;
        if (UpdateFrequency > 0 && Scene.OnRawInterval(UpdateFrequency)) ConstructString(SceneAs<Level>());
    }

    public override void Render() {
        base.Render();
        Level level = SceneAs<Level>();
        if (Flag is string flag && (!level.Session.GetFlag(flag) ^ InvertFlag)) return;
        Vector2 renderPosition = Position - Vector2.One;
        renderPosition.X = float.Lerp(renderPosition.X, level.Camera.Position.X, Parallax.X) + ParallaxOffset.X;
        renderPosition.Y = float.Lerp(renderPosition.Y, level.Camera.Position.Y, Parallax.Y) + ParallaxOffset.Y;
        if (bakedTexture is not null) Draw.SpriteBatch.Draw(bakedTexture, renderPosition, Color.White * Opacity);
        else Logger.Warn(nameof(ScugHelper), "Text bakedTexture is null?");
    }

    internal void RenderText(Vector2 offset, Color color, bool forceColor = false) {
        if (parts is null) return;
        Vector2 printHead = Vector2.Zero;
        Color currentColor = color;
        foreach (var part in parts) {
            string cachedChars = part.CachedValue!;
            currentColor = forceColor ? color : (part.Color is not Color partColor ? currentColor : new(color.ToVector4() * partColor.ToVector4()));
            foreach (char chr in cachedChars) {
                if (chr == '\n') { printHead.X = 0; printHead.Y += GlyphHeight + 1; continue; }
                int codepoint = chr;
                if (codepoint < 32) { continue; }
                var index = Math.Clamp(codepoint, 32, 127) - 32;
                var tex = glyphTextures![index];
                tex.Draw(offset + printHead, Vector2.Zero, currentColor);
                printHead.X += GlyphWidth + 1;
            }
        }
    }
}

#nullable restore
