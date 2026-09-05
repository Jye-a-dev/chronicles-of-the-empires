using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Presentation layer controller for a tactical military unit.
/// Inherits Node2D purely for rendering, animations, and visual tweening.
/// Delegates all state and domain logic strictly to UnitData.
/// </summary>
public partial class UnitController : Node2D
{
    [Signal] public delegate void UnitMovedEventHandler(UnitController unit, Vector2I oldPos, Vector2I newPos);
    [Signal] public delegate void MovementDepletedEventHandler(UnitController unit);
    [Signal] public delegate void UnitDestroyedEventHandler(UnitController unit);

    public UnitData Data { get; private set; } = null!;

    // Forwarding accessors directly to Data Single Source of Truth
    public string UnitName => Data?.Name ?? "Chiến Binh";
    public string Description => Data?.Description ?? "";
    public string UnitConfigId => Data?.Id ?? "";
    public int FactionId => Data?.FactionId ?? 0;
    public int OriginalFactionId => Data?.OriginalFactionId ?? 0;
    public int HpMax => Data?.HpMax ?? 20;
    public int HpCurrent => Data?.CurrentHp ?? 20;
    public int Attack => Data?.Attack ?? 5;
    public int Defense => Data?.Defense ?? 2;
    public int MovementRangeMax => Data?.MovementMax ?? 4;
    public int MovementRangeRemaining => Data?.MovementRemaining ?? 0;
    public int MoraleMax => Data?.MoraleMax ?? 100;
    public int MoraleCurrent => Data?.MoraleCurrent ?? 100;
    public bool IsSurrendered => Data?.IsSurrendered ?? false;
    public int SurrenderTurnsRemaining => Data?.SurrenderTurnsRemaining ?? 2;
    public Vector2I GridPosition => Data?.GridPosition ?? Vector2I.Zero;
    public bool IsMoving => Data?.IsMoving ?? false;
    public bool IsSelected { get; private set; } = false;

    private ReferenceRect? _selectionBorder;
    private ColorRect? _visualRect;
    private ColorRect? _borderRect;
    private Label? _unitLabel;
    private Label? _moraleFlagLabel;

    private Tween? _surrenderTween;
    private Tween? _flagBobTween;
    private Tween? _movementTween;

    public override void _Ready()
    {
        _selectionBorder = GetNodeOrNull<ReferenceRect>("SelectionBorder");
        if (_selectionBorder != null) _selectionBorder.Visible = false;

        _borderRect = GetNodeOrNull<ColorRect>("VisualBorder");
        _visualRect = GetNodeOrNull<ColorRect>("VisualBorder/Visual") ?? GetNodeOrNull<ColorRect>("Visual");
        _unitLabel = GetNodeOrNull<Label>("UnitLabel");

        EnsureMoraleFlagVisual();
        QueueRedraw();
    }

    public void Bind(UnitData data)
    {
        if (Data != null)
        {
            UnbindEvents(Data);
        }

        Data = data;
        Data.OnHpChanged += HandleHpChanged;
        Data.OnMoraleChanged += HandleMoraleChanged;
        Data.OnSurrenderStateChanged += HandleSurrenderState;
        Data.OnFactionChanged += HandleFactionChanged;
        Data.OnDestroyed += HandleDestroyed;

        SnapToGrid(data.GridPosition);
        RefreshVisuals();
        QueueRedraw();
    }

    private void UnbindEvents(UnitData data)
    {
        data.OnHpChanged -= HandleHpChanged;
        data.OnMoraleChanged -= HandleMoraleChanged;
        data.OnSurrenderStateChanged -= HandleSurrenderState;
        data.OnFactionChanged -= HandleFactionChanged;
        data.OnDestroyed -= HandleDestroyed;
    }

    public override void _ExitTree()
    {
        if (Data != null)
        {
            // Fallback emergency snap if tree was exited during movement
            if (Data.IsMoving)
            {
                Position = GridMapManager.GridToWorldCenter(Data.GridPosition);
                Data.IsMoving = false;
            }
            UnbindEvents(Data);
        }

        _movementTween?.Kill();
        _surrenderTween?.Kill();
        _flagBobTween?.Kill();
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

    public void RefreshVisuals()
    {
        _borderRect ??= GetNodeOrNull<ColorRect>("VisualBorder");
        _visualRect ??= GetNodeOrNull<ColorRect>("VisualBorder/Visual") ?? GetNodeOrNull<ColorRect>("Visual");
        _unitLabel ??= GetNodeOrNull<Label>("UnitLabel");

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

        var cfg = !string.IsNullOrEmpty(UnitConfigId) ? GameConfigManager.GetUnitConfig(UnitConfigId) : null;
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

    private void HandleHpChanged(int currentHp, int maxHp)
    {
        PlayHitFlash();
        QueueRedraw();
    }

    private void HandleMoraleChanged(int currentMorale, int maxMorale)
    {
        if (currentMorale < 20 && !IsSurrendered)
        {
            PlayLowMoraleJitter();
        }
        QueueRedraw();
    }

    private void HandleSurrenderState(bool surrendered)
    {
        RefreshVisuals();
        QueueRedraw();
    }

    private void HandleFactionChanged(int newFactionId)
    {
        RefreshVisuals();
        PlayRecaptureFlash();
        QueueRedraw();
    }

    private void HandleDestroyed()
    {
        _movementTween?.Kill();
        _surrenderTween?.Kill();
        _flagBobTween?.Kill();

        EmitSignal(SignalName.UnitDestroyed, this);
        QueueFree();
    }

    private void PlayHitFlash()
    {
        if (_visualRect == null) return;
        var originalColor = _visualRect.Color;
        var tween = CreateTween();
        tween.TweenProperty(_visualRect, "color", new Color(1f, 0.2f, 0.2f), 0.08);
        tween.TweenProperty(_visualRect, "color", originalColor, 0.12);
    }

    private void PlayLowMoraleJitter()
    {
        var tween = CreateTween();
        Vector2 origin = GridMapManager.GridToWorldCenter(GridPosition);
        tween.TweenProperty(this, "position", origin + new Vector2(1.5f, 0), 0.05);
        tween.TweenProperty(this, "position", origin - new Vector2(1.5f, 0), 0.05);
        tween.TweenProperty(this, "position", origin, 0.05);
    }

    public void PlayRecaptureFlash()
    {
        if (_borderRect == null) return;
        var tween = CreateTween();
        tween.TweenProperty(_borderRect, "scale", new Vector2(1.25f, 1.25f), 0.12);
        tween.TweenProperty(_borderRect, "scale", Vector2.One, 0.12);
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
        if (Data == null || IsSurrendered || path.Length <= 1 || IsMoving) return;

        Data.IsMoving = true;
        _movementTween?.Kill();
        _movementTween = CreateTween();

        Vector2I oldPos = GridPosition;

        // Step-by-step tile movement animation
        for (int i = 1; i < path.Length; i++)
        {
            Vector2 targetWorld = GridMapManager.GridToWorldCenter(path[i]);
            _movementTween.TweenProperty(this, "position", targetWorld, 0.12);
        }

        _movementTween.Finished += () =>
        {
            Data.MovementRemaining = Math.Max(0, Data.MovementRemaining - totalCost);
            Data.IsMoving = false;
            Position = GridMapManager.GridToWorldCenter(Data.GridPosition); // Snap strictly to exact cell center
            QueueRedraw();

            EmitSignal(SignalName.UnitMoved, this, oldPos, Data.GridPosition);
            if (Data.MovementRemaining == 0)
            {
                EmitSignal(SignalName.MovementDepleted, this);
            }
            onComplete?.Invoke();
        };
    }

    public void ResetTurnMovement()
    {
        Data?.ResetTurnMovement();
        QueueRedraw();
    }

    public void SnapToGrid(Vector2I gridPos)
    {
        if (Data != null)
        {
            Data.GridPosition = gridPos;
        }
        Position = GridMapManager.GridToWorldCenter(gridPos);
    }

    public override void _Draw()
    {
        // 1. Drop shadow: flattened ellipse under feet to anchor unit to ground
        Vector2 shadowCenter = new(0f, 6.5f);
        Color shadowColor = new(0f, 0f, 0f, 0.42f);
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
}
