using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Domain service executing tactical combat calculations, damage resolution,
/// and strict counter-attack validation without UI or scene-tree dependencies.
/// </summary>
public static class CombatResolver
{
    public const int AttackMovementCost = 2;

    /// <summary>
    /// Checks whether target is within the attacker's striking range using zero-allocation hex neighbor queries.
    /// </summary>
    public static bool IsWithinAttackRange(Vector2I start, Vector2I target, int range, GridMapManager gridMap)
    {
        if (start == target) return true;
        if (range <= 0) return false;

        var ring1 = gridMap.GetNeighbors(start);
        if (ring1.Contains(target)) return true;
        if (range == 1) return false;

        // Range 2 search: expand from ring 1 neighbors
        for (int i = 0; i < ring1.Count; i++)
        {
            var p1 = ring1[i];
            if (!gridMap.IsWithinBounds(p1)) continue;

            var ring2 = gridMap.GetNeighbors(p1);
            if (ring2.Contains(target)) return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves tactical engagement between attacker and defender.
    /// Strictly guarantees no double suicides and enforces proper counter-attack conditions.
    /// </summary>
    public static CombatResult ResolveCombat(UnitData attacker, UnitData defender, int distance)
    {
        if (attacker.MovementRemaining <= 0 || !attacker.IsActive || attacker.CurrentHp <= 0)
        {
            return CombatResult.Failed;
        }

        // Phase 1: Attacker strikes Defender
        float atkMoraleMod = attacker.MoraleCurrent > 80 ? 1.15f : (attacker.MoraleCurrent < 40 ? 0.75f : 1.0f);
        int rawDmg = Math.Max(1, (int)(attacker.Attack * atkMoraleMod) - defender.Defense);

        int defInitialHp = defender.CurrentHp;
        defender.ApplyDamage(rawDmg);
        int actualDamageDealt = defInitialHp - defender.CurrentHp;
        defender.ModifyMorale(-20);

        attacker.MovementRemaining = Math.Max(0, attacker.MovementRemaining - AttackMovementCost);

        bool defenderDied = defender.CurrentHp <= 0 || !defender.IsActive;
        bool defenderSurrendered = defender.IsSurrendered;

        // Phase 2: Counter-attack validation with strict double-suicide & range guards
        // Counter-attack is allowed only if Defender survived, is not surrendered, and Attacker is in range (or melee)
        bool canCounter = !defenderDied && !defenderSurrendered && (!attacker.IsRanged || distance == 1);

        int counterDmg = 0;
        bool attackerDied = false;

        if (canCounter)
        {
            int atkInitialHp = attacker.CurrentHp;
            int rawCounterDmg = Math.Max(1, defender.Defense - (attacker.Defense / 2));
            attacker.ApplyDamage(rawCounterDmg);
            counterDmg = atkInitialHp - attacker.CurrentHp;
            attacker.ModifyMorale(-10);

            attackerDied = attacker.CurrentHp <= 0 || !attacker.IsActive;
        }

        return new CombatResult(
            success: true,
            damageDealt: actualDamageDealt,
            counterDamageDealt: counterDmg,
            attackerMoraleDelta: canCounter ? -10 : 0,
            defenderMoraleDelta: -20,
            defenderDied: defenderDied,
            attackerDied: attackerDied,
            defenderSurrendered: defenderSurrendered,
            distance: distance,
            hasCounterAttack: canCounter,
            attackerMovementCost: AttackMovementCost
        );
    }
}

