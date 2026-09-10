using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.UI;
using ChroniclesOfTheEmpires.Gameplay;

#nullable enable

namespace ChroniclesOfTheEmpires.UI.Modals;

public partial class TileInfoModal
{
    private void UpdateRecruitmentSection(HexCell cell)
    {
        if (_recruitSection == null || _recruitGrid == null) return;

        bool isPlayerTerritory = cell.OwnerFactionId == 0;
        _recruitSection.Visible = isPlayerTerritory;

        if (!isPlayerTerritory) return;

        // Clear existing recruit buttons
        foreach (Node child in _recruitGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Dynamically instantiate recruit cards for Player Faction (Faction 0)
        var unitConfigs = GameConfigManager.GetAllUnitConfigs();
        int instantiated = 0;

        foreach (var (_, unit) in unitConfigs)
        {
            if (unit.FactionId != 0) continue;

            var card = CreateRecruitCard(unit);
            _recruitGrid.AddChild(card);
            instantiated++;
        }

        // Fallback if configs were not loaded
        if (instantiated == 0)
        {
            _recruitGrid.AddChild(CreateFallbackRecruitCard("cam_ve_quan", "Cấm Vệ", 30, 50, 25));
            _recruitGrid.AddChild(CreateFallbackRecruitCard("cung_thu", "Cung Thủ", 20, 45, 20));
        }
    }

    private Button CreateRecruitCard(UnitConfig unit)
    {
        var btn = new Button
        {
            CustomMinimumSize = new Vector2(100, 28),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            ClipText = false
        };

        ApplyRecruitButtonStyle(btn);

        var content = new HBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            OffsetLeft = 3,
            OffsetTop = 1,
            OffsetRight = -3,
            OffsetBottom = -1
        };
        content.AddThemeConstantOverride("separation", 3);

        // Left: 16x16 px unit icon
        var unitTexture = UnitTextureManager.GetUnitTexture(unit.Id, 0)
            ?? IconManager.GetIcon("recruit");
        var sprite = new TextureRect
        {
            Texture = unitTexture,
            CustomMinimumSize = new Vector2(16, 16),
            Size = new Vector2(16, 16),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter = TextureFilterEnum.Nearest,
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        content.AddChild(sprite);

        // Right: Name and Cost Row
        var rightCol = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        rightCol.AddThemeConstantOverride("separation", 1);

        var nameLabel = new Label
        {
            Text = unit.Name,
            MouseFilter = MouseFilterEnum.Ignore,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 8);
        nameLabel.AddThemeColorOverride("font_color", OffWhiteColor);
        rightCol.AddChild(nameLabel);

        var costRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        costRow.AddThemeConstantOverride("separation", 2);

        if (unit.Cost.Food > 0)
        {
            costRow.AddChild(CreateMiniCostBadge(ResourceType.Food, unit.Cost.Food));
        }
        if (unit.Cost.Production > 0)
        {
            costRow.AddChild(CreateMiniCostBadge(ResourceType.Production, unit.Cost.Production));
        }
        if (unit.Cost.Gold > 0)
        {
            costRow.AddChild(CreateMiniCostBadge(ResourceType.Gold, unit.Cost.Gold));
        }

        rightCol.AddChild(costRow);
        content.AddChild(rightCol);
        btn.AddChild(content);

        // 1px upward offset on hover for tactile pixel feedback
        btn.MouseEntered += () => content.Position = new Vector2(0, -1);
        btn.MouseExited += () => content.Position = Vector2.Zero;

        string unitId = unit.Id;
        btn.Pressed += () =>
        {
            if (ActiveCell != null)
            {
                EmitSignal(SignalName.RecruitRequested, unitId, ActiveCell.Coords);
            }
        };

        return btn;
    }

    private Button CreateFallbackRecruitCard(string unitId, string unitName, int food, int prod, int gold)
    {
        var dummyCost = new ResourceBundle(food, prod, gold, 0, 0);
        var dummyConfig = new UnitConfig(
            unitId,
            unitName,
            "",
            0,
            20,
            5,
            3,
            3,
            dummyCost,
            ResourceBundle.Zero,
            "★",
            new Color("#2d241e"),
            GoldColor
        );
        return CreateRecruitCard(dummyConfig);
    }

    private static void ApplyRecruitButtonStyle(Button btn)
    {
        var normalStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.1176f, 0.0941f, 0.0784f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(0.3294f, 0.2353f, 0.0863f, 1f),
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 3,
            ContentMarginRight = 3,
            ContentMarginTop = 2,
            ContentMarginBottom = 2
        };

        var hoverStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.1804f, 0.1373f, 0.0863f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = GoldColor,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 3,
            ContentMarginRight = 3,
            ContentMarginTop = 1,
            ContentMarginBottom = 3
        };

        var pressedStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.0706f, 0.0549f, 0.0392f, 1f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = DimBronzeColor,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusBottomLeft = 0,
            ContentMarginLeft = 3,
            ContentMarginRight = 3,
            ContentMarginTop = 3,
            ContentMarginBottom = 1
        };

        btn.AddThemeStyleboxOverride("normal", normalStyle);
        btn.AddThemeStyleboxOverride("hover", hoverStyle);
        btn.AddThemeStyleboxOverride("pressed", pressedStyle);
        btn.AddThemeStyleboxOverride("focus", hoverStyle);
    }
}

