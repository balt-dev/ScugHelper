using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Celeste;
using Celeste.Mod.ScugHelper;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[RegisterStrawberry(false, true)]
[CustomEntity("ScugHelper/BrassBerry")]
class BrassBerry : Entity, IStrawberry
{
    public static ParticleType P_Glow = Strawberry.P_Glow;
    public static ParticleType P_GhostGlow = Strawberry.P_GhostGlow;

    public EntityID ID;
    public Follower Follower;

    private Sprite sprite;
    private Wiggler wiggler;
    private BloomPoint bloom;
    private VertexLight light;
    private Tween lightTween;
    private float wobble = 0f;
    private float collectTimer = 0f;
    private bool collected = false;
    private readonly bool isOwned;

    public BrassBerry(EntityData data, Vector2 offset, EntityID gid)
    {
        ID = gid;
        Position = data.Position + offset;

        isOwned = SaveData.Instance.CheckStrawberry(ID);
        Depth = -100;
        Collider = new Hitbox(14f, 14f, -7f, -7f);
        Add(new PlayerCollider(OnPlayer));
        Add(new MirrorReflection());
        Add(Follower = new Follower(ID, null, null));
        Follower.FollowDelay = 0.3f;
        
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (!(ScugHelperModule.Session.BrassBerryFollowing?.Equals(ID) ?? true)) {
            scene.Remove(this);
            return;
        }

        sprite = GFX.SpriteBank.Create("brassBerry");
        Add(sprite);

        sprite.Play(isOwned ? "idleGhost" : "idle");

        sprite.OnFrameChange = OnAnimate;

        wiggler = Wiggler.Create(0.4f, 4f, (v) => { sprite.Scale = Vector2.One * (1f + v * 0.35f); }, false, false);
        Add(wiggler);

        bloom = new BloomPoint(isOwned ? 0.5f : 1f, 12f);
        Add(bloom);

        light = new VertexLight(Color.White, 1f, 16, 24);
        lightTween = light.CreatePulseTween();
        Add(light);
        Add(lightTween);

        if (SceneAs<Level>().Session.BloomBaseAdd > 0.1f)
            bloom.Alpha *= 0.5f;
    }

    public override void Update() {
        if (!collected) {
            wobble += Engine.DeltaTime * 4f;
            sprite.Y = bloom.Y = light.Y = (float)Math.Sin(wobble) * 2f;

            if (Follower.Leader != null) {
                if (Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this)) {
                    if (
                        Follower.Leader.Entity is Player player && player.Scene != null &&
                        !player.StrawberriesBlocked &&
                        (player.CollideCheck<BrassBerryCollectTrigger>() || (Scene as Level).Completed)
                    ) {
    					collectTimer += Engine.DeltaTime;
    					if (collectTimer > 0.15f)
    						OnCollect();
                    }
                    else
                        collectTimer = Math.Min(collectTimer, 0f);
                }
                else if (Follower.FollowIndex > 0)
                    collectTimer = -0.15f;
            }
        }

        base.Update();
    }

    private void OnAnimate(string id)
    {
        int numFrames = 35;
        if (sprite.CurrentAnimationFrame == numFrames - 4)
        {
            lightTween.Start();

            bool visuallyObstructed = CollideCheck<FakeWall>() || CollideCheck<Solid>();
            Audio.Play("event:/game/general/strawberry_pulse", Position);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, (!collected && visuallyObstructed) ? 0.1f : 0.2f);
        }
    }

    public void OnPlayer(Player player)
    {
        if (Follower.Leader != null || collected)
            return;

        if (ScugHelperModule.Session.BrassBerryFollowing == null)
            Audio.Play(isOwned ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);

        player.Leader.GainFollower(Follower);
        wiggler.Start();
        Depth = -1000000;

        ScugHelperModule.Session.BrassBerryFollowing = ID;
    }

    public void OnCollect()
    {
        if (collected)
            return;
        ScugHelperModule.Session.BrassBerryFollowing = null;

        collected = true;

        int collectIndex = 0;

        if (Follower.Leader != null)
        {
            Player player = Follower.Leader.Entity as Player;
            collectIndex = player.StrawberryCollectIndex;
            player.StrawberryCollectIndex++;
            player.StrawberryCollectResetTimer = 2.5f;
            Follower.Leader.LoseFollower(Follower);
        }

        SaveData.Instance.AddStrawberry(ID, true);

        Session session = SceneAs<Level>().Session;
        session.DoNotLoad.Add(ID);
        session.Strawberries.Add(ID);
        session.UpdateLevelStartDashes();

        Add(new Coroutine(CollectRoutine(collectIndex), true));
    }

    private IEnumerator CollectRoutine(int collectIndex)
    {
        Tag = Tags.TransitionUpdate;
        Depth = -2000010;

        int color = !isOwned ? 0 : 1;
        Audio.Play("event:/game/general/strawberry_get", Position, "colour", color, "count", collectIndex);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        sprite.Play(isOwned ? "collectGhost" : "collect");
        while (sprite.Animating) yield return null;
        Scene.Add(new StrawberryPoints(Position, isOwned, collectIndex, false));
        RemoveSelf();
        yield break;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (ScugHelperModule.Session.BrassBerryFollowing != null)
        {
            Player player = scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                Logger.Warn(nameof(ScugHelperModule), "Could not find Player to attach to!");
                return;
            }
            OnPlayer(player);
        }
    }

    internal static void LoadHooks()
    {
        On.Celeste.Player.Added += PlayerAddHook;
        On.Celeste.Player.Update += PlayerUpdateHook;
    }

    internal static void UnloadHooks() {
        On.Celeste.Player.Added -= PlayerAddHook;
        On.Celeste.Player.Update -= PlayerUpdateHook;
    }
    
    private static void PlayerUpdateHook(On.Celeste.Player.orig_Update orig, Player self)
    {
        self.level.Session.SetFlag("HasBrassBerry", ScugHelperModule.Session.BrassBerryFollowing != null);
        orig(self);
    }

    private static void PlayerAddHook(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        orig(self, scene);
        var following = ScugHelperModule.Session.BrassBerryFollowing;
        if (following != null)
        {
            BrassBerry berry = scene.Tracker.GetEntity<BrassBerry>();
            if (berry == null)
            {
                Logger.Info(nameof(ScugHelperModule), "Readding brass berry!");
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
