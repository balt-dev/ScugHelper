
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/SeekablePlaybackWatchtower")]
public class SeekablePlaybackWatchtower : Lookout {
    public readonly float ScrollSpeed;
    public readonly bool KeepCameraInBounds;

    readonly Vector2 playbackOffset;
    readonly EntityData entityData;
    PlayerPlayback? playback;
    TimeRateModifier? timeMod;
    double playbackProgress;

    List<Vector2[]>? bakedHairNodes;
    static readonly Dictionary<string, List<Vector2[]>> bakedNodeCache = [];

    public SeekablePlaybackWatchtower(EntityData data, Vector2 offset) : base(data, offset) {
        ScrollSpeed = data.Float("ScrollSpeed", 1);
        KeepCameraInBounds = data.Bool("KeepCameraInBounds", true);
        playbackOffset = offset;
        onlyY = false;
        summit = false;
        nodes = [];
        entityData = data;
    }

    private List<Vector2[]> BakePlayerHair() {
        if (playback is null) return [];
        List<Vector2[]> bakedNodes = [];
        float realDeltaTime = Engine.DeltaTime;
        Engine.DeltaTime = 1f / 60f;
        try {
            for (int i = 0; i < playback.Timeline.Count; i++) {
                playback.SetFrame(i);
                playback.Hair.Update();
                playback.Hair.AfterUpdate();
                bakedNodes.Add(playback.Hair.Nodes.ToArray());
            }
        } finally {
            Engine.DeltaTime = realDeltaTime;
        }
        playback.SetFrame(0);
        return bakedNodes;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (scene is not Level level) { RemoveSelf(); return; }
        EntityData playbackData = new() {
            Position = entityData.FirstNodeNullable(Vector2.Zero) ?? (level.GetSpawnPoint(Position) - playbackOffset),
            Values = entityData.Values
        };
        playback = new(playbackData, playbackOffset) { Visible = false, Active = false, Depth = 1 };
        playback.Add(new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 32, 64));
        Add(timeMod = new TimeRateModifier(1f));
        if (!bakedNodeCache.TryGetValue(playbackData.Attr("tutorial"), out bakedHairNodes))
            bakedNodeCache.Add(playbackData.Attr("tutorial"), bakedHairNodes = BakePlayerHair());
        Components.RemoveAll<TalkComponent>();
        Add(talk = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(-0.5f, -20f), CustomInteract));
        scene.Add(playback);
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        scene.Remove(playback);
    }

    private void CustomInteract(Player player) {
        animPrefix =
            (player.DefaultSpriteMode == PlayerSpriteMode.MadelineAsBadeline || SaveData.Instance.Assists.PlayAsBadeline) ? "badeline_"
            : (player.DefaultSpriteMode == PlayerSpriteMode.MadelineNoBackpack) ? "nobackpack_"
            : "";

        Coroutine coroutine = new(CustomLookRoutine(player)) { RemoveOnComplete = true };
        Add(coroutine);
        interacting = true;
    }

    public IEnumerator CustomLookRoutine(Player player) {
        if (playback is null) yield break;
        if (timeMod is null) yield break;
        Level level = SceneAs<Level>();
        SandwichLava sandwichLava = Scene.Entities.FindFirst<SandwichLava>();
        sandwichLava?.Waiting = true;
        if (player.Holding != null) player.Drop();

        player.StateMachine.State = 11;
        yield return player.DummyWalkToExact((int)X, walkBackwards: false, 1f, cancelOnFall: true);
        if (Math.Abs(X - player.X) > 4f || player.Dead || !player.OnGround()) {
            if (!player.Dead)
                player.StateMachine.State = 0;

            yield break;
        }

        Audio.Play("event:/game/general/lookout_use", Position);
        sprite.Play(animPrefix + ((player.Facing == Facings.Right) ? "lookRight" : "lookLeft"));

        PlayerSprite playerSprite = player.Sprite;
        PlayerHair hair = player.Hair;
        bool visible = false;
        hair.Visible = false;
        playerSprite.Visible = visible;
        yield return 0.2f;
        Scene.Add(hud = []);
        hud.TrackMode = true;
        hud.OnlyY = onlyY;
        nodePercent = 0f;
        node = 0;
        Audio.Play("event:/ui/game/lookout_on");

        playbackProgress = 0f;
        Vector2 cameraStart = level.Camera.Position;
        new FadeWipe(level, wipeIn: false, () => {
            Vector2 cameraPos = playback.start - new Vector2(level.Camera.Right - level.Camera.Left, level.Camera.Bottom - level.Camera.Top) / 2 + level.CameraOffset;
            if (KeepCameraInBounds) {
                cameraPos.X = MathHelper.Clamp(cameraPos.X, level.Bounds.Left, level.Bounds.Right - (level.Camera.Right - level.Camera.Left));
                cameraPos.Y = MathHelper.Clamp(cameraPos.Y, level.Bounds.Top, level.Bounds.Bottom - (level.Camera.Bottom - level.Camera.Top));
            }
            level.Camera.Position = cameraPos;
            new FadeWipe(level, wipeIn: true).Duration = 0.2f;
        }).Duration = 0.2f;

        while ((hud.Easer = Calc.Approach(hud.Easer, 1f, Engine.DeltaTime * 3f)) < 1f) {
            level.ScreenPadding = (int)(Ease.CubeInOut(hud.Easer) * 16f);
            yield return null;
        }
        var camera = level.Camera;

        // We don't set active here
        playback.Visible = true;

        Vector2 lastDir = Vector2.Zero;
        Vector2 camStart = level.Camera.Position;
        while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && interacting) {
            playback.Depth = 1;
            Vector2 input = Input.Aim.Value;

            if (Math.Sign(input.X) != Math.Sign(lastDir.X) || Math.Sign(input.Y) != Math.Sign(lastDir.Y))
                Audio.Play("event:/game/general/lookout_move", Position);

            lastDir = input;
            playbackProgress += -input.Y * Engine.DeltaTime * ScrollSpeed / playback.Duration;
            timeMod.Multiplier = Math.Abs(input.Y);
            if (playbackProgress > 1 || playbackProgress < 0) timeMod.Multiplier = 0f;
            playbackProgress = Math.Clamp(playbackProgress, 0f, 1f);
            int frameIndex = (int)(playbackProgress * (playback.Timeline.Count - 1));
            playback.SetFrame(frameIndex);
            playback.LastPosition = playback.Position;
            foreach (var comp in playback.Components)
                comp.Update();
            playback.Hair.AfterUpdate();
            playback.Hair.Nodes = [..bakedHairNodes![frameIndex]]; // Copy

            hud.TrackPercent = (float) playbackProgress;

            Vector2 cameraPos = playback.Position - new Vector2(level.Camera.Right - level.Camera.Left, level.Camera.Bottom - level.Camera.Top) / 2 + level.CameraOffset;
            if (KeepCameraInBounds) {
                cameraPos.X = MathHelper.Clamp(cameraPos.X, level.Bounds.Left, level.Bounds.Right - (level.Camera.Right - level.Camera.Left));
                cameraPos.Y = MathHelper.Clamp(cameraPos.Y, level.Bounds.Top, level.Bounds.Bottom - (level.Camera.Bottom - level.Camera.Top));
            }
            level.Camera.Position = cameraPos;

            yield return null;
        }
        timeMod.Multiplier = 1f;

        PlayerSprite playerSprite2 = player.Sprite;
        PlayerHair hair2 = player.Hair;
        visible = true;
        hair2.Visible = true;
        playerSprite2.Visible = visible;
        sprite.Play(animPrefix + "idle");
        Audio.Play("event:/ui/game/lookout_off");
        new FadeWipe(level, wipeIn: false, () => {
            playback.Visible = false;
            camera.Position = cameraStart;
            new FadeWipe(level, wipeIn: true).Duration = 0.2f;
        }).Duration = 0.2f;
        while ((hud.Easer = Calc.Approach(hud.Easer, 0f, Engine.DeltaTime * 3f)) > 0f) {
            level.ScreenPadding = (int)(Ease.CubeInOut(hud.Easer) * 16f);
            yield return null;
        }
        Audio.SetMusicParam("escape", 0f);
        level.ScreenPadding = 0f;
        level.ZoomSnap(Vector2.Zero, 1f);
        Scene.Remove(hud);
        interacting = false;
        player.StateMachine.State = 0;
        yield return null;
    }

    [OnLoad]
    internal static void LoadHooks() {
        Everest.Events.AssetReload.OnReloadLevel += OnReloadLevel;
        Everest.Events.Level.OnExit += OnExit;
    }

    [OnUnload]
    internal static void UnloadHooks() {
        Everest.Events.AssetReload.OnReloadLevel -= OnReloadLevel;
        Everest.Events.Level.OnExit -= OnExit;
    }

    private static void OnReloadLevel(Level level) => bakedNodeCache.Clear();
    private static void OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) => bakedNodeCache.Clear();
}
