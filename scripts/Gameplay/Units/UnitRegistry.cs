using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Central registry and Single Source of Truth for tactical military units.
/// Unifies unit tracking across factions, grid coordinates, and lifecycle events.
/// </summary>
public sealed class UnitRegistry
{
    private readonly List<UnitController> _allUnits = new();
    private readonly Dictionary<int, List<UnitController>> _unitsByFaction = new();
    private readonly Dictionary<Vector2I, UnitController> _unitsByPosition = new();

    public IReadOnlyList<UnitController> AllUnits => _allUnits;

    public event Action<UnitController>? UnitRegistered;
    public event Action<UnitController>? UnitUnregistered;
    public event Action<UnitController, Vector2I, Vector2I>? UnitPositionChanged;
    public event Action<UnitController, int, int>? UnitFactionChanged;

    public IReadOnlyList<UnitController> GetUnitsForFaction(int factionId)
    {
        if (_unitsByFaction.TryGetValue(factionId, out var list))
        {
            return list;
        }
        return Array.Empty<UnitController>();
    }

    public UnitController? GetUnitAt(Vector2I pos)
    {
        if (_unitsByPosition.TryGetValue(pos, out var unit) && GodotObject.IsInstanceValid(unit))
        {
            return unit;
        }
        return null;
    }

    public void Register(UnitController unit, FactionData? faction = null)
    {
        if (_allUnits.Contains(unit)) return;

        _allUnits.Add(unit);

        if (!_unitsByFaction.TryGetValue(unit.FactionId, out var fList))
        {
            fList = new List<UnitController>();
            _unitsByFaction[unit.FactionId] = fList;
        }
        fList.Add(unit);

        _unitsByPosition[unit.GridPosition] = unit;

        if (faction != null && !faction.Units.Contains(unit.Data))
        {
            faction.Units.Add(unit.Data);
        }

        unit.UnitMoved += HandleUnitMoved;
        unit.UnitDestroyed += HandleUnitDestroyed;

        UnitRegistered?.Invoke(unit);
    }

    public void Unregister(UnitController unit, FactionData? faction = null)
    {
        _allUnits.Remove(unit);

        if (_unitsByFaction.TryGetValue(unit.FactionId, out var fList))
        {
            fList.Remove(unit);
        }

        if (_unitsByPosition.TryGetValue(unit.GridPosition, out var current) && current == unit)
        {
            _unitsByPosition.Remove(unit.GridPosition);
        }

        var ownerFaction = faction ?? EconomyManager.Instance?.Factions.FirstOrDefault(f => f.FactionId == unit.FactionId);
        ownerFaction?.Units.Remove(unit.Data);

        unit.UnitMoved -= HandleUnitMoved;
        unit.UnitDestroyed -= HandleUnitDestroyed;

        UnitUnregistered?.Invoke(unit);
    }

    public void UpdatePosition(UnitController unit, Vector2I oldPos, Vector2I newPos)
    {
        if (_unitsByPosition.TryGetValue(oldPos, out var current) && current == unit)
        {
            _unitsByPosition.Remove(oldPos);
        }
        _unitsByPosition[newPos] = unit;

        UnitPositionChanged?.Invoke(unit, oldPos, newPos);
    }

    public void ChangeFaction(UnitController unit, int oldFactionId, int newFactionId, FactionData? oldFaction = null, FactionData? newFaction = null)
    {
        if (_unitsByFaction.TryGetValue(oldFactionId, out var oldList))
        {
            oldList.Remove(unit);
        }

        if (!_unitsByFaction.TryGetValue(newFactionId, out var newList))
        {
            newList = new List<UnitController>();
            _unitsByFaction[newFactionId] = newList;
        }
        if (!newList.Contains(unit))
        {
            newList.Add(unit);
        }

        oldFaction?.Units.Remove(unit.Data);
        if (newFaction != null && !newFaction.Units.Contains(unit.Data))
        {
            newFaction.Units.Add(unit.Data);
        }

        UnitFactionChanged?.Invoke(unit, oldFactionId, newFactionId);
    }

    public void ResolveDefection(UnitController unit, FactionData currentOwner, FactionData recipientFaction)
    {
        int oldFactionId = unit.FactionId;
        int newFactionId = recipientFaction.FactionId;

        unit.Data.Recapture(newFactionId, restoredMorale: 25);
        ChangeFaction(unit, oldFactionId, newFactionId, currentOwner, recipientFaction);
    }

    private void HandleUnitMoved(UnitController unit, Vector2I oldPos, Vector2I newPos)
    {
        UpdatePosition(unit, oldPos, newPos);
    }

    private void HandleUnitDestroyed(UnitController unit)
    {
        Unregister(unit);
    }

    public void Clear()
    {
        for (int i = 0; i < _allUnits.Count; i++)
        {
            var u = _allUnits[i];
            if (GodotObject.IsInstanceValid(u))
            {
                u.UnitMoved -= HandleUnitMoved;
                u.UnitDestroyed -= HandleUnitDestroyed;
            }
        }
        _allUnits.Clear();
        _unitsByFaction.Clear();
        _unitsByPosition.Clear();
    }
}

