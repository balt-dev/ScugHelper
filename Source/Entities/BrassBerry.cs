using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Celeste;
using Celeste.Mod.ScugHelper;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using MonoMod.Utils;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[RegisterStrawberry(false, true)]
[CustomEntity("ScugHelper/BrassBerry")]
class BrassBerry : Strawberry, IStrawberry {
    private readonly bool isOwned;

    public BrassBerry(EntityData data, Vector2 offset, EntityID gid): base(data, offset, gid) {
        ID = gid;
        Position = data.Position + offset;

        Golden = false;
        isOwned = SaveData.Instance.CheckStrawberry(ID);
        Depth = -100;
        Collider = new Hitbox(14f, 14f, -7f, -7f);
        Follower.FollowDelay = 0.3f;

    }

    public override void Added(Scene scene) {
        base.Added(scene);
        Visible = true;
        Collidable = true;
        Remove(sprite);
        if (!(ScugHelperModule.Session.BrassBerryFollowing?.Equals(ID) ?? true)) {
            scene.Remove(this);
            return;
        }

        sprite = GFX.SpriteBank.Create("brassBerry");
        Add(sprite);

        sprite.Play(isOwned ? "idleGhost" : "idle");
    }

    public override void Update() {
        Visible = true;
        Collidable = true;
        if (!collected) {
            wobble += Engine.DeltaTime * 4f;
            sprite?.Y = bloom!.Y = light!.Y = (float)Math.Sin(wobble) * 2f;

            if (Follower.Leader != null) {
                if (Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this)) {
                    if (
                        Follower.Leader.Entity is Player player && player.Scene != null &&
                        !player.StrawberriesBlocked &&
                        (player.CollideCheck<BrassBerryCollectTrigger>() || (Scene as Level)!.Completed)
                    ) {
    					collectTimer += Engine.DeltaTime;
    					if (collectTimer > 0.15f)
    						OnCollect();
                    } else
                        collectTimer = Math.Min(collectTimer, 0f);
                } else if (Follower.FollowIndex > 0)
                    collectTimer = -0.15f;
            }
        }

        foreach (var comp in Components.ToArray())
            comp.Update();
    }

    public void NewOnPlayer(Player player) {
        if (Follower.Leader != null || collected)
            return;

        if (ScugHelperModule.Session.BrassBerryFollowing == null)
            Audio.Play(isOwned ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);

        player.Leader.GainFollower(Follower);
        wiggler?.Start();
        Depth = -1000000;

        ScugHelperModule.Session.BrassBerryFollowing = ID;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (ScugHelperModule.Session.BrassBerryFollowing != null) {
            Player player = scene.Tracker.GetEntity<Player>();
            if (player == null) {
                Logger.Warn(nameof(ScugHelper), "Could not find Player to attach to!");
                return;
            }
            OnPlayer(player);
        }
    }

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Player.Added += PlayerAddHook;
        On.Celeste.Player.Update += PlayerUpdateHook;
        On.Celeste.Strawberry.OnPlayer += OnStrawberryOnPlayer;
        On.Celeste.Strawberry.OnCollect += OnStrawberryOnCollect;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Player.Added -= PlayerAddHook;
        On.Celeste.Player.Update -= PlayerUpdateHook;
        On.Celeste.Strawberry.OnPlayer -= OnStrawberryOnPlayer;
        On.Celeste.Strawberry.OnCollect -= OnStrawberryOnCollect;
    }

    private static void OnStrawberryOnCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self) {
        if (self is not BrassBerry) { orig(self); return; }
        if (self.collected)
            return;
        ScugHelperModule.Session.BrassBerryFollowing = null;

        self.collected = true;

        int collectIndex = 0;

        if (self.Follower.Leader != null) {
            Player player = (self.Follower.Leader.Entity as Player)!;
            collectIndex = player.StrawberryCollectIndex;
            player.StrawberryCollectIndex++;
            player.StrawberryCollectResetTimer = 2.5f;
            self.Follower.Leader.LoseFollower(self.Follower);
        }

        SaveData.Instance.AddStrawberry(self.ID, true);

        Session session = self.SceneAs<Level>().Session;
        session.DoNotLoad.Add(self.ID);
        session.Strawberries.Add(self.ID);
        session.UpdateLevelStartDashes();

        self.Add(new Coroutine(self.CollectRoutine(collectIndex), true));
    }

    private static void OnStrawberryOnPlayer(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player) {
        if (self is not BrassBerry brass) { orig(self, player); return; }
        brass.NewOnPlayer(player);
    }
    
    private static void PlayerUpdateHook(On.Celeste.Player.orig_Update orig, Player self) {
        // Ideally this should be an SSV but it was added way way way before that so
        self.level.Session.SetFlag("ScugHelper.HasBrassBerry", ScugHelperModule.Session.BrassBerryFollowing != null);
        self.level.Session.SetFlag("HasBrassBerry", ScugHelperModule.Session.BrassBerryFollowing != null);
        orig(self);
    }

    private static void PlayerAddHook(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
        orig(self, scene);
        var following = ScugHelperModule.Session.BrassBerryFollowing;
        if (following is null) {
            ScugHelperModule.Session.BrassBerryFollowing = following = (EntityID?) DynamicData.For(self.level.Session).Get("BrassBerryCrossLevel");
        }
        if (following != null) {
            BrassBerry berry = scene.Tracker.GetEntity<BrassBerry>();
            if (berry == null) {
                Logger.Log(nameof(ScugHelper), "Readding brass berry!");
                var followID = (EntityID)following;
                var data = new EntityData
                {
                    ID = followID.ID,
                    Position = self.Position,
                    Level = self.SceneAs<Level>().Session.LevelData,
                    Name = "ScugHelper/BrassBerry"
                };

                scene.Add(new BrassBerry(data, Vector2.Zero, followID));
                return;
            }
        }
    }
}
