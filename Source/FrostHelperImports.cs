using System;
using System.Diagnostics.CodeAnalysis;
using Celeste;
using MonoMod.ModInterop;

[ModImportName("FrostHelper")]
public static class FrostHelperImports
{
    public delegate bool TryCreateSessionExpressionDelegate(string str, [NotNullWhen(true)] out object? expression);
    public static TryCreateSessionExpressionDelegate TryCreateSessionExpression;

    public delegate int GetIntSessionExpressionValueDelegate(object expression, Session session);
    public static GetIntSessionExpressionValueDelegate GetIntSessionExpressionValue;

    public delegate float GetFloatSessionExpressionValueDelegate(object expression, Session session);
    public static GetFloatSessionExpressionValueDelegate GetFloatSessionExpressionValue;

    public delegate bool GetBoolSessionExpressionValueDelegate(object expression, Session session);
    public static GetBoolSessionExpressionValueDelegate GetBoolSessionExpressionValue;

    public delegate object GetSessionExpressionValueDelegate(object expression, Session session);
    public static GetSessionExpressionValueDelegate GetSessionExpressionValue;

    public static bool IsLoaded {
        get => TryCreateSessionExpression is not null
        && GetIntSessionExpressionValue is not null
        && GetFloatSessionExpressionValue is not null
        && GetBoolSessionExpressionValue is not null
        && GetSessionExpressionValue is not null;
    }
}
