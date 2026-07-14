using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Linq;
using System;
using System.Collections.Generic;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[TrackedAs(typeof(CrushBlock))]
[CustomEntity("ScugHelper/SeekerCrushBlock")]
public class SeekerCrushBlock : CrushBlock {
    readonly string SpritePath;
    readonly bool NoReturn;

    public SeekerCrushBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, Axes.Horizontal, data.Bool("chillout"))
    {
        fill = data.HexColor("FaceBackground", Calc.HexToColor("343e4e"));
        SpritePath = data.String("SpritePath", "objects/crushblock") + "/";
        NoReturn = data.Bool("NoReturn");
        Components.RemoveAll<Image>();
        OnDashCollide = NewOnDashCollide;
        face.RemoveSelf();
        AddImages();
    }

    private DashCollisionResults NewOnDashCollide(Player player, Vector2 direction) {
        if (player.Get<PlayerSeekerComponent>() is null) return DashCollisionResults.Ignore;
        return OnDashed(player, direction);
    }

    void AddImages() {
        List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(SpritePath + "block");
        MTexture idle;
        idle = atlasSubtextures[1];
        canMoveHorizontally = true;
        canMoveVertically = false;

        face = new Sprite(GFX.Game, SpritePath);
        
        // Transcribed from vanilla Sprites.xml
        face.AddLoop("idle", "idle_face", 0.08f);
        face.Add("hurt", "hurt", 0.08f, "idle", [3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
        face.Add("hit", "hit", 0.08f);
        face.AddLoop("left", "hit_left", 0.08f);
        face.AddLoop("right", "hit_right", 0.08f);
        face.AddLoop("up", "hit_up", 0.08f);
        face.AddLoop("down", "hit_down", 0.08f);
        face.CenterOrigin();

        Add(face);
        
        face.Position = new Vector2(Width, Height) / 2f;
        face.Play("idle");
        face.OnLastFrame = f => { if (f == "hit") face.Play(nextFaceDirection); };
        int middleWidth = (int)(Width / 8f) - 1;
        int middleHeight = (int)(Height / 8f) - 1;
        AddImage(idle, 0, 0, 0, 0, -1, -1);
        AddImage(idle, middleWidth, 0, 3, 0, 1, -1);
        AddImage(idle, 0, middleHeight, 0, 3, -1, 1);
        AddImage(idle, middleWidth, middleHeight, 3, 3, 1, 1);
        for (int i = 1; i < middleWidth; i++) {
            AddImage(idle, i, 0, Calc.Random.Choose(1, 2), 0, 0, -1);
            AddImage(idle, i, middleHeight, Calc.Random.Choose(1, 2), 3, 0, 1);
        }

        for (int num4 = 1; num4 < middleHeight; num4++) {
            AddImage(idle, 0, num4, 0, Calc.Random.Choose(1, 2), -1);
            AddImage(idle, middleWidth, num4, 3, Calc.Random.Choose(1, 2), 1);
        }
    }
    
    public new void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0) {
        MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8);
        Vector2 vector = new(x * 8, y * 8);
        if (borderX != 0) {
            Image image = new(subtexture) {
                Color = Color.Black,
                Position = vector + new Vector2(borderX, 0f)
            };
            Add(image);
        }

        if (borderY != 0) {
            Image image2 = new(subtexture) {
                Color = Color.Black,
                Position = vector + new Vector2(0f, borderY)
            };
            Add(image2);
        }

        Image image3 = new(subtexture) { Position = vector };
        Add(image3);
        idleImages.Add(image3);
        if (borderX != 0 || borderY != 0) {
            if (borderX < 0) {
                Image image4 = new(GFX.Game[SpritePath + "lit_left"].GetSubtexture(0, ty * 8, 8, 8)) {
                    Position = vector,
                    Visible = false
                };
                activeLeftImages.Add(image4);
                Add(image4);
            } else if (borderX > 0) {
                Image image5 = new(GFX.Game[SpritePath + "lit_right"].GetSubtexture(0, ty * 8, 8, 8)) {
                    Position = vector,
                    Visible = false
                };
                activeRightImages.Add(image5);
                Add(image5);
            }

            if (borderY < 0) {
                Image image6 = new(GFX.Game[SpritePath + "lit_top"].GetSubtexture(tx * 8, 0, 8, 8)) {
                    Position = vector,
                    Visible = false
                };
                activeTopImages.Add(image6);
                Add(image6);
            } else if (borderY > 0) {
                Image image7 = new(GFX.Game[SpritePath + "lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8)) {
                    Position = vector,
                    Visible = false
                };
                activeBottomImages.Add(image7);
                Add(image7);
            }
        }
    }
    
    public override void Update() {
        foreach (Entity barrier in Scene.Tracker.GetEntities<SeekerBarrier>()) barrier.Collidable = true;
        base.Update();
        foreach (Entity barrier in Scene.Tracker.GetEntities<SeekerBarrier>()) barrier.Collidable = false;
        if (NoReturn) { returnLoopSfx.Stop(); returnStack.Clear(); }
    }

    [OnLoad] internal static void LoadHooks() => On.Celeste.Seeker.SlammedIntoWall += OnSlammedIntoWall;

    [OnUnload] internal static void UnloadHooks() => On.Celeste.Seeker.SlammedIntoWall -= OnSlammedIntoWall;

    private static void OnSlammedIntoWall(On.Celeste.Seeker.orig_SlammedIntoWall orig, Seeker self, CollisionData data) {
        if (data.Hit is SeekerCrushBlock scb && scb.CanActivate(-data.Direction)) {
            scb.Attack(-data.Direction);
        }
        orig(self, data);
    }

    internal void OnBonk(Vector2 direction, Vector2 delta, Platform hit) {
        if (hit is SeekerBarrier bar) {
            bar.OnReflectSeeker();
            Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
        }
    }
}