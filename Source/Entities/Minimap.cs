using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Editor;

namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

public enum MinimapBindBehavior
{
    Toggle,
    Hold,
    Invert
}

[CustomEntity("ScugHelper/Minimap")]
public class MinimapEntity : Entity
{
    private static readonly float CameraSpeed = 300f;
    
    [OnLoad]
    public static void LoadHooks()
    {
        Everest.Events.LevelLoader.OnLoadingThread += OnLevelLoad;
    }
    [OnUnload]
    public static void UnloadHooks()
    {
        Everest.Events.LevelLoader.OnLoadingThread -= OnLevelLoad;
    }

    private static void OnLevelLoad(Level level)
    {
        level.Add(new MinimapEntity());
    }

    internal Camera Camera;
    internal Vector2 Speed;
    internal float Opacity = ScugHelperModule.Settings.Minimap.UnfocusedOpacity;
    internal List<LevelTemplate> templates;

    private static bool focusToggle = false;
    internal static bool Focused
    {
        get => ScugHelperModule.Settings.Minimap.ButtonBehavior switch
        {
            MinimapBindBehavior.Toggle => focusToggle,
            MinimapBindBehavior.Hold => ScugHelperModule.Settings.Minimap.MinimapBind.Check,
            MinimapBindBehavior.Invert => !ScugHelperModule.Settings.Minimap.MinimapBind.Check,
        };
    }
    public MinimapEntity() : base()
    {
        Camera = new(ScugHelperModule.Settings.Minimap.MinimapWidth, ScugHelperModule.Settings.Minimap.MinimapHeight);
        Tag |= Tags.Global | Tags.HUD | Tags.TransitionUpdate | Tags.FrozenUpdate;
        Add(new BeforeRenderHook(BeforeRender));
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        focusToggle = false;
        templates = (scene as Level).Session.MapData.Levels.Select((data) => new LevelTemplate(data)).ToList();
    }
    
    public override void Update()
    {
        base.Update();
        var levelCam = SceneAs<Level>().Camera;
        if (ScugHelperModule.Settings.Minimap.ButtonBehavior == MinimapBindBehavior.Toggle && ScugHelperModule.Settings.Minimap.MinimapBind.Pressed)
        {
            ScugHelperModule.Settings.Minimap.MinimapBind.ConsumePress();
            focusToggle = !focusToggle;
        }

        if (Focused)
        {
            Speed = Calc.Approach(Speed, Input.Aim.Value * CameraSpeed, Engine.RawDeltaTime);
            Camera.Position += Speed * Engine.RawDeltaTime;
            Opacity = float.Lerp(Opacity, ScugHelperModule.Settings.Minimap.FocusedOpacity, 1f - MathF.Pow(0.1f, Engine.RawDeltaTime));
        }
        else
        {
            Speed = Vector2.Zero;
            Camera.Position = levelCam.Position;
            Camera.Zoom = levelCam.Zoom;
            Opacity = float.Lerp(Opacity, ScugHelperModule.Settings.Minimap.UnfocusedOpacity, 1f - MathF.Pow(0.1f, Engine.RawDeltaTime));
        }
    }
    
    VirtualRenderTarget buffer;
    public void BeforeRender()
    {
        var level = SceneAs<Level>();
        var settings = ScugHelperModule.Settings.Minimap;
        buffer ??= VirtualContent.CreateRenderTarget("minimap-renderer", settings.MinimapWidth, settings.MinimapHeight);

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Camera.Matrix);

        Engine.Graphics.GraphicsDevice.Clear(Color.Black);
        
        foreach (var temp in templates) {
            if (!temp.Rect.Intersects(new Rectangle((int)Camera.Left, (int)Camera.Top, (int)(Camera.Right - Camera.Left), (int)(Camera.Bottom - Camera.Top)))) continue;
            temp.RenderOutline(Camera);
            temp.RenderContents(Camera, templates);
            if (level.Session.LevelData.Name == temp.Name)
                temp.RenderHighlight(Camera, true, false);
        }

        Draw.SpriteBatch.End();
    }

    public override void Render()
    {
        base.Render();
    
        Draw.SpriteBatch.Draw(buffer.Target, new(ScugHelperModule.Settings.Minimap.MinimapX, ScugHelperModule.Settings.Minimap.MinimapY), null, Color.White * Opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

    }
}