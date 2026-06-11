using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Linq;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/RefillRectangle")]
public class RefillRectangle : Entity
{
    static readonly bool[][] ScanPatterns = [
        [true],
        [true, true],
        [true, false, true],
        [true, false, false, true],
    ];

    static bool IsFilled(int width, int i) {
        if (width <= ScanPatterns.Length)
            return ScanPatterns[width][i];
        if (i < 2 || (width - i - 1) < 2) return true;
        if (i == 2 || (width - i - 1) == 2) return false;
        float center = ((float)width) / 2;
        return (width % 6) switch
        {
            2 or 5 => i % 3 != 2,
            0 or 3 => i % 3 != 1,
            1 => (i < center) ? i % 3 != 2 : i % 3 != 1,
            4 => i != width / 2 && i != width / 2 - 1 && ((i < center) ? i % 3 != 2 : i % 3 != 1),
            _ => false,
        };
    }

    Refill? refill;
    public readonly Color OutlineColor;
    public readonly Color InfillColor;
    public readonly float InfillOpacity;
    public readonly int ID;
    private VirtualRenderTarget? bakedTexture;
    readonly string? FallbackRefillType;
    readonly bool FallbackRefillOneUse;

    public RefillRectangle(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        Tag |= Tags.TransitionUpdate;
        Depth = 5000;
        ID = id.ID;
        Collider = new Hitbox(data.Width, data.Height);
        OutlineColor = data.HexColor("OutlineColor", Calc.HexToColor("93bd40"));
        InfillColor = data.HexColor("InfillColor", Calc.HexToColor("208020"));
        FallbackRefillType = data.String("FallbackRefillType");
        FallbackRefillOneUse = data.Bool("FallbackRefillOneUse");
        InfillOpacity = data.Float("InfillOpacity", 0.8f);
        Add(new BeforeRenderHook(BakeTexture));
        Add(new CustomBloom(OnRenderBloom));
        Add(new PlayerCollider(OnPlayer));
    }

    internal void BakeTexture() {
        if (bakedTexture is null) {
            var oldTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(bakedTexture = VirtualContent.CreateRenderTarget($"outlinePrerender_{ID}", (int)Width, (int)Height));

            Draw.SpriteBatch.Begin();

            for (int x = 0; x <= Width; x++) {
                if (IsFilled((int)Width, x)) {
                    Draw.Pixel.Draw(new(x, 0));
                    Draw.Pixel.Draw(new(x, Height - 1));
                }
            }

            for (int y = 0; y <= Height; y++) {
                if (IsFilled((int)Height, y)) {
                    Draw.Pixel.Draw(new(0, y));
                    Draw.Pixel.Draw(new(Width - 1, y));
                }
            }

            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTargets(oldTargets);
        }
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        Refill? closestRefill = null;
        foreach (Entity entity in scene.Entities)
            if (entity is Refill refill && CollideCheck(refill) && (closestRefill is null || (closestRefill.Center - Center).LengthSquared() < (refill.Center - Center).LengthSquared()))
                closestRefill = refill;

        if (closestRefill is null) {
            switch (FallbackRefillType) {
                case "green":
                    scene.Add(closestRefill = new Refill(Position, false, FallbackRefillOneUse));
                    break;
                case "pink":
                    scene.Add(closestRefill = new Refill(Position, true, FallbackRefillOneUse));
                    break;
                case "blue":
                    scene.Add(closestRefill = new MidairRefill(Position, FallbackRefillOneUse));
                    break;
                case "black":
                    scene.Add(closestRefill = new LimboRefill(Position, FallbackRefillOneUse));
                    break;
                case "dark_green":
                    scene.Add(closestRefill = new SeekerRefill(Position, FallbackRefillOneUse));
                    break;
                case "cyan":
                    scene.Add(closestRefill = new OverchargeRefill(Position, FallbackRefillOneUse));
                    break;
                case "rose":
                    scene.Add(closestRefill = new HiccupRefill(Position, FallbackRefillOneUse));
                    break;
                case "gold":
                    scene.Add(closestRefill = new BoostRefill(Position, FallbackRefillOneUse, 1.75f));
                    break;
            }
            if (closestRefill is null) {
                Logger.Warn(nameof(ScugHelper), "No refill found! Deleting refill rectangle...");
                RemoveSelf();
                return;
            }
        }
        closestRefill.Position = Position + closestRefill.Center - closestRefill.Position;
        closestRefill.Collider = new Hitbox(0, 0);
        refill = closestRefill;
        refill.Add(new StaticRefillComponent(false, false));
    }

    public void OnPlayer(Player player) {
        if (refill is null) return;
        if (refill.respawnTimer > 0f) return;
        foreach (PlayerCollider collider in refill.Components.GetAll<PlayerCollider>().ToArray())
            collider.OnCollide(player);
        if (refill.Scene == null) RemoveSelf();
    }

    public override void Update() {
        base.Update();
        refill?.sine.counter = 0;
        refill?.Position = Center + refill.Center - refill.Position;
        refill?.Collidable = false;
    }

    public override void Render() {
        base.Render();
        if (refill is null) return;
        if (!(refill.sprite.Visible || refill.outline.Visible)) { return; }
        if (!refill.sprite.Visible) {
            if (bakedTexture is not null) Draw.SpriteBatch.Draw(bakedTexture, Position, Color.White);
        } else {
            Draw.HollowRect(Collider, OutlineColor);
            Draw.Rect(Left + 2, Top + 2, Width - 4, Height - 4, InfillColor * InfillOpacity);
        }
    }

    internal void OnRenderBloom() {
        if (refill is null) return;
        if (refill.sprite.Visible) {
            Draw.HollowRect(Collider, Color.White);
            Draw.Rect(Left + 2, Top + 2, Width - 4, Height - 4, Color.White * InfillOpacity);
        }
    }
    
    [Tracked]
    internal class StaticRefillComponent(bool active, bool visible) : Component(active, visible) {}

    [OnLoad] internal static void LoadHooks() => On.Celeste.Refill.UpdateY += OnUpdateY;
    [OnUnload] internal static void UnloadHooks() => On.Celeste.Refill.UpdateY -= OnUpdateY;

    private static void OnUpdateY(On.Celeste.Refill.orig_UpdateY orig, Refill self)
    {
        if (self.Get<StaticRefillComponent>() is not null) self.sprite.Y = self.flash.Y = self.outline.Y = self.light.Y = self.bloom.Y = 0;
        else orig(self);
    }
}

