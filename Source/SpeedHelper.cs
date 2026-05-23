using Microsoft.Xna.Framework;
using Celeste;
using Monocle;
using System.Reflection;
using System;
using Celeste.Mod;
using System.Collections.Concurrent;

#nullable enable

public interface IHasSpeed
{
    public abstract Vector2 Speed { get; set; }
}

public readonly struct SpeedAccessor
{
    private readonly Func<object, Vector2> Getter;
    private readonly Action<object, Vector2> Setter;
    private readonly object Object;
    public readonly Vector2 Speed { get {
            if (Getter == null) throw new NullReferenceException("Speed getter must not be null. Check you're not accidentally initializing the struct with null values.");
            var ret = Getter(Object);
            if (Object is Actor actor && actor.IsInverted()) ret = new(ret.X, -ret.Y);
            return ret;
        }
        set {
            if (Setter == null) throw new NullReferenceException("Speed setter must not be null. Check you're not accidentally initializing the struct with null values.");
            if (Object is Actor actor && actor.IsInverted()) value = new(value.X, -value.Y);
            Setter(Object, value);
        }
    }

    internal SpeedAccessor(Func<object, Vector2> getter, Action<object, Vector2> setter, object obj) {
        Getter = getter ?? throw new NullReferenceException("Speed getter must not be null. (in ctor)");
        Setter = setter ?? throw new NullReferenceException("Speed setter must not be null. (in ctor)");
        Object = obj ?? throw new NullReferenceException("Speed object must not be null."); ;
    }

    // TODO: Remove these when IHasSpeed is implemented on them
    public SpeedAccessor(Player o) : this(static (o) => ((Player)o).Speed, static (o, value) => ((Player)o).Speed = value, o) { }
    public SpeedAccessor(Solid o) : this(static (o) => ((Solid)o).Speed, static (o, value) => ((Solid)o).Speed = value, o) { }
    public SpeedAccessor(Puffer o) : this(static (o) => ((Puffer)o).hitSpeed, static (o, value) => ((Puffer)o).hitSpeed = value, o) { }
    public SpeedAccessor(Seeker o) : this(static (o) => ((Seeker)o).Speed, static (o, value) => ((Seeker)o).Speed = value, o) { }
    public SpeedAccessor(PlayerSeeker o) : this(static (o) => ((PlayerSeeker)o).speed, static (o, value) => ((PlayerSeeker)o).speed = value, o) { }
    public SpeedAccessor(TheoCrystal o) : this(static (o) => ((TheoCrystal)o).Speed, static (o, value) => ((TheoCrystal)o).Speed = value, o) { }
    // -----
    public SpeedAccessor(Holdable o) : this(static (o) => ((Holdable)o).GetSpeed(), static (o, value) => ((Holdable)o).SetSpeed(value), o) { }
    public SpeedAccessor(IHasSpeed o) : this(static (o) => ((IHasSpeed)o).Speed, static (o, value) => ((IHasSpeed)o).Speed = value, o) { }

    private static readonly ConcurrentDictionary<Type, ReflectionSpeedAccessorFactory?> SpeedAccessorCache = [];

    public static SpeedAccessor? For(Entity obj) {
        switch (obj) {
            // TODO: Remove these when IHasSpeed is implemented on them
            case Player player: return new SpeedAccessor(player);
            case Solid solid: return new SpeedAccessor(solid);
            case PlayerSeeker seeker: return new SpeedAccessor(seeker);
            case Seeker seeker: return new SpeedAccessor(seeker);
            case TheoCrystal crystal: return new SpeedAccessor(crystal);
            case Puffer puffer: return new SpeedAccessor(puffer);
            // -----
            case IHasSpeed speed: return new SpeedAccessor(speed);
            default:
                var holdable = obj.Components.Get<Holdable>();
                if (holdable != null && holdable.SpeedGetter != null && holdable.SpeedSetter != null)
                    return new SpeedAccessor(holdable);
                break;
        }

        Type type = obj.GetType();
        if (SpeedAccessorCache.TryGetValue(type, out ReflectionSpeedAccessorFactory? value))
            return value?.For(obj);

        Logger.Verbose(nameof(SpeedAccessor), $"Creating new reflection speed accessor for entity type {type}!");

        PropertyInfo? speedProperty
            = type.GetProperty("Speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetProperty("speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetProperty("Velocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetProperty("velocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetProperty("hitSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (speedProperty != null && speedProperty.PropertyType == typeof(Vector2)) {
            var accessor = new ReflectionSpeedPropertyAccessorFactory(speedProperty);
            if (accessor.castedGetter != null && accessor.castedSetter != null) {
                SpeedAccessorCache.TryAdd(type, accessor);
                return accessor.For(obj);
            }
        }

        FieldInfo? speedField
            = type.GetField("Speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetField("speed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetField("Velocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetField("velocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetField("hitSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (speedField != null && speedField.FieldType == typeof(Vector2)) {
            var accessor = new ReflectionSpeedFieldAccessorFactory(speedField);
            if (accessor.castedGetter != null && accessor.castedSetter != null) {
                SpeedAccessorCache.TryAdd(type, accessor);
                return accessor.For(obj);
            }
        }
        Logger.Warn(nameof(SpeedAccessor), $"Could not find speed for entity type {type}!");

        SpeedAccessorCache.TryAdd(type, null);
        return null;
    }
}

internal abstract class ReflectionSpeedAccessorFactory
{
    public abstract SpeedAccessor? For(Entity entity);
}

internal class ReflectionSpeedFieldAccessorFactory : ReflectionSpeedAccessorFactory
{
    private readonly FieldInfo SpeedField;
    internal readonly Func<object, Vector2> castedGetter;
    internal readonly Action<object, Vector2> castedSetter;
    public ReflectionSpeedFieldAccessorFactory(FieldInfo speedField) {
        SpeedField = speedField;
        castedGetter = (o) => (Vector2)(SpeedField.GetValue(o) ?? throw new Exception("Speed field was null."));
        castedSetter = (o, val) => SpeedField.SetValue(o, val);
    }

    public override SpeedAccessor? For(Entity entity)
        => new SpeedAccessor(castedGetter, castedSetter, entity);
}

internal class ReflectionSpeedPropertyAccessorFactory : ReflectionSpeedAccessorFactory
{
    internal readonly Func<object, Vector2>? castedGetter;
    internal readonly Action<object, Vector2>? castedSetter;

    public ReflectionSpeedPropertyAccessorFactory(PropertyInfo speedProperty) {
        var getMethod = speedProperty.GetGetMethod(true);
        var setMethod = speedProperty.GetSetMethod(true);
        if (getMethod != null) castedGetter = (o) => (Vector2)getMethod.Invoke(o, null)!;
        if (setMethod != null) castedSetter = (o, val) => setMethod.Invoke(o, [val]);
    }

    public override SpeedAccessor? For(Entity entity) {
        if (castedGetter == null || castedSetter == null) return null;
        return new SpeedAccessor(castedGetter, castedSetter, entity);
    }
}

#nullable restore
