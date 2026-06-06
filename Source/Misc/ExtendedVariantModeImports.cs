using MonoMod.ModInterop;

[ModImportName("ExtendedVariantMode")]
public static class ExtendedVariantModeImports {
    public delegate object GetCurrentVariantValueDelegate(string variantString);
    public static GetCurrentVariantValueDelegate GetCurrentVariantValue = null!;

    public delegate void TriggerVariantDelegate(string variantString, object newValue, bool revertOnDeath);
    public static TriggerVariantDelegate TriggerVariant = null!;

    public static bool IsLoaded => GetCurrentVariantValue is not null && TriggerVariant is not null;

    public static bool GetBoolVariant(string name) => IsLoaded && (bool) GetCurrentVariantValue(name);
    public static void SetBoolVariant(string name, bool value) { if (!IsLoaded) return; TriggerVariant(name, value, false); }
    public static int GetIntVariant(string name) => IsLoaded ? (int) GetCurrentVariantValue(name) : 0;
    public static void SetIntVariant(string name, int value) { if (!IsLoaded) return; TriggerVariant(name, value, false); }
    public static float GetFloatVariant(string name) => IsLoaded ? (float) GetCurrentVariantValue(name) : 0.0f;
    public static void SetFloatVariant(string name, float value) { if (!IsLoaded) return; TriggerVariant(name, value, false); }
}