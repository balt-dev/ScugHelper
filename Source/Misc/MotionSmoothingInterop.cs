using Monocle;
using MonoMod.ModInterop;

namespace Celeste.Mod.ScugHelper;

[ModImportName("MotionSmoothing")]
internal static class MotionSmoothingImportHandler {
    public delegate void DisableInterpolationDelegate(Entity entity);
    public static DisableInterpolationDelegate? DisableInterpolation = null;

    public delegate void ReenableInterpolationDelegate(Entity entity);
    public static DisableInterpolationDelegate? ReenableInterpolation = null;
}

public static class MotionSmoothingImports {
    public static void DisableInterpolation(this Entity self) =>
        MotionSmoothingImportHandler.DisableInterpolation?.Invoke(self);
        
    public static void ReenableInterpolation(this Entity self) =>
        MotionSmoothingImportHandler.ReenableInterpolation?.Invoke(self);
        
    public static bool IsLoaded
        => MotionSmoothingImportHandler.DisableInterpolation is not null && MotionSmoothingImportHandler.ReenableInterpolation is not null;
}
