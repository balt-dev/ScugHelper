using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/PhysicalSand")]
public class PhysicalSand : Solid {
    internal readonly int ParticleWidth;
    internal readonly int ParticleHeight;
    internal readonly Color Tint;
    internal readonly Color[] ParticleFrontbuffer;
    internal readonly Color[] ParticleBackbuffer;
    internal readonly bool[,] CollisionMask;
    internal readonly float CollisionUpdateFrequency;
    internal readonly float UpdateFrequency;
    internal readonly int RandomSeed;
    internal readonly string? IfFlag;
    internal readonly bool FlagState;
    internal readonly bool WrapX;
    internal readonly bool WrapY;
    internal readonly int Range;
    internal readonly Grid Grid;
    internal readonly Texture2D RenderTarget;
    private readonly Random RNG;

    public PhysicalSand(EntityData data, Vector2 offset): base(data.Position + offset, data.Width, data.Height, false) {
        ParticleFrontbuffer = new Color[(ParticleWidth = data.Width) * (ParticleHeight = data.Height)];
        ParticleBackbuffer = new Color[ParticleWidth * ParticleHeight];
        Collider = Grid = new Grid(ParticleWidth, ParticleHeight, 1, 1);
        CollisionMask = new bool[data.Width, data.Height];
        CollisionUpdateFrequency = data.Float("CollisionUpdateFrequency", 0f);
        UpdateFrequency = data.Float("UpdateFrequency", 0.05f);
        RandomSeed = data.Int("RandomSeed", 0);
        Tint = data.HexColor("Tint", Color.White);
        WrapX = data.Bool("WrapX", false);
        WrapY = data.Bool("WrapY", false);
        IfFlag = data.String("IfFlag");
        FlagState = data.Bool("FlagState", true);
        Range = data.Int("Range", 1);
        int imageOffsetX = data.Int("ImageOffsetX", 0);
        int imageOffsetY = data.Int("ImageOffsetY", 0);
        Collidable = data.Bool("Collidable");
        Depth = Collidable ? -9750 : 1750;
        RenderTarget = new Texture2D(Engine.Graphics.GraphicsDevice, ParticleWidth, ParticleHeight);

        RNG = new Random(RandomSeed);

        var mtex = GFX.Game[data.String("InitialStateImage", "")];
        var tex = mtex.GetSubtextureCopy();
        Color[] pixelData = new Color[tex.Width * tex.Height];
        tex.GetData(pixelData);
        for (int y = 0; y < Math.Min(ParticleHeight, tex.Height); y++)
            for (int x = 0; x < Math.Min(ParticleWidth, tex.Width); x++) {
                int dx = x + imageOffsetX + (int) mtex.DrawOffset.X;
                int dy = y + imageOffsetY + (int) mtex.DrawOffset.Y;
                if (dx < 0 || dx >= ParticleWidth || dy < 0 || dy >= ParticleHeight) continue;
                ParticleBackbuffer[dy * ParticleWidth + dx] = pixelData[y * tex.Width + x].Mul(Tint);
            }
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        BakeParticleCollision();
        UpdateParticles();
    }

    bool suffocating;
    float suffocateTimer;

    public override void Update() {
        Speed = Vector2.Zero;
        base.Update();
        if (IfFlag is not null && SceneAs<Level>().Session.GetFlag(IfFlag) != FlagState) return;
        if (CollisionUpdateFrequency > 0 && Scene.OnInterval(CollisionUpdateFrequency)) BakeParticleCollision();
        if (UpdateFrequency > 0 && Scene.OnInterval(UpdateFrequency)) UpdateParticles();
        if (CollideFirst<Player>() is Player player) {
            if (suffocateTimer > 2f)
                player.Die(Vector2.Zero);
            var sol = new Solid(Vector2.Zero, 0, 0, false);
            if (!player.TrySquishWiggle(new CollisionData() { Hit = sol, Pusher = sol, TargetPosition = player.Position }, 0, 10))
                suffocating = true;
        } else {
            suffocating = false;
        }
        if (suffocating)
            suffocateTimer += Engine.DeltaTime;
        else
            suffocateTimer = 0f;
    }

    public override void Render() {
        base.Render();
        Draw.SpriteBatch.Draw(RenderTarget, Position.Rounded(), Color.White);
    }

    public override void DebugRender(Camera camera) {
        base.DebugRender(camera);
        Draw.HollowRect(X - 1, Y - 1, ParticleWidth + 2, ParticleHeight + 2, Color.Cyan);
        for (int y = 0; y < ParticleHeight; y++)
            for (int x = 0; x < ParticleWidth; x++) {
                if (Grid[x, y])
                    Draw.Pixel.Draw(new(X + x, Y + y), Vector2.Zero, Color.Green);
                if (CollisionMask[x, y])
                    Draw.Pixel.Draw(new(X + x, Y + y), Vector2.Zero, Color.White);
            }
    }

    private void BakeParticleCollision() {
        bool oldCollidable = Collidable;
        Collidable = false;
        for (int y = 0; y < ParticleHeight; y++)
            for (int x = 0; x < ParticleWidth; x++)
                CollisionMask[x, y] = Scene.CollideCheck<Solid>(new Vector2(X + x, Y + y));
        Collidable = oldCollidable;
    }

    private bool Blocked(int x, int y)
        => (!WrapX && (x < 0 || x >= ParticleWidth)) || (!WrapY && (y < 0 || y >= ParticleHeight))
            || CollisionMask[Mod(x, ParticleWidth), Mod(y, ParticleHeight)] || ParticleBackbuffer[Mod(y, ParticleHeight) * ParticleWidth + Mod(x, ParticleWidth)].A != 0;

    private void UpdateParticles() {
        Array.Copy(ParticleBackbuffer, ParticleFrontbuffer, ParticleBackbuffer.Length);
        RenderTarget.SetData(ParticleFrontbuffer);
        if (Collidable) UpdateCollisionGrid();
        for (int y = ParticleHeight - 1; y >= 0; y--)
            for (int x = 0; x < ParticleWidth; x++) {
                bool bias = RNG.NextSingle() >= 0.5f;
                Color color = ParticleAt(x, y);
                if (color.A == 0) continue;
                for (int j = Range; j >= 1; j--) {
                    if (!Blocked(x, y + j)) {
                        SwapParticles(x, y, x, y + j);
                        continue;
                    }
                    int dir = bias ? -1 : 1;
                    for (int i = 1; i <= Range; i++) {
                        if (!Blocked(x + dir * i, y + j)) {
                            SwapParticles(x, y, x + dir * i, y + j);
                            break;
                        }
                        if (!Blocked(x - dir * i, y + j)) {
                            SwapParticles(x, y, x - dir * i, y + j);
                            break;
                        }
                    }
                }
            }
    }

    private int Mod(int val, int div)
        => ((val % div) + div) % div;

    private void SwapParticles(int x1, int y1, int x2, int y2) {
        (x1, x2) = (Mod(x1, ParticleWidth), Mod(x2, ParticleWidth));
        (y1, y2) = (Mod(y1, ParticleHeight), Mod(y2, ParticleHeight));
        (ParticleBackbuffer[y1 * ParticleWidth + x1], ParticleBackbuffer[y2 * ParticleWidth + x2])
        = (ParticleBackbuffer[y2 * ParticleWidth + x2], ParticleBackbuffer[y1 * ParticleWidth + x1]);
    }

    private Color ParticleAt(int x, int y) => ParticleFrontbuffer[Mod(y, ParticleHeight) * ParticleWidth + Mod(x, ParticleWidth)];

    private void UpdateCollisionGrid() {
        // TODO: Speed this up
        for (int y = 0; y < ParticleHeight; y++)
            for (int x = 0; x < ParticleWidth; x++)
                Grid[x, y] = ParticleAt(x, y).A != 0;
    }

    [OnLoad] internal static void LoadHooks() => On.Celeste.Player.OnCollideH += OnPlayerCollideH;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.Player.OnCollideH -= OnPlayerCollideH;

    private static void OnPlayerCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
        Vector2 wantDir = new(Math.Sign(self.Speed.X), -1);
        Vector2 wantPos = self.Position + wantDir;
        if (!self.CollideCheck<PhysicalSand>(wantPos) && self.CollideCheck<PhysicalSand>(wantPos + Vector2.UnitY)) {
            self.MoveVExact(-1);
            self.MoveHExact(Math.Sign(self.Speed.X));
            return;
        }
        orig(self, data);
    }
}
