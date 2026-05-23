using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/SeekerBarrierMaskRenderer")]
public class SeekerBarrierMaskRenderer : Entity
{

    [OnLoad]
    public static void LoadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread += OnLevelLoad;
    }
    [OnUnload]
    public static void UnloadHooks() {
        Everest.Events.LevelLoader.OnLoadingThread -= OnLevelLoad;
    }

    private static void OnLevelLoad(Level level) {
        level.Add(new SeekerBarrierMaskRenderer());
    }

    static readonly float FieldOpacity = 0.15f;

    readonly List<Entity> Entities = [];

    protected static readonly float[] speeds = [12f, 20f, 40f];
    private static readonly int BufferWidth = 512;
    private static readonly int BufferHeight = 512;
    private static readonly int ParticleWidth = 512;
    private static readonly int ParticleHeight = 512;
    protected static readonly Vector2[] particles = new Vector2[ParticleWidth * ParticleHeight / 16];

    static SeekerBarrierMaskRenderer() {
        for (int i = 0; i < particles.Length; i++)
            particles[i] = new Vector2(Calc.Random.NextFloat(ParticleWidth - 1f), Calc.Random.NextFloat(ParticleHeight - 1f));
    }

    public SeekerBarrierMaskRenderer() : base() {
        Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
        Depth = -8500;
        Add(new BeforeRenderHook(BeforeRender));
        Add(new CustomBloom(OnRenderBloom));
    }

    internal void Track(Entity ent) {
        if (Entities.Contains(ent)) return;
        ent.Visible = false;
        Entities.Add(ent);
    }

    internal void Untrack(Entity ent) {
        Entities.Remove(ent);
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        buffer?.Dispose();
    }

    Vector2 lastCamPosition;

    public override void Update() {
        base.Update();
        var newCamPosition = SceneAs<Level>().Camera.Position;
        var deltaCam = newCamPosition - lastCamPosition;
        int count = particles.Length;
        for (int i = 0; i < count; i++) {
            Vector2 value = particles[i] - deltaCam + Vector2.UnitY * speeds[i % speeds.Length] * Engine.DeltaTime;
            value.X = (value.X % ParticleWidth + ParticleWidth) % BufferWidth;
            value.Y = (value.Y % ParticleHeight + ParticleHeight) % ParticleHeight;
            particles[i] = value;
        }
        lastCamPosition = newCamPosition;
        foreach (Entity entity in Entities)
            entity.Visible = false;
    }

    VirtualRenderTarget buffer;
    Color[] pixelData;

    public void BeforeRender() {
        buffer ??= VirtualContent.CreateRenderTarget("seeker-barrier-mask-renderer", BufferWidth, BufferHeight);

        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);

        var cam = (Scene as Level).Camera;
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, RasterizerState.CullNone, null, cam.Matrix);

        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

        foreach (Entity entity in Entities) {
            entity.Visible = true;
            entity.Render();
            entity.Visible = false;
        }


        Draw.SpriteBatch.End();

        pixelData ??= new Color[buffer.Width * buffer.Height];
        buffer.Target.GetData(pixelData);
    }

    public bool CheckParticle(Vector2 pos) {
        int x = (int)pos.X;
        int y = (int)pos.Y;
        if (x < 0 || y < 0 || x >= buffer.Width || y >= buffer.Height) return false;
        return pixelData[y * buffer.Width + x].A > 0;
    }

    public override void Render() {
        base.Render();
        if (buffer is not null) {
            var cam = (Scene as Level).Camera;

            Draw.SpriteBatch.Draw(buffer.Target, cam.Position, null, Color.White * FieldOpacity, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
            int count = particles.Length;
            for (int i = 0; i < count; i++) {
                Vector2 part = particles[i];
                for (int x = 0; x < BufferWidth / ParticleWidth; x += ParticleWidth)
                    for (int y = 0; y < BufferHeight / ParticleHeight; y += ParticleHeight)
                        if (CheckParticle(part))
                            Draw.Pixel.Draw(part + cam.Position, Vector2.Zero, Color.White * 0.5f);
            }

        }
    }

    private void OnRenderBloom() {
        if (buffer is not null) {
            var cam = (Scene as Level).Camera;

            Draw.SpriteBatch.Draw(buffer.Target, cam.Position, null, Color.White, 0f, Vector2.Zero, 1f / cam.Zoom, SpriteEffects.None, 0f);
        }
    }
}
