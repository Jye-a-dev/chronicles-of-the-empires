using System;
using System.Linq;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay;
using ChroniclesOfTheEmpires.Gameplay.Economy;

namespace ChroniclesOfTheEmpires.Core.Entities;

/// <summary>
/// Authoritative lifecycle manager for military units.
/// Atomically cleans up A* collision flags, cell occupancy, faction units, domain events,
/// and presentation layer bindings to prevent memory leaks and ghost collisions.
/// </summary>
public static class UnitLifecycleManager
{
    public static void TerminateUnit(
        UnitController unit,
        UnitRegistry registry,
        PathfindingManager pathfinding,
        GridMapManager? gridMap = null,
        EconomyManager? economy = null)
    {
        if (unit == null || !GodotObject.IsInstanceValid(unit)) return;

        UnitData data = unit.Data;
        Vector2I currentPos = data != null ? data.GridPosition : unit.GridPosition;

        // 1. Remove pathfinding solid obstacle and grid cell occupancy
        pathfinding?.SetPointSolid(currentPos, false);

        if (gridMap != null)
        {
            var cell = gridMap.GetCell(currentPos);
            if (cell != null && cell.OccupyingUnit == unit)
            {
                cell.OccupyingUnit = null;
            }
        }

        // 2. Remove domain entity from faction registry and sever domain delegates
        if (data != null)
        {
            var ownerFaction = economy?.Factions?.FirstOrDefault(f => f.FactionId == data.FactionId);
            ownerFaction?.Units.Remove(data);

            data.Disband();
        }

        registry?.Unregister(unit);

        // 3. Sever presentation layer bindings
        unit.Unbind();
        unit.Visible = false;

        // 4. Safely queue free node
        if (!unit.IsQueuedForDeletion())
        {
            unit.QueueFree();
        }
    }
}

