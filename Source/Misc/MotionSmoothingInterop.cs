using Monocle;
using MonoMod.ModInterop;

namespace Celeste.Mod.ScugHelper;

[ModImportName("MotionSmoothing")]
internal static class MotionSmoothingImportHandler {
    public delegate void DisableInterpolationDelegate(Entity entity);
    public static DisableInterpolationDelegate? DisableObjectSmoothing = null;

    public delegate void ReenableInterpolationDelegate(Entity entity);
    public static DisableInterpolationDelegate? ReenableObjectSmoothing = null;
}

public static class MotionSmoothingImports {
    public static void DisableInterpolation(this Entity self) =>
        MotionSmoothingImportHandler.DisableObjectSmoothing?.Invoke(self);
        
    public static void ReenableInterpolation(this Entity self) =>
        MotionSmoothingImportHandler.ReenableObjectSmoothing?.Invoke(self);
        
    public static bool IsLoaded
        => MotionSmoothingImportHandler.DisableObjectSmoothing is not null && MotionSmoothingImportHandler.ReenableObjectSmoothing is not null;
}
