using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using static Godot.GodotObject;
using ChroniclesOfTheEmpires.Core.Mathematics;
using ChroniclesOfTheEmpires.Gameplay.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay.AI;

/// <summary>
/// Basic autonomous turn executor for rival non-player factions.
/// Handles nearest-player targeting, safe path-truncated movement, melee/ranged combat decisions,
/// and smooth visual pacing on Godot's Main Thread with zero-allocation neighbors and cancellation safety.
/// </summary>
public class SimpleAITurnExecutor
{
    public async Task ExecuteAllAITurnsAsync(
        WorldMap worldMap,
        GridMapManager gridMap,
        PathfindingManager pathfinding,
        EconomyManager economy,
        UnitRegistry unitRegistry,
        AsyncPacer pacer,
        CancellationToken ct = default)
    {
        try
        {
            var factions = economy.Factions;
            var allUnits = unitRegistry.AllUnits;

            for (int f = 0; f < factions.Count; f++)
            {
                ct.ThrowIfCancellationRequested();
                var faction = factions[f];
                if (faction.FactionId == 0) continue; // Skip human player

                // Snapshot active AI units belonging to this faction
                var factionAiUnits = unitRegistry.GetUnitsForFaction(faction.FactionId);
                // Copy references to local array to avoid mutation during turn
                var aiUnitsSnapshot = new UnitController[factionAiUnits.Count];
                for (int u = 0; u < factionAiUnits.Count; u++)
                {
                    aiUnitsSnapshot[u] = factionAiUnits[u];
                }

                for (int i = 0; i < aiUnitsSnapshot.Length; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var aiUnit = aiUnitsSnapshot[i];
                    if (!IsInstanceValid(aiUnit) || aiUnit.IsSurrendered || !aiUnit.Data.IsActive || aiUnit.MovementRangeRemaining <= 0)
                    {
                        continue;
                    }

                    // 1. Locate closest alive and unsurrendered player unit (FactionId == 0)
                    UnitController? closestTarget = null;
                    int minDistance = int.MaxValue;
                    var playerUnits = unitRegistry.GetUnitsForFaction(0);

                    for (int p = 0; p < playerUnits.Count; p++)
                    {
                        var playerCandidate = playerUnits[p];
                        if (!IsInstanceValid(playerCandidate) || playerCandidate.FactionId != 0 ||
                            playerCandidate.IsSurrendered || !playerCandidate.Data.IsActive)
                        {
                            continue;
                        }

                        int dist = HexMath.GetDistance(aiUnit.GridPosition, playerCandidate.GridPosition);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closestTarget = playerCandidate;
                        }
                    }

                    if (closestTarget == null || !IsInstanceValid(closestTarget)) continue;
                    UnitController target = closestTarget;

                    int attackRange = aiUnit.Data.IsRanged ? aiUnit.Data.AttackRange : 1;

                    // 2. Decision Tree
                    if (minDistance <= attackRange)
                    {
                        // Case A: Target already in attack range -> Strike immediately
                        worldMap.ExecuteCombatResolution(aiUnit, target);
                        await pacer.DelayAsync(worldMap.GetTree(), 0.22f, ct);
                    }
                    else
                    {
                        // Case B: Outside attack range -> Approach target
                        var neighborOffsets = HexMath.GetNeighborOffsets(target.GridPosition.Y);
                        Vector2I[] bestPath = Array.Empty<Vector2I>();
                        int bestPathCost = int.MaxValue;

                        pathfinding.SetPointSolid(aiUnit.GridPosition, false);
                        for (int n = 0; n < neighborOffsets.Length; n++)
                        {
                            var neighbor = target.GridPosition + neighborOffsets[n];
                            if (!gridMap.IsWithinBounds(neighbor) || pathfinding.IsPointSolid(neighbor)) continue;

                            var testPath = pathfinding.FindPath(aiUnit.GridPosition, neighbor);
                            if (testPath.Length > 1)
                            {
                                int cost = pathfinding.CalculatePathCost(testPath, gridMap);
                                if (cost < bestPathCost)
                                {
                                    bestPathCost = cost;
                                    bestPath = testPath;
                                }
                            }
                        }
                        pathfinding.SetPointSolid(aiUnit.GridPosition, true);

                        if (bestPath.Length > 1)
                        {
                            // Calculate maximum reach with current movement budget
                            int budget = aiUnit.MovementRangeRemaining;
                            int maxStepIndex = 0;
                            int accumulatedCost = 0;

                            for (int s = 1; s < bestPath.Length; s++)
                            {
                                var stepCell = gridMap.GetCell(bestPath[s]);
                                int moveCost = stepCell?.MoveCost ?? 1;
                                if (accumulatedCost + moveCost <= budget)
                                {
                                    accumulatedCost += moveCost;
                                    maxStepIndex = s;
                                }
                                else break;
                            }

                            // ANTI-DEADLOCK SAFE PATH TRUNCATION:
                            // Back-track backwards along the sliced path to find the furthest unblocked, unoccupied destination
                            int validStepIndex = -1;
                            for (int s = maxStepIndex; s >= 1; s--)
                            {
                                var candidatePos = bestPath[s];
                                var candidateCell = gridMap.GetCell(candidatePos);
                                if (candidateCell != null && candidateCell.OccupyingUnit == null &&
                                    !candidateCell.IsSolid && !pathfinding.IsPointSolid(candidatePos))
                                {
                                    validStepIndex = s;
                                    break;
                                }
                            }

                            // Only execute move if a genuinely unoccupied step exists
                            if (validStepIndex >= 1)
                            {
                                var slicedPath = new Vector2I[validStepIndex + 1];
                                Array.Copy(bestPath, 0, slicedPath, 0, validStepIndex + 1);

                                var destGrid = slicedPath[^1];
                                var destCell = gridMap.GetCell(destGrid);

                                if (destCell != null)
                                {
                                    int moveCost = pathfinding.CalculatePathCost(slicedPath, gridMap);
                                    await worldMap.ExecuteSafeMoveTransactionalAsync(aiUnit, destGrid, slicedPath, moveCost);
                                    await pacer.DelayAsync(worldMap.GetTree(), 0.18f, ct);

                                    // Post-move attack opportunity check
                                    if (IsInstanceValid(target) && target.Data.CurrentHp > 0 && !target.IsSurrendered)
                                    {
                                        int postDist = HexMath.GetDistance(aiUnit.GridPosition, target.GridPosition);
                                        if (postDist <= attackRange)
                                        {
                                            worldMap.ExecuteCombatResolution(aiUnit, target);
                                            await pacer.DelayAsync(worldMap.GetTree(), 0.22f, ct);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Small pacing pause between AI actions
                    await pacer.DelayAsync(worldMap.GetTree(), 0.12f, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cleanly abort AI loop when scene changes or player exits to menu
        }
    }

    public static int GetHexDistance(Vector2I a, Vector2I b, GridMapManager? gridMap = null) =>
        HexMath.GetDistance(a, b);
}
