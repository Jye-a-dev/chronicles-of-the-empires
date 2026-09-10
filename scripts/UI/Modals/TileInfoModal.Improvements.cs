using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.UI;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

public partial class TileInfoModal
{
    private void UpdateImprovementCard(HexCell cell)
    {
        if (_improvementCard == null) return;

        if (cell.TerrainData.Improvement != ImprovementType.None)
        {
            _improvementCard.Visible = true;
            string impName = cell.TerrainData.Improvement switch
            {
                ImprovementType.Farm => "Nông Trại",
                ImprovementType.Mine => "Mỏ Quặng",
                ImprovementType.LumberMill => "Trại Cưa",
                ImprovementType.Watchtower => "Tiêu Đồn",
                _ => cell.TerrainData.Improvement.ToString()
            };

            if (_featureNameLabel != null)
            {
                _featureNameLabel.Text = impName;
            }

            if (_featureStatusLabel != null)
            {
                if (cell.TerrainData.IsConstructed)
                {
                    _featureStatusLabel.Text = "[Đã hoàn tất]";
                    _featureStatusLabel.AddThemeColorOverride("font_color", PositiveYieldColor);
                }
                else
                {
                    _featureStatusLabel.Text = $"[Đang xây: còn {cell.TerrainData.ConstructionTurnsRemaining} lượt]";
                    _featureStatusLabel.AddThemeColorOverride("font_color", OrangeStatusColor);
                }
            }

            if (_featureYieldLabel != null)
            {
                _featureYieldLabel.Text = FormatBundle(cell.TerrainData.ImprovementBonusYield);
            }

            if (_btnBuildImprovement != null)
            {
                _btnBuildImprovement.Visible = false;
            }
        }
        else if (cell.Deposit != null)
        {
            _improvementCard.Visible = true;
            var dep = cell.Deposit;

            if (_featureNameLabel != null)
            {
                _featureNameLabel.Text = dep.Name;
            }

            if (_featureStatusLabel != null)
            {
                if (dep.IsExploited)
                {
                    _featureStatusLabel.Text = "[Đã khai thác]";
                    _featureStatusLabel.AddThemeColorOverride("font_color", PositiveYieldColor);
                }
                else
                {
                    _featureStatusLabel.Text = "[Chưa khai thác]";
                    _featureStatusLabel.AddThemeColorOverride("font_color", OrangeStatusColor);
                }
            }

            if (_featureYieldLabel != null)
            {
                _featureYieldLabel.Text = FormatBundle(dep.BonusYield);
            }

            // Configure compact action button
            if (!dep.IsExploited && cell.OwnerFactionId == 0)
            {
                _pendingImprovementId = dep.RequiredImprovement;
                var bldCfg = GameConfigManager.GetBuildingConfig(_pendingImprovementId);
                string bldName = bldCfg?.Name ?? dep.RequiredImprovement;

                if (_btnBuildLabel != null)
                {
                    _btnBuildLabel.Text = $"Xây {bldName}";
                }

                PopulateCostBadges(_btnBuildCostRow, bldCfg?.Cost ?? ResourceBundle.Zero);

                if (_btnBuildImprovement != null)
                {
                    _btnBuildImprovement.Visible = true;
                }
            }
            else if (_btnBuildImprovement != null)
            {
                _btnBuildImprovement.Visible = false;
            }
        }
        else if (cell.ConstructedBuilding != null)
        {
            _improvementCard.Visible = true;
            var bld = cell.ConstructedBuilding;

            if (_featureNameLabel != null)
            {
                _featureNameLabel.Text = bld.Name;
            }

            if (_featureStatusLabel != null)
            {
                _featureStatusLabel.Text = "[Công trình]";
                _featureStatusLabel.AddThemeColorOverride("font_color", PositiveYieldColor);
            }

            if (_featureYieldLabel != null)
            {
                _featureYieldLabel.Text = FormatBundle(bld.YieldBonus);
            }

            if (_btnBuildImprovement != null)
            {
                _btnBuildImprovement.Visible = false;
            }
        }
        else
        {
            _improvementCard.Visible = true;

            if (cell.OwnerFactionId == 0)
            {
                if (cell.TerrainData.Biome == BiomeType.River || cell.TerrainData.Biome == BiomeType.Ocean)
                {
                    if (_featureNameLabel != null) _featureNameLabel.Text = "Thủy vực tự nhiên";
                    if (_featureStatusLabel != null)
                    {
                        _featureStatusLabel.Text = "[Không thể xây dựng]";
                        _featureStatusLabel.AddThemeColorOverride("font_color", DimFlavorColor);
                    }
                    if (_featureYieldLabel != null) _featureYieldLabel.Text = "Khai thác tài nguyên mặt nước";
                    if (_btnBuildImprovement != null) _btnBuildImprovement.Visible = false;
                }
                else
                {
                    // Can build basic terrain improvement on friendly territory
                    var (defaultImp, impName, cost) = GetDefaultImprovementForTile(cell.TerrainData.Biome);
                    _pendingImprovementId = defaultImp.ToString();

                    if (_featureNameLabel != null)
                    {
                        _featureNameLabel.Text = "Địa hình tự nhiên";
                    }

                    if (_featureStatusLabel != null)
                    {
                        _featureStatusLabel.Text = "[Có thể khai thác]";
                        _featureStatusLabel.AddThemeColorOverride("font_color", DimBronzeColor);
                    }

                    if (_featureYieldLabel != null)
                    {
                        _featureYieldLabel.Text = "Chưa có công trình";
                    }

                    if (_btnBuildLabel != null)
                    {
                        _btnBuildLabel.Text = $"Xây {impName}";
                    }

                    PopulateCostBadges(_btnBuildCostRow, cost);

                    if (_btnBuildImprovement != null)
                    {
                        _btnBuildImprovement.Visible = true;
                    }
                }
            }
            else
            {
                if (_featureNameLabel != null)
                {
                    _featureNameLabel.Text = "Địa hình tự nhiên";
                }

                if (_featureStatusLabel != null)
                {
                    _featureStatusLabel.Text = "[Không có mỏ]";
                    _featureStatusLabel.AddThemeColorOverride("font_color", DimFlavorColor);
                }

                if (_featureYieldLabel != null)
                {
                    _featureYieldLabel.Text = "Không có sản lượng thưởng";
                }

                if (_btnBuildImprovement != null)
                {
                    _btnBuildImprovement.Visible = false;
                }
            }
        }
    }

    public static (ImprovementType Type, string Name, ResourceBundle Cost) GetDefaultImprovementForTile(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Forest => (ImprovementType.LumberMill, "Trại Gỗ", new ResourceBundle(0, 20, 10, 0, 0)),
            BiomeType.Mountain or BiomeType.Highlands or BiomeType.Hill => (ImprovementType.Mine, "Mỏ Quặng", new ResourceBundle(0, 30, 15, 0, 0)),
            _ => (ImprovementType.Farm, "Nông Trại", new ResourceBundle(15, 25, 0, 0, 0))
        };
    }

    private static void PopulateCostBadges(HBoxContainer? targetRow, in ResourceBundle cost)
    {
        if (targetRow == null) return;

        foreach (Node child in targetRow.GetChildren())
        {
            child.QueueFree();
        }

        if (cost.Food > 0)
        {
            targetRow.AddChild(CreateMiniCostBadge(ResourceType.Food, cost.Food));
        }
        if (cost.Production > 0)
        {
            targetRow.AddChild(CreateMiniCostBadge(ResourceType.Production, cost.Production));
        }
        if (cost.Gold > 0)
        {
            targetRow.AddChild(CreateMiniCostBadge(ResourceType.Gold, cost.Gold));
        }
    }

    private static HBoxContainer CreateMiniCostBadge(ResourceType type, int amount)
    {
        var badge = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        badge.AddThemeConstantOverride("separation", 1);

        var icon = IconManager.CreateResourceIconRect(type, 10);
        icon.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        badge.AddChild(icon);

        var lbl = new Label
        {
            Text = $"{amount}",
            MouseFilter = MouseFilterEnum.Ignore
        };
        lbl.AddThemeFontSizeOverride("font_size", 7);
        lbl.AddThemeColorOverride("font_color", GoldCostColor);
        badge.AddChild(lbl);

        return badge;
    }

    private void OnBuildImprovementPressed()
    {
        if (ActiveCell == null || string.IsNullOrEmpty(_pendingImprovementId)) return;
        EmitSignal(SignalName.UpgradeRequested, _pendingImprovementId, ActiveCell.Coords);
    }

    private static string FormatBundle(in ResourceBundle bundle)
    {
        var parts = new List<string>();
        if (bundle.Food > 0) parts.Add($"+{bundle.Food} Lương");
        if (bundle.Production > 0) parts.Add($"+{bundle.Production} Sản xuất");
        if (bundle.Gold > 0) parts.Add($"+{bundle.Gold} Vàng");
        if (bundle.Science > 0) parts.Add($"+{bundle.Science} Khoa học");
        if (bundle.Faith > 0) parts.Add($"+{bundle.Faith} Tín ngưỡng");
        return parts.Count > 0 ? string.Join(", ", parts) : "Không";
    }
}

