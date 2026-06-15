using Celeste.Mod.Entities;
using Monocle;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using FMOD.Studio;
using System;

namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/RaycastedAudioController")]
public class RaycastedAudioController(EntityData data, Vector2 _) : Entity() {
    public readonly float MaxDistance = data.Float("MaxDistance", 256);
    public readonly float AirFalloff = data.Float("AirFalloff", 1f);
    public readonly float SolidFalloff = data.Float("SolidFalloff", 0.92f);

    [OnLoad]
    internal static void LoadHooks() {
        On.Celeste.Audio.Play_string_Vector2 += OnAudioPlay;
        On.Celeste.Audio.Play_string_Vector2_string_float += OnAudioPlay;
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float += OnAudioPlay;
        On.Celeste.Audio.Loop_string_Vector2 += OnAudioLoop;
        On.Celeste.Audio.Loop_string_Vector2_string_float += OnAudioLoop;
    }
    [OnUnload]
    internal static void UnloadHooks() {
        On.Celeste.Audio.Play_string_Vector2 -= OnAudioPlay;
        On.Celeste.Audio.Play_string_Vector2_string_float -= OnAudioPlay;
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float -= OnAudioPlay;
        On.Celeste.Audio.Loop_string_string_float -= OnAudioLoop;
        On.Celeste.Audio.Loop_string_Vector2 -= OnAudioLoop;
        On.Celeste.Audio.Loop_string_Vector2_string_float -= OnAudioLoop;
    }
    
    private static EventInstance OnAudioPlay(On.Celeste.Audio.orig_Play_string_Vector2 orig, string path, Vector2 position) {
        var res = orig(path, position);
        AddInstanceMutator(res, position);
        return res;
    }

    private static EventInstance OnAudioPlay(On.Celeste.Audio.orig_Play_string_Vector2_string_float orig, string path, Vector2 position, string param, float value) {
        var res = orig(path, position, param, value);
        AddInstanceMutator(res, position);
        return res;
    }

    private static EventInstance OnAudioPlay(On.Celeste.Audio.orig_Play_string_Vector2_string_float_string_float orig, string path, Vector2 position, string param, float value, string param2, float value2) {
        var res = orig(path, position, param, value, param2, value2);
        AddInstanceMutator(res, position);
        return res;
    }
    
    private static EventInstance OnAudioLoop(On.Celeste.Audio.orig_Loop_string_string_float orig, string path, string param, float value) {
        var res = orig(path, param, value);
        AddInstanceMutator(res);
        return res;
    }
    
    private static EventInstance OnAudioLoop(On.Celeste.Audio.orig_Loop_string_Vector2 orig, string path, Vector2 position) {
        var res = orig(path, position);
        AddInstanceMutator(res, position);
        return res;
    }

    private static EventInstance OnAudioLoop(On.Celeste.Audio.orig_Loop_string_Vector2_string_float orig, string path, Vector2 position, string param, float value) {
        var res = orig(path, position, param, value);
        AddInstanceMutator(res, position);
        return res;
    }

    private static void AddInstanceMutator(EventInstance inst, Vector2? maybePos = null) {
        if (Engine.Scene is not Level level) return;
        if (level.Tracker.GetEntity<RaycastedAudioController>() is not {} cont) return;
        if (level.Tracker.GetEntity<Player>() is not {} player) return;
        if (inst is null) return;
        if (inst.getVolume(out float initialVolume, out var _) != FMOD.RESULT.OK) return;
        Vector2 position = maybePos ?? player.Position;

        FMOD.RESULT res;
        float volumeAmplifier = cont.GetRaycastedVolumeTo(position, player.Position + player.Collider.TopCenter);
        if ((res = inst.setVolume(initialVolume * volumeAmplifier)) != FMOD.RESULT.OK) {
            if (res != FMOD.RESULT.ERR_INVALID_HANDLE)
                Logger.Warn(nameof(ScugHelper), $"Failed to set volume of FMOD event instance! Removing RaycastedAudioComponent... ({res})");
            return;
        }

        player.Scene.OnEndOfFrame += () =>
            player.Add(new RaycastedAudioComponent(cont, inst, position, initialVolume));
    }

    private class RaycastedAudioComponent(RaycastedAudioController cont, EventInstance inst, Vector2 initialPosition, float initialVolume) : Component(true, false) {
        public override void Update() {
            FMOD.RESULT res;
            if ((res = inst.getPlaybackState(out var playbackState)) != FMOD.RESULT.OK) {
                if (res != FMOD.RESULT.ERR_INVALID_HANDLE)
                    Logger.Warn(nameof(ScugHelper), $"Failed to get playback state of FMOD event instance! Removing RaycastedAudioComponent... ({res})");
                RemoveSelf(); return;
            }
            if (playbackState == PLAYBACK_STATE.STOPPED) { RemoveSelf(); return; }
            if (cont.Scene is null) { RemoveSelf(); return; }
            Vector2 position = initialPosition;
            //if (inst.get3DAttributes(out var attrs) == FMOD.RESULT.OK)
            //    position += new Vector2(attrs.position.x, attrs.position.y);
            float volumeAmplifier = cont.GetRaycastedVolumeTo(position, Entity.Position + Entity.Collider.TopCenter);
            if ((res = inst.setVolume(initialVolume * volumeAmplifier)) != FMOD.RESULT.OK) {
                Logger.Warn(nameof(ScugHelper), $"Failed to set volume of FMOD event instance! Removing RaycastedAudioComponent... ({res})");
                RemoveSelf(); return;
            }
        }
    }

    private const float VolumeCutoff = 0.02f;

    private float GetRaycastedVolumeTo(Vector2 source, Vector2 destination) {
        float finalVolumeMultiplier = 1f;
        Vector2 delta = destination - source;
        Vector2 deltaDir = delta.SafeNormalize();
        if (delta.Length() < 1f) return 1f;

        int collidedSolids = 0;
        for (int i = 1; i < Math.Min(delta.Length(), MaxDistance); i++)
        {
            if (finalVolumeMultiplier < VolumeCutoff) return 0f;
            bool solid = Scene.CollideCheck<Solid>(source + deltaDir * i);
            finalVolumeMultiplier *= solid ? SolidFalloff : AirFalloff;
            if (solid) collidedSolids++;
        }

        return finalVolumeMultiplier;
    }
}
