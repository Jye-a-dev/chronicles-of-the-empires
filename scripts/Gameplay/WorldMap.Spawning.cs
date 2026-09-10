using System;
using System.Linq;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.Entities;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

public partial class WorldMap
{
    private void SpawnInitialUnits()
    {
        var unitScene = GD.Load<PackedScene>("res://scenes/gameplay/unit.tscn");
        if (unitScene == null) return;

        Vector2I p1Pos = FindWalkableCell(new Vector2I(2, 2));
        Vector2I p2Pos = FindWalkableCell(new Vector2I(3, 3));
        Vector2I rivalPos = FindWalkableCell(new Vector2I(_gridMapManager.MapWidth - 4, _gridMapManager.MapHeight - 4));

        SpawnUnit(unitScene, "cam_ve_quan", 0, p1Pos);
        SpawnUnit(unitScene, "cung_thu", 0, p2Pos);
        SpawnUnit(unitScene, "tien_phong_dich", 1, rivalPos);

        _camera.Position = GridMapManager.GridToWorldCenter(p1Pos);
    }

    private void SpawnUnit(PackedScene unitScene, string configId, int factionId, Vector2I gridPos)
    {
        var uCfg = GameConfigManager.GetUnitConfig(configId);
        bool isRanged = configId.Contains("cung_thu", StringComparison.OrdinalIgnoreCase);

        var data = new UnitData(
            id: configId,
            name: uCfg?.Name ?? "Chiến Binh",
            factionId: factionId,
            hpMax: uCfg?.HpMax ?? 20,
            attack: uCfg?.Attack ?? 6,
            defense: uCfg?.Defense ?? 3,
            movementMax: uCfg?.MovementMax ?? 4,
            gridPosition: gridPos,
            upkeep: uCfg?.Upkeep ?? ResourceBundle.Zero,
            cost: uCfg?.Cost ?? ResourceBundle.Zero,
            description: uCfg?.Description ?? "",
            isRanged: isRanged,
            attackRange: isRanged ? 2 : 1
        );

        var controller = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(controller);
        controller.Bind(data);
        RegisterUnit(controller, FindFactionData(factionId));
    }

    public UnitController? ExecuteRecruitUnit(FactionData faction, string unitTypeId, Vector2I targetTile)
    {
        if (!RecruitmentManager.CanRecruitUnit(faction, unitTypeId, targetTile, _gridMapManager, _pathfindingManager, out Vector2I spawnTile))
        {
            return null;
        }

        var data = RecruitmentManager.CreateRecruitData(faction, unitTypeId, spawnTile);
        if (data == null) return null;

        var unitScene = GD.Load<PackedScene>("res://scenes/gameplay/unit.tscn");
        if (unitScene == null) return null;

        var controller = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(controller);
        controller.Bind(data);

        RegisterUnit(controller, faction);
        RefreshEconomyUI();

        if (faction.FactionId == 0)
        {
            _fogOfWarManager.UpdatePlayerVisibility(_unitRegistry.AllUnits, _gridMapManager);
        }

        return controller;
    }

    private void RegisterUnit(UnitController unit, FactionData? faction)
    {
        _unitRegistry.Register(unit, faction);
        _pathfindingManager.SetPointSolid(unit.GridPosition, true);

        var cell = _gridMapManager.GetCell(unit.GridPosition);
        if (cell != null) cell.OccupyingUnit = unit;

        unit.UnitDestroyed += HandleUnitDestroyed;
        unit.UnitClicked += SelectUnit;
        unit.UnitMoved += HandleUnitMovedVisualSync;
    }

    private void HandleUnitMovedVisualSync(UnitController u, Vector2I oldPos, Vector2I newPos)
    {
        _hexIndicator.SelectHex(u.GlobalPosition);
        _hud.RefreshUnitInfo();
    }

    private void HandleUnitDestroyed(UnitController unit)
    {
        unit.UnitDestroyed -= HandleUnitDestroyed;
        unit.UnitClicked -= SelectUnit;
        unit.UnitMoved -= HandleUnitMovedVisualSync;

        if (unit.FactionId != 0)
        {
            _enemiesEliminated++;
        }

        UnitLifecycleManager.TerminateUnit(unit, _unitRegistry, _pathfindingManager, _gridMapManager, _economyManager);

        if (_selectedUnit == unit)
        {
            DeselectAll();
        }

        EvaluateAndTriggerGameOver();
    }

    public void ClearAllUnits()
    {
        var units = _unitRegistry.AllUnits.ToArray();
        for (int i = 0; i < units.Length; i++)
        {
            var u = units[i];
            if (GodotObject.IsInstanceValid(u))
            {
                u.UnitDestroyed -= HandleUnitDestroyed;
                u.UnitClicked -= SelectUnit;
                u.UnitMoved -= HandleUnitMovedVisualSync;
                UnitLifecycleManager.TerminateUnit(u, _unitRegistry, _pathfindingManager, _gridMapManager, _economyManager);
            }
        }
        _unitRegistry.Clear();
    }

    public UnitController? SpawnCustomUnit(string configId, int factionId, Vector2I gridPos, int hp = -1)
    {
        var unitScene = GD.Load<PackedScene>("res://scenes/gameplay/unit.tscn");
        if (unitScene == null) return null;

        var uCfg = GameConfigManager.GetUnitConfig(configId);
        bool isRanged = configId.Contains("cung_thu", StringComparison.OrdinalIgnoreCase);

        int maxHp = uCfg?.HpMax ?? 20;
        var data = new UnitData(
            id: configId,
            name: uCfg?.Name ?? "Chiến Binh",
            factionId: factionId,
            hpMax: maxHp,
            attack: uCfg?.Attack ?? 6,
            defense: uCfg?.Defense ?? 3,
            movementMax: uCfg?.MovementMax ?? 4,
            gridPosition: gridPos,
            upkeep: uCfg?.Upkeep ?? ResourceBundle.Zero,
            cost: uCfg?.Cost ?? ResourceBundle.Zero,
            description: uCfg?.Description ?? "",
            isRanged: isRanged,
            attackRange: isRanged ? 2 : 1
        );

        if (hp > 0 && hp < maxHp)
        {
            data.ApplyDamage(maxHp - hp);
        }

        var controller = unitScene.Instantiate<UnitController>();
        _unitContainer.AddChild(controller);
        controller.Bind(data);
        RegisterUnit(controller, FindFactionData(factionId));
        return controller;
    }
}
