using System;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Core.Economy;

/// <summary>
/// Level 2 Faction Entity & Single Source of Truth for tactical military units.
/// Encapsulates combat stats, movement budgets, morale, surrender states, and domain event lifecycle.
/// </summary>
public class UnitData
{
    private static int _nextNumericId = 1;
    public int UnitId { get; set; } = System.Threading.Interlocked.Increment(ref _nextNumericId);

    public string Id { get; init; } = "";
    private string? _unitType;
    public string UnitType
    {
        get => !string.IsNullOrEmpty(_unitType) ? _unitType : Id;
        init => _unitType = value;
    }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public int FactionId { get; private set; }
    public int OriginalFactionId { get; init; }

    public int HpMax { get; init; } = 20;
    public int CurrentHp { get; private set; } = 20;
    public int Attack { get; init; } = 5;
    public int Defense { get; init; } = 2;
    public int MovementMax { get; init; } = 4;
    public int MovementRemaining { get; set; } = 4;

    public bool IsRanged { get; init; } = false;
    public int AttackRange { get; init; } = 1;

    public Vector2I GridPosition { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsMoving { get; set; } = false;

    // Morale & Surrender Protocol
    public int MoraleMax { get; init; } = 100;
    public int MoraleCurrent { get; private set; } = 100;
    public bool IsSurrendered { get; private set; } = false;
    public int SurrenderTurnsRemaining { get; set; } = 2;

    // Level 3 Economy Contracts
    public ResourceBundle ProductionCost { get; init; } = ResourceBundle.Zero;
    public ResourceBundle Upkeep { get; init; } = ResourceBundle.Zero;

    // Domain Events
    public event Action<int, int>? OnHpChanged;
    public event Action<int, int>? OnMoraleChanged;
    public event Action<bool>? OnSurrenderStateChanged;
    public event Action<int>? OnFactionChanged;
    public event Action? OnDestroyed;

    private bool _isDestroyed = false;

    public UnitData() { }

    public UnitData(
        string id,
        string name,
        int factionId,
        int hpMax,
        int attack,
        int defense,
        int movementMax,
        Vector2I gridPosition,
        ResourceBundle upkeep,
        ResourceBundle cost,
        string description = "",
        bool isRanged = false,
        int attackRange = 1)
    {
        Id = id;
        Name = name;
        Description = description;
        FactionId = factionId;
        OriginalFactionId = factionId;
        HpMax = hpMax;
        CurrentHp = hpMax;
        Attack = attack;
        Defense = defense;
        MovementMax = movementMax;
        MovementRemaining = movementMax;
        GridPosition = gridPosition;
        Upkeep = upkeep;
        ProductionCost = cost;
        IsRanged = isRanged;
        AttackRange = attackRange;
    }

    public void ApplyDamage(int dmg)
    {
        if (_isDestroyed) return;

        CurrentHp = Math.Clamp(CurrentHp - dmg, 0, HpMax);
        OnHpChanged?.Invoke(CurrentHp, HpMax);

        if (CurrentHp <= 0)
        {
            Disband();
        }
    }

    public void ModifyMorale(int delta)
    {
        if (_isDestroyed) return;

        MoraleCurrent = Math.Clamp(MoraleCurrent + delta, 0, MoraleMax);
        OnMoraleChanged?.Invoke(MoraleCurrent, MoraleMax);

        if (MoraleCurrent <= 0 && !IsSurrendered)
        {
            IsSurrendered = true;
            SurrenderTurnsRemaining = 2;
            MovementRemaining = 0;
            OnSurrenderStateChanged?.Invoke(true);
        }
    }

    public void Recapture(int newFactionId, int restoredMorale)
    {
        if (_isDestroyed) return;

        FactionId = newFactionId;
        IsSurrendered = false;
        MoraleCurrent = Math.Clamp(restoredMorale, 0, MoraleMax);
        SurrenderTurnsRemaining = 2;
        MovementRemaining = 0;

        OnSurrenderStateChanged?.Invoke(false);
        OnFactionChanged?.Invoke(FactionId);
        OnMoraleChanged?.Invoke(MoraleCurrent, MoraleMax);
    }

    public void Disband()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        IsActive = false;
        CurrentHp = 0;
        OnDestroyed?.Invoke();

        // Detach all listeners to guarantee immediate GC collection
        OnHpChanged = null;
        OnMoraleChanged = null;
        OnSurrenderStateChanged = null;
        OnFactionChanged = null;
        OnDestroyed = null;
    }

    public void ResetTurnMovement()
    {
        if (IsSurrendered)
        {
            MovementRemaining = 0;
            return;
        }

        MovementRemaining = MovementMax;
    }
}
