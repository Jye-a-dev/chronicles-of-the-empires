namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Immutable value object detailing tactical combat outcome.
/// Allows presentation layer to trigger animations and SFX without participating in combat calculations.
/// </summary>
public readonly struct CombatResult
{
    public readonly bool Success;
    public readonly int DamageDealt;
    public readonly int CounterDamageDealt;
    public readonly int AttackerMoraleDelta;
    public readonly int DefenderMoraleDelta;
    public readonly bool DefenderDied;
    public readonly bool AttackerDied;
    public readonly bool DefenderSurrendered;
    public readonly int Distance;
    public readonly bool HasCounterAttack;
    public readonly int AttackerMovementCost;

    public CombatResult(
        bool success,
        int damageDealt,
        int counterDamageDealt,
        int attackerMoraleDelta,
        int defenderMoraleDelta,
        bool defenderDied,
        bool attackerDied,
        bool defenderSurrendered,
        int distance,
        bool hasCounterAttack,
        int attackerMovementCost)
    {
        Success = success;
        DamageDealt = damageDealt;
        CounterDamageDealt = counterDamageDealt;
        AttackerMoraleDelta = attackerMoraleDelta;
        DefenderMoraleDelta = defenderMoraleDelta;
        DefenderDied = defenderDied;
        AttackerDied = attackerDied;
        DefenderSurrendered = defenderSurrendered;
        Distance = distance;
        HasCounterAttack = hasCounterAttack;
        AttackerMovementCost = attackerMovementCost;
    }

    public static CombatResult Failed => new(
        success: false,
        damageDealt: 0,
        counterDamageDealt: 0,
        attackerMoraleDelta: 0,
        defenderMoraleDelta: 0,
        defenderDied: false,
        attackerDied: false,
        defenderSurrendered: false,
        distance: 0,
        hasCounterAttack: false,
        attackerMovementCost: 0
    );
}

