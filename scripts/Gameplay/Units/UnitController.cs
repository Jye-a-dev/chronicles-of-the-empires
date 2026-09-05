using System;
using System.Collections.Generic;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Gameplay.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Tactical combat unit controller managing stats, movement budgets, morale states, and input hit detection.
/// </summary>
public partial class UnitController : CharacterBody2D
{
    [Signal] public delegate void UnitSelectedEventHandler(UnitController unit);
    [Signal] public delegate void UnitMovedEventHandler(UnitController unit, Vector2I oldPos, Vector2I newPos);
    [Signal] public delegate void MovementDepletedEventHandler(UnitController unit);
    [Signal] public delegate void UnitSurrenderedEventHandler(UnitController unit);
    [Signal] public delegate void UnitRecapturedEventHandler(UnitController unit, int previousFaction, int newFaction);

    [Export] public string UnitName = "Chiến Binh Văn Lang";
    [Export] public string Description = "Binh chủng thiện chiến bảo vệ bờ cõi.";
    [Export] public string UnitConfigId = "";
    [Export] public int HpMax = 20;
    [Export] public int Attack = 6;
    [Export] public int Defense = 3;
    [Export] public int MovementRangeMax = 4;
    [Export] public int FactionId = 0; // 0: Player, 1: Rival
    [Export] public int MoraleMax = 100;

    public int HpCurrent { get; set; } = 20;
    public int MoraleCurrent { get; set; } = 100;
    public int OriginalFactionId { get; set; } = 0;
    public bool IsSurrendered { get; set; } = false;
    public int SurrenderTurnsRemaining { get; set; } = 2;
    public int MovementRangeRemaining { get; set; } = 4;
    public Vector2I GridPosition { get; set; } = Vector2I.Zero;
    public bool IsSelected { get; private set; } = false;
    public bool IsMoving { get; private set; } = false;

    private ReferenceRect? _selectionBorder;
    private ColorRect? _visualRect;
    private ColorRect? _borderRect;
    private Label? _unitLabel;
    private Label? _moraleFlagLabel;
    private Tween? _surrenderTween;
    private Tween? _flagBobTween;

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
            MoraleCurrent = MoraleMax;
            OriginalFactionId = FactionId;
            MovementRangeRemaining = MovementRangeMax;
            RefreshVisuals();
        }

        _selectionBorder = GetNodeOrNull<ReferenceRect>("SelectionBorder");
        if (_selectionBorder != null) _selectionBorder.Visible = false;

        EnsureMoraleFlagVisual();
        QueueRedraw();
    }

    private void EnsureMoraleFlagVisual()
    {
        if (_moraleFlagLabel != null && IsInstanceValid(_moraleFlagLabel)) return;

        _moraleFlagLabel = new Label
        {
            Name = "MoraleFlagLabel",
            Text = "🏳️",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = new Vector2(-10, -25),
            Size = new Vector2(20, 16),
            Visible = IsSurrendered,
            ZIndex = 12
        };
        _moraleFlagLabel.AddThemeFontSizeOverride("font_size", 12);
        AddChild(_moraleFlagLabel);

        if (IsSurrendered)
        {
            StartFlagBobbing();
        }
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
            OriginalFactionId = cfg.FactionId;
            HpMax = cfg.HpMax;
            HpCurrent = cfg.HpMax;
            MoraleCurrent = MoraleMax;
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

        if (IsSurrendered)
        {
            if (_moraleFlagLabel != null)
            {
                _moraleFlagLabel.Visible = true;
                StartFlagBobbing();
            }

            if (_visualRect != null)
            {
                _visualRect.Color = new Color("#3c3c3c");
            }

            if (_unitLabel != null)
            {
                _unitLabel.Text = "🏳";
                _unitLabel.AddThemeColorOverride("font_color", new Color("#dddddd"));
            }

            if (_borderRect != null && (_surrenderTween == null || !_surrenderTween.IsValid()))
            {
                _surrenderTween = CreateTween().SetLoops();
                _surrenderTween.TweenProperty(_borderRect, "color", new Color("#ffffff"), 0.35);
                _surrenderTween.TweenProperty(_borderRect, "color", new Color("#7a7a7a"), 0.35);
            }
            return;
        }

        StopSurrenderVisuals();

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

    private void StartFlagBobbing()
    {
        if (_moraleFlagLabel == null) return;
        _flagBobTween?.Kill();
        _flagBobTween = CreateTween().SetLoops();
        _flagBobTween.TweenProperty(_moraleFlagLabel, "position:y", -28.0f, 0.45).SetTrans(Tween.TransitionType.Sine);
        _flagBobTween.TweenProperty(_moraleFlagLabel, "position:y", -24.0f, 0.45).SetTrans(Tween.TransitionType.Sine);
    }

    private void StopSurrenderVisuals()
    {
        _surrenderTween?.Kill();
        _surrenderTween = null;
        _flagBobTween?.Kill();
        _flagBobTween = null;

        if (_moraleFlagLabel != null)
        {
            _moraleFlagLabel.Visible = false;
        }
    }

    public void ModifyMorale(int delta)
    {
        MoraleCurrent = Mathf.Clamp(MoraleCurrent + delta, 0, MoraleMax);

        if (MoraleCurrent <= 0 && !IsSurrendered)
        {
            TriggerSurrender();
        }
        else if (MoraleCurrent < 20 && !IsSurrendered)
        {
            PlayLowMoraleJitter();
        }
    }

    private void TriggerSurrender()
    {
        IsSurrendered = true;
        SurrenderTurnsRemaining = 2;
        MovementRangeRemaining = 0;

        EnsureMoraleFlagVisual();
        RefreshVisuals();
        QueueRedraw();
        EmitSignal(SignalName.UnitSurrendered, this);
    }

    public bool TryInteractRecapture(UnitController interactor, EconomyManager economy, FactionData interactorFaction)
    {
        if (!IsSurrendered) return false;

        bool isOriginalOwner = interactorFaction.FactionId == OriginalFactionId;
        var cost = isOriginalOwner
            ? new ResourceBundle(15, 0, 10, 0, 0)
            : new ResourceBundle(0, 0, 10, 0, 0);

        if (!interactorFaction.Treasury.HasEnough(cost)) return false;

        interactorFaction.Treasury -= cost;

        int previousFaction = FactionId;
        if (isOriginalOwner)
        {
            FactionId = OriginalFactionId;
            MoraleCurrent = 40;
        }
        else
        {
            FactionId = interactorFaction.FactionId;
            MoraleCurrent = 30;
        }

        IsSurrendered = false;
        SurrenderTurnsRemaining = 2;
        MovementRangeRemaining = 0;

        RefreshVisuals();
        PlayRecaptureFlash();
        QueueRedraw();

        EmitSignal(SignalName.UnitRecaptured, this, previousFaction, FactionId);
        return true;
    }

    public void ProcessEndTurnSurrender(List<UnitController> allUnits)
    {
        if (!IsSurrendered) return;

        SurrenderTurnsRemaining--;

        if (SurrenderTurnsRemaining <= 0)
        {
            UnitController? nearestRival = null;
            float minWorldDist = float.MaxValue;
            float maxRange = 3.3f * GridMapManager.CellDimension;

            foreach (var unit in allUnits)
            {
                if (unit == this || unit.IsSurrendered || unit.FactionId == OriginalFactionId) continue;
                float dist = Position.DistanceTo(unit.Position);
                if (dist <= maxRange && dist < minWorldDist)
                {
                    minWorldDist = dist;
                    nearestRival = unit;
                }
            }

            if (nearestRival != null)
            {
                int prevFaction = FactionId;
                FactionId = nearestRival.FactionId;
                MoraleCurrent = 25;
                IsSurrendered = false;
                SurrenderTurnsRemaining = 2;

                RefreshVisuals();
                PlayRecaptureFlash();
                QueueRedraw();

                EmitSignal(SignalName.UnitRecaptured, this, prevFaction, FactionId);
            }
        }
    }

    private void PlayLowMoraleJitter()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "position", Position + new Vector2(1.5f, 0), 0.05);
        tween.TweenProperty(this, "position", Position - new Vector2(1.5f, 0), 0.05);
        tween.TweenProperty(this, "position", Position, 0.05);
    }

    private void PlayRecaptureFlash()
    {
        if (_borderRect == null) return;
        var tween = CreateTween();
        tween.TweenProperty(_borderRect, "scale", new Vector2(1.25f, 1.25f), 0.12);
        tween.TweenProperty(_borderRect, "scale", Vector2.One, 0.12);
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
        Color gemColor;
        if (IsSurrendered)
        {
            gemColor = new Color("#ffffff");
        }
        else
        {
            bool hasMoves = MovementRangeRemaining > 0;
            gemColor = hasMoves ? new Color("#4de890") : new Color("#666666");
        }
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
        if (IsSurrendered || path.Length <= 1 || IsMoving) return;

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
        if (IsSurrendered)
        {
            MovementRangeRemaining = 0;
            QueueRedraw();
            return;
        }

        MovementRangeRemaining = MovementRangeMax;
        QueueRedraw();
    }

    public void SnapToGrid(Vector2I gridPos)
    {
        GridPosition = gridPos;
        Position = GridMapManager.GridToWorldCenter(gridPos);
    }
}

