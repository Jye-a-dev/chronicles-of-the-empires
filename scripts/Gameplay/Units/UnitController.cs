using System;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Tactical combat unit controller managing stats, movement budgets, and input hit detection.
/// </summary>
public partial class UnitController : CharacterBody2D
{
    [Signal] public delegate void UnitSelectedEventHandler(UnitController unit);
    [Signal] public delegate void UnitMovedEventHandler(UnitController unit, Vector2I oldPos, Vector2I newPos);
    [Signal] public delegate void MovementDepletedEventHandler(UnitController unit);

    [Export] public string UnitName = "Chiến Binh Văn Lang";
    [Export] public string Description = "Binh chủng thiện chiến bảo vệ bờ cõi.";
    [Export] public string UnitConfigId = "";
    [Export] public int HpMax = 20;
    [Export] public int Attack = 6;
    [Export] public int Defense = 3;
    [Export] public int MovementRangeMax = 4;
    [Export] public int FactionId = 0; // 0: Player, 1: Rival

    public int HpCurrent { get; set; } = 20;
    public int MovementRangeRemaining { get; set; } = 4;
    public Vector2I GridPosition { get; set; } = Vector2I.Zero;
    public bool IsSelected { get; private set; } = false;
    public bool IsMoving { get; private set; } = false;

    private ReferenceRect? _selectionBorder;
    private ColorRect? _visualRect;
    private ColorRect? _borderRect;
    private Label? _unitLabel;

    public override void _Ready()
    {
        InputPickable = true;

        if (!string.IsNullOrEmpty(UnitConfigId))
        {
            ApplyConfig(UnitConfigId);
        }
        else
        {
            HpCurrent = HpMax;
            MovementRangeRemaining = MovementRangeMax;
            RefreshVisuals();
        }

        _selectionBorder = GetNodeOrNull<ReferenceRect>("SelectionBorder");
        if (_selectionBorder != null) _selectionBorder.Visible = false;
        QueueRedraw();
    }

    public void ApplyConfig(string configId)
    {
        UnitConfigId = configId;
        var cfg = ChroniclesOfTheEmpires.Core.Config.GameConfigManager.GetUnitConfig(configId);
        if (cfg != null)
        {
            UnitName = cfg.Name;
            Description = cfg.Description;
            FactionId = cfg.FactionId;
            HpMax = cfg.HpMax;
            HpCurrent = cfg.HpMax;
            Attack = cfg.Attack;
            Defense = cfg.Defense;
            MovementRangeMax = cfg.MovementMax;
            MovementRangeRemaining = cfg.MovementMax;
        }

        RefreshVisuals();
        QueueRedraw();
    }

    public void RefreshVisuals()
    {
        _borderRect = GetNodeOrNull<ColorRect>("VisualBorder");
        _visualRect = GetNodeOrNull<ColorRect>("VisualBorder/Visual") ?? GetNodeOrNull<ColorRect>("Visual");
        _unitLabel = GetNodeOrNull<Label>("UnitLabel");

        var cfg = !string.IsNullOrEmpty(UnitConfigId) ? ChroniclesOfTheEmpires.Core.Config.GameConfigManager.GetUnitConfig(UnitConfigId) : null;

        Color borderColor = cfg?.BorderColor ?? (FactionId == 0 ? new Color("#f5c842") : new Color("#e03b24"));
        Color bodyColor = cfg?.VisualColor ?? (FactionId == 0 ? new Color("#2d241e") : new Color("#2e1614"));
        string symbol = cfg?.Symbol ?? (FactionId == 0 ? "★" : "◆");

        if (_borderRect != null)
        {
            _borderRect.Color = borderColor;
        }

        if (_visualRect != null)
        {
            _visualRect.Color = bodyColor;
        }

        if (_unitLabel != null)
        {
            _unitLabel.Text = symbol;
            _unitLabel.AddThemeColorOverride("font_color", borderColor);
        }
    }

    public override void _Draw()
    {
        // 1. Drop shadow: flattened ellipse under feet to anchor unit to ground
        Vector2 shadowCenter = new(0f, 6.5f);
        Color shadowColor = new(0f, 0f, 0f, 0.42f);
        // Approximate 16-segment flattened ellipse
        const int segments = 16;
        var shadowPoly = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.Tau / segments;
            shadowPoly[i] = shadowCenter + new Vector2(Mathf.Cos(angle) * 8.5f, Mathf.Sin(angle) * 3.8f);
        }
        DrawColoredPolygon(shadowPoly, shadowColor);

        // 2. Movement / turn readiness gem on head
        Vector2 gemCenter = new(0f, -12.5f);
        bool hasMoves = MovementRangeRemaining > 0;
        Color gemColor = hasMoves ? new Color("#4de890") : new Color("#666666");
        DrawCircle(gemCenter, 2.5f, gemColor);
        DrawArc(gemCenter, 2.5f, 0, Mathf.Tau, 12, new Color(0.08f, 0.08f, 0.08f, 0.85f), 0.8f);
    }

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            EmitSignal(SignalName.UnitSelected, this);
            GetViewport().SetInputAsHandled(); // Prevent map ground-click raycast
        }
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (_selectionBorder != null)
        {
            _selectionBorder.Visible = selected;
        }
    }

    public void MoveAlongPath(Vector2I[] path, int totalCost, Action? onComplete = null)
    {
        if (path.Length <= 1 || IsMoving) return;

        IsMoving = true;
        var tween = CreateTween();

        // Step-by-step tile movement animation
        for (int i = 1; i < path.Length; i++)
        {
            Vector2 targetWorld = GridMapManager.GridToWorldCenter(path[i]);
            tween.TweenProperty(this, "position", targetWorld, 0.12);
        }

        tween.Finished += () =>
        {
            var oldPos = GridPosition;
            GridPosition = path[^1];
            Position = GridMapManager.GridToWorldCenter(GridPosition); // Snap to exact pixel center
            MovementRangeRemaining = Math.Max(0, MovementRangeRemaining - totalCost);
            IsMoving = false;
            QueueRedraw();

            EmitSignal(SignalName.UnitMoved, this, oldPos, GridPosition);
            if (MovementRangeRemaining == 0)
            {
                EmitSignal(SignalName.MovementDepleted, this);
            }
            onComplete?.Invoke();
        };
    }

    public void ResetTurnMovement()
    {
        MovementRangeRemaining = MovementRangeMax;
        QueueRedraw();
    }

    public void SnapToGrid(Vector2I gridPos)
    {
        GridPosition = gridPos;
        Position = GridMapManager.GridToWorldCenter(gridPos);
    }
}

