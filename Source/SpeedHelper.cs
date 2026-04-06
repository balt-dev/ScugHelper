using Microsoft.Xna.Framework;
using Celeste;
using Monocle;
using System.Collections.Generic;
using System.Reflection;
using System;
using Celeste.Mod.ScugHelper;
using Celeste.Mod;

#nullable enable

public static class SpeedHelper
{
    private static readonly Dictionary<Type, ReflectionSpeedAccessorFactory?> SpeedAccessorCache = [];
    public static SpeedAccessor? GetSpeedOfEntity(Entity obj)
    {
        switch (obj)
        {
            case Player player: return new SpeedAccessor(player);
            case Solid solid: return new SpeedAccessor(solid);
            case PlayerSeeker seeker: return new SpeedAccessor(seeker);
            case Seeker seeker: return new SpeedAccessor(seeker);
            case TheoCrystal crystal: return new SpeedAccessor(crystal);
            case Puffer puffer: return new SpeedAccessor(puffer);
            default:
                var holdable = obj.Components.Get<Holdable>();
                if (holdable != null && holdable.SpeedGetter != null && holdable.SpeedSetter != null)
                    return new SpeedAccessor(holdable);
                break;
        }

        Type type = obj.GetType();
        if (SpeedAccessorCache.TryGetValue(type, out ReflectionSpeedAccessorFactory? value))
            return value?.ForEntity(obj);

        Logger.Info(nameof(ScugHelperModule), $"Creating new reflection speed accessor for entity type {type}!");

        PropertyInfo? speedProperty
            = type.GetProperty("Speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetProperty("speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetProperty("hitSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (speedProperty != null && speedProperty.PropertyType == typeof(Vector2))
        {
            var accessor = new ReflectionSpeedPropertyAccessorFactory(speedProperty);
            SpeedAccessorCache.Add(type, accessor);
            return accessor.ForEntity(obj);
        }

        FieldInfo? speedField
            = type.GetField("Speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetField("speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetField("hitSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (speedField != null && speedField.FieldType == typeof(Vector2))
        {
            var accessor = new ReflectionSpeedFieldAccessorFactory(speedField);
            SpeedAccessorCache.Add(type, accessor);
            return accessor.ForEntity(obj);
        }
        Logger.Warn(nameof(ScugHelperModule), $"Could not find speed for entity type {type}!");

        SpeedAccessorCache.Add(type, null);
        return null;
    }
}

public readonly struct SpeedAccessor
{
    private readonly Func<object, Vector2> Getter;
    private readonly Action<object, Vector2> Setter;
    private readonly object Object;
    public readonly Vector2 Speed { get => Getter(Object); set => Setter(Object, value); }
    
    internal SpeedAccessor(Func<object, Vector2> getter, Action<object, Vector2> setter, object obj) {
        Getter = getter; Setter = setter; Object = obj;
    }

    public SpeedAccessor(Player o) : this(static (o) => ((Player)o).Speed, static (o, value) => ((Player)o).Speed = value, o) { }
    public SpeedAccessor(Solid o) : this(static (o) => ((Solid)o).Speed, static (o, value) => ((Solid)o).Speed = value, o) { }
    public SpeedAccessor(Puffer o) : this(static (o) => ((Puffer)o).hitSpeed, static (o, value) => ((Puffer)o).hitSpeed = value, o) { }
    public SpeedAccessor(Seeker o) : this(static (o) => ((Seeker)o).Speed, static (o, value) => ((Seeker)o).Speed = value, o) { }
    public SpeedAccessor(PlayerSeeker o) : this(static (o) => ((PlayerSeeker)o).speed, static (o, value) => ((PlayerSeeker)o).speed = value, o) { }
    public SpeedAccessor(TheoCrystal o) : this(static (o) => ((TheoCrystal)o).Speed, static (o, value) => ((TheoCrystal)o).Speed = value, o) { }
    public SpeedAccessor(Holdable o) : this(static (o) => ((Holdable)o).GetSpeed(), static (o, value) => ((Holdable)o).SetSpeed(value), o) { }
}

internal abstract class ReflectionSpeedAccessorFactory {
    public abstract SpeedAccessor? ForEntity(Entity entity);
}

internal class ReflectionSpeedFieldAccessorFactory : ReflectionSpeedAccessorFactory
{
    private readonly FieldInfo SpeedField;
    private readonly Func<object, Vector2> castedGetter;
    private readonly Action<object, Vector2> castedSetter;
    public ReflectionSpeedFieldAccessorFactory(FieldInfo speedField)
    {
        SpeedField = speedField;
        castedGetter = (o) => (Vector2)SpeedField.GetValue(o)!;
        castedSetter = (o, val) => SpeedField.SetValue(o, val);
    }

    public override SpeedAccessor? ForEntity(Entity entity)
    {
        return new SpeedAccessor(castedGetter, castedSetter, entity);
    }
}

internal class ReflectionSpeedPropertyAccessorFactory : ReflectionSpeedAccessorFactory {
    private readonly Func<object, Vector2>? castedGetter;
    private readonly Action<object, Vector2>? castedSetter;

    public ReflectionSpeedPropertyAccessorFactory(PropertyInfo speedProperty) {
        var getMethod = speedProperty.GetGetMethod(true);
        var setMethod = speedProperty.GetSetMethod(true);
        if (getMethod != null) castedGetter = (o) => (Vector2) getMethod.Invoke(o, null)!;
        if (setMethod != null) castedSetter = (o, val) => setMethod.Invoke(o, [val]);
    }

    public override SpeedAccessor? ForEntity(Entity entity) {
        if (castedGetter == null || castedSetter == null) return null;
        return new SpeedAccessor(castedGetter, castedSetter, entity);
    }
}

#nullable restore