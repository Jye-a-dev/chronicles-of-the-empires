using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Domain service managing tactical recruitment constraints, economic deductions,
/// and UnitData initialization.
/// </summary>
public static class RecruitmentManager
{
    /// <summary>
    /// Checks 4 core recruitment constraints:
    /// 1. Target tile is within controlled territory.
    /// 2. Recruitment is not sanctioned/locked due to deficit.
    /// 3. Faction has sufficient stockpile for unit cost.
    /// 4. Target tile or at least 1 adjacent hex is walkable and unoccupied.
    /// </summary>
    public static bool CanRecruitUnit(
        FactionData faction,
        string unitTypeId,
        Vector2I targetTile,
        GridMapManager gridMap,
        PathfindingManager pathfinding,
        out Vector2I spawnTile)
    {
        spawnTile = targetTile;
        if (!faction.ControlledTiles.Contains(targetTile)) return false;
        if (faction.IsRecruitmentLocked) return false;

        var uCfg = GameConfigManager.GetUnitConfig(unitTypeId);
        if (uCfg == null) return false;
        if (!faction.Treasury.HasEnough(uCfg.Cost)) return false;

        // Check targetTile itself
        var targetCell = gridMap.GetCell(targetTile);
        if (targetCell != null && targetCell.OccupyingUnit == null && !targetCell.IsSolid && !pathfinding.IsPointSolid(targetTile))
        {
            spawnTile = targetTile;
            return true;
        }

        // Check surrounding hex neighbors via zero-allocation struct
        var neighbors = gridMap.GetNeighbors(targetTile);
        for (int i = 0; i < neighbors.Count; i++)
        {
            var n = neighbors[i];
            if (!gridMap.IsWithinBounds(n)) continue;

            var nCell = gridMap.GetCell(n);
            if (nCell != null && nCell.OccupyingUnit == null && !nCell.IsSolid && !pathfinding.IsPointSolid(n))
            {
                spawnTile = n;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Atomically deducts treasury costs and creates a fresh UnitData domain model.
    /// Returns null if configuration is missing or resources are insufficient.
    /// </summary>
    public static UnitData? CreateRecruitData(FactionData faction, string unitTypeId, Vector2I spawnTile)
    {
        var uCfg = GameConfigManager.GetUnitConfig(unitTypeId);
        if (uCfg == null) return null;
        if (!faction.Treasury.HasEnough(uCfg.Cost)) return null;

        // Deduct treasury
        faction.Treasury -= uCfg.Cost;

        bool isRanged = unitTypeId.Contains("cung_thu", StringComparison.OrdinalIgnoreCase);
        return new UnitData(
            id: unitTypeId,
            name: uCfg.Name,
            factionId: faction.FactionId,
            hpMax: uCfg.HpMax,
            attack: uCfg.Attack,
            defense: uCfg.Defense,
            movementMax: uCfg.MovementMax,
            gridPosition: spawnTile,
            upkeep: uCfg.Upkeep,
            cost: uCfg.Cost,
            description: uCfg.Description,
            isRanged: isRanged,
            attackRange: isRanged ? 2 : 1
        );
    }
}

