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
    private static readonly float CameraSpeed = 320;
    private static readonly float CameraAcceleration = 1600f;
    private static readonly float ReturnTime = 0.4f;

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

    internal static VirtualJoystick MinimapAim = new(ScugHelperModule.Settings.MinimapUp.Binding, ScugHelperModule.Settings.MinimapDown.Binding, ScugHelperModule.Settings.MinimapLeft.Binding, ScugHelperModule.Settings.MinimapRight.Binding, 0, 0.3f);

    internal Camera Camera;
    internal Vector2 Speed;
    internal float Opacity = ScugHelperModule.Settings.Minimap.UnfocusedOpacity;
    internal List<LevelTemplate> templates;

    private Vector2 oldPosition;
    private static bool focusToggle = false;
    private static float unfocusedTimer = 100f;
    internal static bool Focused
    {
        get => ScugHelperModule.Settings.Minimap.ButtonBehavior switch
        {
            MinimapBindBehavior.Toggle => focusToggle,
            MinimapBindBehavior.Hold => ScugHelperModule.Settings.MinimapBind.Check,
            MinimapBindBehavior.Invert => !ScugHelperModule.Settings.MinimapBind.Check,
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
        Camera.Zoom = ZoomTarget = (scene as Level).Camera.Zoom * 4f;
    }

    public override void Update()
    {
        base.Update();
        var levelCam = SceneAs<Level>().Camera;
        if (ScugHelperModule.Settings.Minimap is null) throw new Exception("Minimap null!?");
        if (ScugHelperModule.Settings.MinimapBind is null) throw new Exception("Minimap bind null!?");
        if (ScugHelperModule.Settings.Minimap.ButtonBehavior == MinimapBindBehavior.Toggle && ScugHelperModule.Settings.MinimapBind.Pressed)
        {
            ScugHelperModule.Settings.MinimapBind.ConsumePress();
            focusToggle = !focusToggle;
        }

        if (Focused)
        {
            Speed = Calc.Approach(Speed, MinimapAim.Value * CameraSpeed / Camera.Zoom, CameraAcceleration / Camera.Zoom * Engine.RawDeltaTime);
            if (ScugHelperModule.Settings.MinimapZoomIn.Pressed)
            {
                ScugHelperModule.Settings.MinimapZoomIn.ConsumePress();
                ZoomTarget *= 2f;
            }
            if (ScugHelperModule.Settings.MinimapZoomOut.Pressed)
            {
                ScugHelperModule.Settings.MinimapZoomOut.ConsumePress();
                ZoomTarget *= 0.5f;
            }
            Camera.Position += Speed * Engine.RawDeltaTime;
            Opacity = float.Lerp(Opacity, ScugHelperModule.Settings.Minimap.FocusedOpacity, 1f - MathF.Pow(0.0001f, Engine.RawDeltaTime));
            oldPosition = Camera.Position;
            unfocusedTimer = 0f;
        }
        else
        {
            Speed = Vector2.Zero;
            float positionFac = 1 - MathF.Pow(1 - Math.Clamp(unfocusedTimer / ReturnTime, 0.0f, 1.0f), 3);
            Vector2 camSize = new(levelCam.Viewport.Bounds.Width, levelCam.Viewport.Bounds.Height);
            Vector2 levelCamCenter = levelCam.Position + camSize / 2;
            Camera.Position = Vector2.Lerp(oldPosition, levelCamCenter / 8f, positionFac);
            ZoomTarget = levelCam.Zoom * 4f;
            Opacity = float.Lerp(Opacity, ScugHelperModule.Settings.Minimap.UnfocusedOpacity, 1f - MathF.Pow(0.0001f, Engine.RawDeltaTime));
            unfocusedTimer += Engine.RawDeltaTime;
        }
        Camera.Zoom = float.Lerp(Camera.Zoom, ZoomTarget, 1f - MathF.Pow(0.1f, Engine.RawDeltaTime));
    }

    VirtualRenderTarget buffer;
    private float ZoomTarget;

    public void BeforeRender()
    {
        var level = SceneAs<Level>();
        var settings = ScugHelperModule.Settings.Minimap;
        buffer ??= VirtualContent.CreateRenderTarget("minimap-renderer", settings.MinimapWidth, settings.MinimapHeight);

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        Vector2 viewportOffset = new(Camera.Viewport.Width / Camera.Zoom / 2, Camera.Viewport.Height / Camera.Zoom / 2);
        Camera.Position -= viewportOffset;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Camera.Matrix);

        Engine.Graphics.GraphicsDevice.Clear(Color.Black);

        foreach (var temp in templates)
        {
            if (!temp.Rect.Intersects(new Rectangle((int)Camera.Left, (int)Camera.Top, (int)(Camera.Right - Camera.Left), (int)(Camera.Bottom - Camera.Top)))) continue;
            temp.RenderOutline(Camera);
            temp.RenderContents(Camera, templates);
            if (level.Session.LevelData.Name == temp.Name)
                temp.RenderHighlight(Camera, true, false);
        }
        if (level.Tracker.GetEntity<Player>() is Player player)
            Draw.Pixel.Draw((player.Position / 8f).Round() - Vector2.UnitY, Vector2.Zero, Color.Pink);

        Draw.SpriteBatch.End();

        Camera.Position += viewportOffset;
    }

    public override void Render()
    {
        base.Render();

        Draw.SpriteBatch.Draw(buffer.Target, new(ScugHelperModule.Settings.Minimap.MinimapX, ScugHelperModule.Settings.Minimap.MinimapY), null, Color.White * Opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

    }
}
