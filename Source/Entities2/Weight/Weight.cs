namespace Celeste.Mod.ScugHelper.Entities.Weight;

/// A newtype wrapper around some weight. Based on the player's weight of 20.
public readonly struct Weight {
    private Weight(double val) => Value = val;
    private readonly double Value;

    public static explicit operator Weight(float weight) => new(weight);
    public static explicit operator Weight(double weight) => new(weight);
    public static explicit operator double(Weight weight) => weight.Value;
}
