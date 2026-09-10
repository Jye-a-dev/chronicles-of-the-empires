using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Combat;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public partial class WorldMap
{
    private void HandleCombatTarget(UnitController attacker, UnitController defender, Vector2I targetGrid)
    {
        if (attacker.MovementRangeRemaining <= 0) return;

        bool inRange = CombatResolver.IsWithinAttackRange(
            attacker.GridPosition,
            targetGrid,
            attacker.Data.IsRanged ? attacker.Data.AttackRange : 1,
            _gridMapManager
        );

        if (inRange)
        {
            ExecuteCombatResolution(attacker, defender);
            return;
        }

        if (!attacker.Data.IsRanged)
        {
            var neighbors = _gridMapManager.GetNeighbors(targetGrid);
            Vector2I[] bestPath = Array.Empty<Vector2I>();
            int lowestCost = int.MaxValue;

            _pathfindingManager.SetPointSolid(attacker.GridPosition, false);
            for (int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                if (!_gridMapManager.IsWithinBounds(neighbor) || _pathfindingManager.IsPointSolid(neighbor)) continue;

                var testPath = _pathfindingManager.FindPath(attacker.GridPosition, neighbor);
                if (testPath.Length > 1)
                {
                    int cost = _pathfindingManager.CalculatePathCost(testPath, _gridMapManager);
                    if (cost < lowestCost && cost <= attacker.MovementRangeRemaining)
                    {
                        lowestCost = cost;
                        bestPath = testPath;
                    }
                }
            }
            _pathfindingManager.SetPointSolid(attacker.GridPosition, true);

            if (bestPath.Length > 1)
            {
                Vector2I moveDest = bestPath[^1];
                var destCell = _gridMapManager.GetCell(moveDest);
                if (destCell != null)
                {
                    ExecuteSafeMove(attacker, moveDest, destCell, () =>
                    {
                        if (IsInstanceValid(defender) && defender.HpCurrent > 0)
                        {
                            ExecuteCombatResolution(attacker, defender);
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// Executes sequential combat resolution via CombatResolver domain service.
    /// WorldMap triggers visual feedback and refreshes UI.
    /// </summary>
    public void ExecuteCombatResolution(UnitController attacker, UnitController defender)
    {
        if (attacker.IsMoving || defender.IsMoving) return;
        bool wasIdle = State == MapInteractionState.Idle;
        if (wasIdle) State = MapInteractionState.CombatResolving;

        int distance = _gridMapManager.GetNeighbors(attacker.GridPosition).Contains(defender.GridPosition) ? 1 : 2;
        var result = CombatResolver.ResolveCombat(attacker.Data, defender.Data, distance);

        if (result.Success)
        {
            _hud.RefreshUnitInfo();
            RefreshEconomyUI();

            if (result.DefenderDied && !attacker.Data.IsRanged)
            {
                ExecuteClaimTile(attacker.FactionId, defender.GridPosition);
            }
        }

        if (wasIdle) State = MapInteractionState.Idle;
        EvaluateAndTriggerGameOver();
    }

    public void ExecuteRecaptureOrder(UnitController actor, UnitController target)
    {
        if (actor.IsMoving || target.IsMoving || !target.IsSurrendered) return;

        bool isOriginalOwner = _playerFaction.FactionId == target.OriginalFactionId;
        var cost = isOriginalOwner
            ? new ResourceBundle(15, 0, 10, 0, 0)
            : new ResourceBundle(0, 0, 10, 0, 0);

        if (!_playerFaction.Treasury.HasEnough(cost)) return;

        _playerFaction.Treasury -= cost;

        var oldOwner = FindFactionData(target.FactionId);
        target.Data.Recapture(_playerFaction.FactionId, isOriginalOwner ? 40 : 30);
        _unitRegistry.ChangeFaction(target, target.FactionId, _playerFaction.FactionId, oldOwner, _playerFaction);

        actor.Data.MovementRemaining = Math.Max(0, actor.Data.MovementRemaining - 1);

        RefreshEconomyUI();
        _hud.DisplayUnit(target);
    }

    public BattlePayload? RequestCombatAnalysis(UnitController attacker, UnitController defender)
    {
        int distance = _gridMapManager.GetNeighbors(attacker.GridPosition).Contains(defender.GridPosition) ? 1 : 2;
        if (distance == 1)
        {
            return BuildBattlePayload(attacker, defender);
        }
        return null;
    }

    // ======================================================================
    // BATTLEPAYLOAD & SUSPEND / RESUME CONTRACT
    // ======================================================================
    public BattlePayload BuildBattlePayload(UnitController attacker, UnitController defender)
    {
        var cell = _gridMapManager.GetCell(defender.GridPosition);
        var biome = cell?.TerrainData.Biome ?? BiomeType.Plains;

        var atkSnapshot = new UnitCombatSnapshot(
            unitId: attacker.Data.UnitId,
            factionId: attacker.FactionId,
            unitType: attacker.Data.UnitType,
            hpCurrent: attacker.HpCurrent,
            hpMax: attacker.HpMax,
            attack: attacker.Attack,
            defense: attacker.Defense,
            moraleCurrent: attacker.MoraleCurrent,
            moraleMax: attacker.MoraleMax,
            isRanged: attacker.Data.IsRanged
        );

        var defSnapshot = new UnitCombatSnapshot(
            unitId: defender.Data.UnitId,
            factionId: defender.FactionId,
            unitType: defender.Data.UnitType,
            hpCurrent: defender.HpCurrent,
            hpMax: defender.HpMax,
            attack: defender.Attack,
            defense: defender.Defense,
            moraleCurrent: defender.MoraleCurrent,
            moraleMax: defender.MoraleMax,
            isRanged: defender.Data.IsRanged
        );

        return new BattlePayload(
            battleId: Guid.NewGuid().ToString("N"),
            hexPosition: defender.GridPosition,
            terrainType: biome,
            attackers: new List<UnitCombatSnapshot> { atkSnapshot },
            defenders: new List<UnitCombatSnapshot> { defSnapshot }
        );
    }

    public void ApplyBattleResolution(BattleResolutionResult result)
    {
        // 1. Update surviving units state
        for (int i = 0; i < result.SurvivingUnits.Count; i++)
        {
            var snapshot = result.SurvivingUnits[i];
            var unit = FindUnitByNumericId(snapshot.UnitId);
            if (unit != null && IsInstanceValid(unit))
            {
                int hpDiff = unit.Data.CurrentHp - snapshot.HpCurrent;
                if (hpDiff > 0)
                {
                    unit.Data.ApplyDamage(hpDiff);
                }
                int moraleDiff = snapshot.MoraleCurrent - unit.Data.MoraleCurrent;
                if (moraleDiff != 0)
                {
                    unit.Data.ModifyMorale(moraleDiff);
                }
            }
        }

        // 2. Eliminate destroyed units
        for (int i = 0; i < result.DestroyedUnitIds.Count; i++)
        {
            int destroyedId = result.DestroyedUnitIds[i];
            var unit = FindUnitByNumericId(destroyedId);
            if (unit != null && IsInstanceValid(unit))
            {
                unit.Data.Disband();
            }
        }

        _hud.RefreshUnitInfo();
        RefreshEconomyUI();
        _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);

        State = MapInteractionState.Idle;
        EvaluateAndTriggerGameOver();
    }
}

