using System;
using System.Diagnostics.CodeAnalysis;
using Celeste;
using Microsoft.Xna.Framework;
using MonoMod.ModInterop;

[ModImportName("GravityHelper")]
public static class GravityHelperImports
{
    public delegate bool IsActorInvertedDelegate(Actor actor);
    public static IsActorInvertedDelegate IsActorInverted;

    public delegate void SetPlayerGravityDelegate(int gravityType, float momentumMultiplier);
    public static SetPlayerGravityDelegate SetPlayerGravity;

    public delegate bool IsPlayerInvertedDelegate();
    public static IsPlayerInvertedDelegate IsPlayerInverted;

    public static bool IsInverted(this Actor self) => IsActorInverted is not null && IsActorInverted(self);
    public static bool PlayerInverted() => IsPlayerInverted is not null && IsPlayerInverted();
    public static void SetPlayerInverted(bool value) {
        if (SetPlayerGravity is not null) SetPlayerGravity(value ? 1 : 0, 1f);
    }
    public static Vector2 AdjustedSpeed(this Player self) => self.IsInverted() ? new Vector2(self.Speed.X, -self.Speed.Y) : self.Speed;
    public static void SetAdjustedSpeed(this Player self, Vector2 value) => self.Speed = self.IsInverted() ? new Vector2(value.X, -value.Y) : value;
    public static void SetAdjustedSpeed(this Player self, float x, float y) => self.Speed = self.IsInverted() ? new Vector2(x, -y) : new(x, y);


    public static bool IsLoaded {
        get => IsActorInverted is not null
        && SetPlayerGravity is not null
        && IsPlayerInverted is not null;
    }
}
