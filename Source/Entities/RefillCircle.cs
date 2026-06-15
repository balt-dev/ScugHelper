using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/RefillCircle")]
public class RefillCircle : Entity
{
    Refill? refill;
    public readonly Color OutlineColor;
    public readonly Color InfillColor;
    public readonly float InfillOpacity;
    public readonly int ID;
    readonly string? FallbackRefillType;
    readonly bool FallbackRefillOneUse;
    public readonly float Radius;
    private readonly VertexLight? Light;
    private VirtualRenderTarget? bakedTexture;

    public RefillCircle(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
        Tag |= Tags.TransitionUpdate;
        Depth = 5000;
        ID = id.ID;
        Collider = new Circle(Radius = data.Float("Radius", 16f), Radius, Radius);
        OutlineColor = data.HexColor("OutlineColor", Calc.HexToColor("93bd40"));
        InfillColor = data.HexColor("InfillColor", Calc.HexToColor("208020"));
        FallbackRefillType = data.String("FallbackRefillType");
        FallbackRefillOneUse = data.Bool("FallbackRefillOneUse");
        InfillOpacity = data.Float("InfillOpacity", 0.8f);
        //Add(new BeforeRenderHook(BakeTexture));
        Add(new CustomBloom(OnRenderBloom));
        Add(new PlayerCollider(OnPlayer));
        Add(Light = new VertexLight(Vector2.One * Radius, Color.White, 0.5f, (int) Radius, (int) Radius + 32));
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
                    scene.Add(closestRefill = new BoostRefill(Position, FallbackRefillOneUse, true, 1.75f));
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
        refill.Add(new RefillRectangle.StaticRefillComponent(false, false));
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
        refill?.Position = Center + refill.Center - refill.Position;
        refill?.light.Position = Center + refill.Center - refill.Position;
        refill?.Collidable = false;
        Light?.Alpha = (refill?.sprite.Visible ?? false) ? 1.0f : 0.0f;
    }

    internal void BakeTexture() {
        if (bakedTexture is null) {
            var oldTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(bakedTexture = VirtualContent.CreateRenderTarget($"refillCirclePrerender_{ID}", (int)Radius * 2, (int)Radius * 2));

            DrawFilledCircle(Position + Vector2.One * (Radius - 2), Radius - 2, Color.White, 32);

            Engine.Graphics.GraphicsDevice.SetRenderTargets(oldTargets);
        }
    }

    public override void Render() {
        base.Render();
        if (refill is null) return;
        refill.sprite.Y = refill.flash.Y = refill.outline.Y = 0;
        if (!(refill.sprite.Visible || refill.outline.Visible)) { return; }
        if (!refill.sprite.Visible) {
            int res = (int) (Math.Clamp(Radius / 4, 1, 8) * 4);
            for (int i = 0; i < res; i += 2) {
                var point = Position + Vector2.One * Radius + Calc.AngleToVector(i * 2f * MathF.PI / res, Radius);
                var nextPoint = Position + Vector2.One * Radius + Calc.AngleToVector((i + 1) * 2f * MathF.PI / res, Radius);
                Draw.Line(point, nextPoint, Color.White);
            }
        } else {
            Draw.Circle(Position + Vector2.One * Radius, Radius, OutlineColor, 32);
            
            GameplayRenderer.End();
            
            DrawFilledCircle(Position + Vector2.One * 2, Radius - 2, InfillColor * InfillOpacity, 32);
            
            GameplayRenderer.Begin();
        }
    }

    internal void DrawFilledCircle(Vector2 position, float radius, Color color, int resolution) {
        List<VertexPositionColor> points = [];
        Vector2 center = position + Vector2.One * radius;
        points.Add(new(new(center.X, center.Y, 0), color));

        for (int i = 0; i <= resolution; i++) {
            var angle = Calc.AngleToVector(i * 2f * MathF.PI / resolution, radius) + center;
            points.Add(new(new(angle.X, angle.Y, 0), color));
        }
        
        var matrix = (Scene as Level)!.Camera?.Matrix ?? Matrix.Identity;
        
        List<int> indices = [];
        for (int i = 1; i <= resolution; i++) {
            indices.Add(0);
            indices.Add(i);
            indices.Add((short)(i + 1));
        }

        GFX.DrawIndexedVertices(matrix, points.ToArray(), points.Count, indices.ToArray(), resolution);
    }

    internal void OnRenderBloom() {
        if (refill is null) return;
        if (refill.sprite.Visible) {
            if (bakedTexture is not null) Draw.SpriteBatch.Draw(bakedTexture, Position, Color.White * InfillOpacity);
            Draw.Circle(Position + Vector2.One * Radius, Radius, Color.White, 32);
        }
    }
}
