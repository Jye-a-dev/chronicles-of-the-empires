using System;

namespace ChroniclesOfTheEmpires.Core.Economy;

/// <summary>
/// Immutable value struct aggregating all 5 primary strategic resource amounts.
/// Optimized for zero heap allocations during high-frequency turn-based calculations.
/// </summary>
public readonly record struct ResourceBundle(int Food, int Production, int Gold, int Science, int Faith)
{
    public static readonly ResourceBundle Zero = new(0, 0, 0, 0, 0);

    public int this[ResourceType type] => type switch
    {
        ResourceType.Food => Food,
        ResourceType.Production => Production,
        ResourceType.Gold => Gold,
        ResourceType.Science => Science,
        ResourceType.Faith => Faith,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static ResourceBundle operator +(in ResourceBundle a, in ResourceBundle b) =>
        new(a.Food + b.Food, a.Production + b.Production, a.Gold + b.Gold, a.Science + b.Science, a.Faith + b.Faith);

    public static ResourceBundle operator -(in ResourceBundle a, in ResourceBundle b) =>
        new(a.Food - b.Food, a.Production - b.Production, a.Gold - b.Gold, a.Science - b.Science, a.Faith - b.Faith);

    public static ResourceBundle operator *(in ResourceBundle a, int multiplier) =>
        new(a.Food * multiplier, a.Production * multiplier, a.Gold * multiplier, a.Science * multiplier, a.Faith * multiplier);

    /// <summary>
    /// Checks whether this bundle has at least the required amounts of all resources in the cost bundle.
    /// </summary>
    public bool HasEnough(in ResourceBundle cost) =>
        Food >= cost.Food &&
        Production >= cost.Production &&
        Gold >= cost.Gold &&
        Science >= cost.Science &&
        Faith >= cost.Faith;

    /// <summary>
    /// Checks if any resource quantity has dropped below zero.
    /// </summary>
    public bool HasDeficit(out ResourceType deficitType)
    {
        if (Food < 0) { deficitType = ResourceType.Food; return true; }
        if (Production < 0) { deficitType = ResourceType.Production; return true; }
        if (Gold < 0) { deficitType = ResourceType.Gold; return true; }
        if (Science < 0) { deficitType = ResourceType.Science; return true; }
        if (Faith < 0) { deficitType = ResourceType.Faith; return true; }

        deficitType = default;
        return false;
    }

    /// <summary>
    /// Clamps negative values to zero.
    /// </summary>
    public ResourceBundle ClampNonNegative() =>
        new(Math.Max(0, Food), Math.Max(0, Production), Math.Max(0, Gold), Math.Max(0, Science), Math.Max(0, Faith));

    public override string ToString() =>
        $"[🌾{Food} 🔨{Production} 🪙{Gold} 🔬{Science} 📿{Faith}]";
}

