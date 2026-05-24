using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Editor;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Utils;

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
    public static void LoadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread += OnLevelLoad;
    }
    [OnUnload]
    public static void UnloadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread -= OnLevelLoad;
    }

    private static void OnLevelLoad(Level level) {
        level.Add(new MinimapEntity());
    }

    internal Camera Camera;
    internal Vector2 Speed;
    internal float Opacity = ScugHelperModule.Settings.Minimap.UnfocusedOpacity;
    internal List<LevelTemplate> templates;

    private Vector2 oldPosition;
    private static bool focusToggle = false;
    private static bool visibleToggle = true;
    private static float unfocusedTimer = 100f;
    internal static bool Focused
    {
        get => ScugHelperModule.Settings.Minimap.Minimap && focusToggle;
    }
    internal static bool Visible
    {
        get => ScugHelperModule.Settings.Minimap.Minimap && (focusToggle || visibleToggle);
    }
    public MinimapEntity() : base() {
        Camera = new(ScugHelperModule.Settings.Minimap.MinimapWidth, ScugHelperModule.Settings.Minimap.MinimapHeight);
        Tag |= Tags.Global | Tags.HUD | Tags.TransitionUpdate | Tags.FrozenUpdate;
        Add(new BeforeRenderHook(BeforeRender));
        ScugHelperModule.Session.RenderedEditorOnce = false;
    }
    public override void Awake(Scene scene) {
        base.Awake(scene);
        focusToggle = false;
        templates = (scene as Level).Session.MapData.Levels.Select((data) => new LevelTemplate(data)).ToList();
        Camera.Zoom = ZoomTarget = (scene as Level).Camera.Zoom * 4f;
    }

    public override void Update() {
        base.Update();
        if (!ScugHelperModule.Settings.Minimap.Minimap) { focusToggle = false; return; }
        var levelCam = SceneAs<Level>().Camera;
        if (ScugHelperModule.Settings.Minimap is null) throw new Exception("Minimap null!?");
        if (ScugHelperModule.Settings.MinimapBind is null) throw new Exception("Minimap bind null!?");
        if (ScugHelperModule.Settings.MinimapBind.Pressed) {
            ScugHelperModule.Settings.MinimapBind.ConsumePress();
            focusToggle = !focusToggle;
        }
        if (ScugHelperModule.Settings.MinimapVisibleBind.Pressed) {
            ScugHelperModule.Settings.MinimapVisibleBind.ConsumePress();
            visibleToggle = !visibleToggle;
        }

        if (Focused) {
            Speed = Calc.Approach(Speed, Input.Aim.Value * CameraSpeed / Camera.Zoom, CameraAcceleration / Camera.Zoom * Engine.RawDeltaTime);
            if (ScugHelperModule.Settings.MinimapZoomIn.Pressed) {
                ScugHelperModule.Settings.MinimapZoomIn.ConsumePress();
                ZoomTarget *= 2f;
            }
            if (ScugHelperModule.Settings.MinimapZoomOut.Pressed) {
                ScugHelperModule.Settings.MinimapZoomOut.ConsumePress();
                ZoomTarget *= 0.5f;
            }
            Camera.Position += Speed * Engine.RawDeltaTime;
            Opacity = float.Lerp(Opacity, ScugHelperModule.Settings.Minimap.FocusedOpacity, 1f - MathF.Pow(0.0001f, Engine.RawDeltaTime));
            oldPosition = Camera.Position;
            unfocusedTimer = 0f;
        } else {
            Speed = Vector2.Zero;
            float positionFac = 1 - MathF.Pow(1 - Math.Clamp(unfocusedTimer / ReturnTime, 0.0f, 1.0f), 3);
            Vector2 camSize = new(levelCam.Viewport.Bounds.Width, levelCam.Viewport.Bounds.Height);
            Vector2 levelCamCenter = levelCam.Position + camSize / 2;
            Camera.Position = Vector2.Lerp(oldPosition, levelCamCenter / 8f, positionFac);
            ZoomTarget = levelCam.Zoom * 4f;
            Opacity = float.Lerp(Opacity, visibleToggle ? ScugHelperModule.Settings.Minimap.UnfocusedOpacity : 0f, 1f - MathF.Pow(0.0001f, Engine.RawDeltaTime));
            unfocusedTimer += Engine.RawDeltaTime;
        }
        Camera.Zoom = float.Lerp(Camera.Zoom, ZoomTarget, 1f - MathF.Pow(0.1f, Engine.RawDeltaTime));
    }

    VirtualRenderTarget buffer;
    private float ZoomTarget;

    public void BeforeRender() {
        if (!ScugHelperModule.Settings.Minimap.Minimap) return;
        
        var level = SceneAs<Level>();
        var settings = ScugHelperModule.Settings.Minimap;
        
        Vector2 viewportOffset = new(Camera.Viewport.Width / Camera.Zoom / 2, Camera.Viewport.Height / Camera.Zoom / 2);
        Camera.Position -= viewportOffset;
        
        if (!ScugHelperModule.Session.RenderedEditorOnce) {
            // GameHelper compat
            VirtualRenderTarget scratchBuffer = VirtualContent.CreateRenderTarget("minimap-scratch", settings.MinimapWidth, settings.MinimapHeight);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(scratchBuffer);
            MapEditor ed = new(level.Session.Area, false); 
            DynamicData.For(ed).Set("CurrentSession", level.Session);
            ed.Render();
            ScugHelperModule.Session.RenderedEditorOnce = true;
            scratchBuffer.Dispose();
        }
        buffer ??= VirtualContent.CreateRenderTarget("minimap-renderer", settings.MinimapWidth, settings.MinimapHeight);

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Camera.Matrix);

        Engine.Graphics.GraphicsDevice.Clear(Color.Black);

        foreach (var temp in templates) {
            if (!temp.Rect.Intersects(new Rectangle((int)Camera.Left, (int)Camera.Top, (int)(Camera.Right - Camera.Left), (int)(Camera.Bottom - Camera.Top)))) continue;
            try
            {
                temp.RenderOutline(Camera);
                temp.RenderContents(Camera, templates);
                if (level.Session.LevelData.Name == temp.Name)
                    temp.RenderHighlight(Camera, true, false);
            } catch (NullReferenceException) {
                Logger.Warn(nameof(ScugHelperModule), "GameHelper is fucking with the minimap. Caught NullReferenceException.");
                // GameHelper does this sometimes for some reason.
            }
        }
        if (level.Tracker.GetEntity<Player>() is Player player) {
            Draw.Pixel.Draw((player.Position / 8f).Round() - Vector2.UnitY, Vector2.Zero, Color.Pink);
            Draw.SpriteBatch.DrawString(Draw.DefaultFont, $"{level.Session.LevelData.Name}", Camera.Position, Color.Yellow, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
            Draw.SpriteBatch.DrawString(Draw.DefaultFont, $"{player.X}, {player.Y}", Camera.Position + Vector2.UnitY * 6, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
        }

        Draw.SpriteBatch.End();

        Camera.Position += viewportOffset;
    }



    public override void Render() {
        base.Render();
        if (!ScugHelperModule.Settings.Minimap.Minimap) return;

        Draw.SpriteBatch.Draw(buffer.Target, new(ScugHelperModule.Settings.Minimap.MinimapX, ScugHelperModule.Settings.Minimap.MinimapY), null, Color.White * Opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        buffer?.Dispose();
    }

    private static Hook hookButtonCheck;
    private static Hook hookButtonPressed;
    private static Hook hookButtonReleased;
    private static Hook hookGrabCheck;
    private static Hook hookDashPressed;
    private static Hook hookCrouchDashPressed;

    [OnLoad]
    public static void Load() {
        // break directions
        On.Celeste.Player.Update += BreakTheControls;

        // break Input.X.Check, Input.X.Pressed, Input.X.Released with X being Jump, Dash, Grab or CrouchDash
        hookButtonCheck = new Hook(typeof(VirtualButton).GetMethod("get_Check"), HookOnButton);
        hookButtonPressed = new Hook(typeof(VirtualButton).GetMethod("get_Pressed"), HookOnButton);
        hookButtonReleased = new Hook(typeof(VirtualButton).GetMethod("get_Released"), HookOnButton);

        // break Input.GrabCheck and Input.DashPressed
        hookGrabCheck = new Hook(typeof(Input).GetMethod("get_GrabCheck"), ModGrabResult);
        hookDashPressed = new Hook(typeof(Input).GetMethod("get_DashPressed"), ModDashResult);
        hookCrouchDashPressed = new Hook(typeof(Input).GetMethod("get_CrouchDashPressed"), ModDashResult);
    }

    [OnUnload]
    public static void Unload() {
        On.Celeste.Player.Update -= BreakTheControls;

        hookButtonCheck?.Dispose();
        hookButtonPressed?.Dispose();
        hookButtonReleased?.Dispose();
        hookGrabCheck?.Dispose();
        hookDashPressed?.Dispose();
        hookCrouchDashPressed?.Dispose();
    }

    private static void BreakTheControls(On.Celeste.Player.orig_Update orig, Player self) {
        if (!Focused) {
            orig(self);
            return;
        }

        Vector2 oldAim = Input.Aim;
        int oldMoveX = Input.MoveX.Value;
        int oldMoveY = Input.MoveY.Value;

        Input.Aim.Value = Vector2.Zero;
        Input.MoveX.Value = 0;
        Input.MoveY.Value = 0;

        orig(self);

        Input.Aim.Value = oldAim;
        Input.MoveX.Value = oldMoveX;
        Input.MoveY.Value = oldMoveY;
    }

    private static bool HookOnButton(Func<VirtualButton, bool> orig, VirtualButton self)
        => (!Focused || !(self == Input.Dash || self == Input.CrouchDash || self == Input.Jump || self == Input.Grab)) && orig(self);

    private static bool ModGrabResult(Func<bool> orig) => !Focused && orig();

    private static bool ModDashResult(Func<bool> orig) => !Focused && orig();
}
