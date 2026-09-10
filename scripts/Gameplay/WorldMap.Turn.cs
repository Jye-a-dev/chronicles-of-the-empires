using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.Game;
using ChroniclesOfTheEmpires.UI.Modals;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public partial class WorldMap
{
    private void OnTileUpgradeRequested(string improvementId, Vector2I coords)
    {
        var cell = _gridMapManager.GetCell(coords);
        if (cell == null || cell.OwnerFactionId != 0) return;

        ResourceBundle cost;
        ImprovementType impType;
        ResourceBundle bonusYield;
        int buildTurns = 2;

        if (cell.Deposit != null && !cell.Deposit.IsExploited)
        {
            var bldCfg = GameConfigManager.GetBuildingConfig(cell.Deposit.RequiredImprovement);
            cost = bldCfg?.Cost ?? new ResourceBundle(0, 30, 20, 0, 0);
            impType = cell.Deposit.Category switch
            {
                DepositCategory.Mineral => ImprovementType.Mine,
                DepositCategory.Agricultural => ImprovementType.Farm,
                _ => ImprovementType.Mine
            };
            bonusYield = cell.Deposit.BonusYield;
        }
        else
        {
            var (defaultImp, _, impCost) = TileInfoModal.GetDefaultImprovementForTile(cell.TerrainData.Biome);
            impType = defaultImp;
            cost = impCost;
            bonusYield = impType switch
            {
                ImprovementType.Farm => new ResourceBundle(2, 0, 0, 0, 0),
                ImprovementType.LumberMill => new ResourceBundle(0, 2, 0, 0, 0),
                ImprovementType.Mine => new ResourceBundle(0, 2, 1, 0, 0),
                ImprovementType.Watchtower => new ResourceBundle(0, 0, 0, 0, 1),
                _ => ResourceBundle.Zero
            };
        }

        if (!_playerFaction.Treasury.HasEnough(cost))
        {
            return;
        }

        _playerFaction.Treasury -= cost;
        cell.TerrainData.Improvement = impType;
        cell.TerrainData.ImprovementBonusYield = bonusYield;
        cell.TerrainData.ConstructionTurnsRemaining = buildTurns;
        cell.TerrainData.IsConstructed = false;

        if (cell.Deposit != null)
        {
            cell.Deposit.IsExploited = true;
        }

        RefreshEconomyUI();
        _hud.DisplayTile(cell);
    }

    private async void OnEndTurnPressed()
    {
        if (State != MapInteractionState.Idle) return;

        State = MapInteractionState.AITurnProcessing;
        _hud.SetButtonsDisabled(true);

        try
        {
            // 1. Process Surrendered Units: 0-allocation hex scan for rival defections
            var allUnits = _unitRegistry.AllUnits;
            for (int i = allUnits.Count - 1; i >= 0; i--)
            {
                var unit = allUnits[i];
                if (!IsInstanceValid(unit) || !unit.IsSurrendered) continue;

                unit.Data.SurrenderTurnsRemaining--;
                if (unit.Data.SurrenderTurnsRemaining <= 0)
                {
                    UnitController? rivalNeighbor = null;
                    var neighbors = _gridMapManager.GetNeighbors(unit.GridPosition);
                    for (int n = 0; n < neighbors.Count; n++)
                    {
                        var cell = _gridMapManager.GetCell(neighbors[n]);
                        if (cell?.OccupyingUnit is UnitController other && IsInstanceValid(other) &&
                            !other.IsSurrendered && other.FactionId != unit.OriginalFactionId)
                        {
                            rivalNeighbor = other;
                            break;
                        }
                    }

                    if (rivalNeighbor != null)
                    {
                        var currentOwner = FindFactionData(unit.FactionId);
                        var recipientFaction = FindFactionData(rivalNeighbor.FactionId);

                        if (currentOwner != null && recipientFaction != null)
                        {
                            _unitRegistry.ResolveDefection(unit, currentOwner, recipientFaction);
                        }
                        else
                        {
                            unit.Data.Recapture(rivalNeighbor.FactionId, 25);
                        }
                    }
                }
            }

            // 2. Execute AI turns sequentially on Main Thread
            await _aiTurnExecutor.ExecuteAllAITurnsAsync(
                this,
                _gridMapManager,
                _pathfindingManager,
                _economyManager,
                _unitRegistry,
                _asyncPacer,
                _asyncPacer.Token
            );

            // 3. Reset movement points for non-player units
            for (int i = 0; i < allUnits.Count; i++)
            {
                var u = allUnits[i];
                if (IsInstanceValid(u) && u.FactionId != 0)
                {
                    u.ResetTurnMovement();
                }
            }

            // 4. Advance turn counter, calculate economy for all factions, and refresh player military points
            _turnManager.EndTurn();
            DeselectAll();

            // 5. Update player fog of war vision for the new turn
            _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
            RefreshEconomyUI();
        }
        catch (OperationCanceledException)
        {
            // Clean abort when game is exited to menu during turn processing
        }
        finally
        {
            if (State != MapInteractionState.Disabled)
            {
                _hud.SetButtonsDisabled(false);
                State = MapInteractionState.Idle;
                EvaluateAndTriggerGameOver();
            }
        }
    }

    private void OnTurnChanged(int turn, int food, int prod, int gold, int dFood, int dProd, int dGold)
    {
        RefreshEconomyUI();
        _hud.RefreshUnitInfo();
    }

    public void RefreshEconomyUI()
    {
        var (_, _, netIncome) = _economyManager.CalculateTurnIncome(_playerFaction);
        _hud.UpdateEconomy(_playerFaction.Treasury, netIncome, _turnManager.TurnCount);
    }

    // ======================================================================
    // TERRITORY & EXPAND PIPELINE
    // ======================================================================
    public bool CanClaimTile(int factionId, Vector2I coord)
    {
        if (!_gridMapManager.IsWithinBounds(coord)) return false;
        var cell = _gridMapManager.GetCell(coord);
        if (cell == null || cell.IsSolid) return false;

        // Cannot claim if occupied by active, non-surrendered hostile unit
        if (cell.OccupyingUnit is UnitController occupier && IsInstanceValid(occupier) &&
            occupier.FactionId != factionId && !occupier.IsSurrendered && occupier.Data.IsActive)
        {
            return false;
        }

        return true;
    }

    public void ExecuteClaimTile(int factionId, Vector2I coord)
    {
        if (!CanClaimTile(factionId, coord)) return;

        var cell = _gridMapManager.GetCell(coord);
        if (cell == null) return;

        int oldOwner = cell.OwnerFactionId;
        if (oldOwner == factionId) return;

        if (oldOwner >= 0)
        {
            var oldFaction = FindFactionData(oldOwner);
            oldFaction?.ControlledTiles.Remove(coord);
        }

        var newFaction = FindFactionData(factionId);
        if (newFaction != null)
        {
            newFaction.ControlledTiles.Add(coord);
            cell.OwnerFactionId = factionId;

            _gridMapManager.RefreshTileOwnerVisual(coord, factionId);
            _economyManager.NotifyTerritoryChanged(newFaction);
            RefreshEconomyUI();
        }
    }

    // ======================================================================
    // VICTORY / DEFEAT PIPELINE
    // ======================================================================
    public GameResult CheckGameOverConditions(out VictoryType victoryType)
    {
        // 1. Defeat Condition: Player has 0 active unsurrendered units OR lost all territory
        var playerUnits = _unitRegistry.GetUnitsForFaction(0);
        int activePlayerUnits = 0;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            var u = playerUnits[i];
            if (IsInstanceValid(u) && u.Data.IsActive && !u.IsSurrendered && u.HpCurrent > 0)
            {
                activePlayerUnits++;
            }
        }

        if (activePlayerUnits == 0)
        {
            victoryType = VictoryType.Conquest;
            return GameResult.Defeat;
        }

        if (_playerFaction.ControlledTiles.Count == 0)
        {
            victoryType = VictoryType.Domination;
            return GameResult.Defeat;
        }

        // 2. Victory Condition (Conquest): All rival factions have 0 active units
        bool anyRivalAlive = false;
        var factions = _economyManager.Factions;
        for (int f = 0; f < factions.Count; f++)
        {
            var faction = factions[f];
            if (faction.FactionId == 0) continue;

            var rivalUnits = _unitRegistry.GetUnitsForFaction(faction.FactionId);
            for (int u = 0; u < rivalUnits.Count; u++)
            {
                var ru = rivalUnits[u];
                if (IsInstanceValid(ru) && ru.Data.IsActive && !ru.IsSurrendered && ru.HpCurrent > 0)
                {
                    anyRivalAlive = true;
                    break;
                }
            }
            if (anyRivalAlive) break;
        }

        if (!anyRivalAlive && factions.Count > 1)
        {
            victoryType = VictoryType.Conquest;
            return GameResult.Victory;
        }

        victoryType = VictoryType.None;
        return GameResult.Undecided;
    }

    public bool EvaluateAndTriggerGameOver()
    {
        if (State == MapInteractionState.Disabled) return true;

        var result = CheckGameOverConditions(out var victoryType);
        if (result == GameResult.Undecided) return false;

        State = MapInteractionState.Disabled;
        _hud.SetButtonsDisabled(true);
        DeselectAll();

        if (_endGameModal == null)
        {
            var modalScene = GD.Load<PackedScene>("res://scenes/ui/end_game_modal.tscn");
            if (modalScene != null)
            {
                _endGameModal = modalScene.Instantiate<EndGameModal>();
                AddChild(_endGameModal);
            }
        }

        _endGameModal?.ShowResult(result, victoryType, _turnManager.TurnCount, _enemiesEliminated);
        return true;
    }
}

