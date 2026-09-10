using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Economy;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Presentation layer controller for a tactical military unit.
/// Inherits Node2D purely for rendering, animations, and visual tweening.
/// Delegates all state and domain logic strictly to UnitData.
/// Visuals, animations, and movement logic are modularized into partial components.
/// </summary>
public partial class UnitController : Node2D
{
    [Signal] public delegate void UnitMovedEventHandler(UnitController unit, Vector2I oldPos, Vector2I newPos);
    [Signal] public delegate void MovementDepletedEventHandler(UnitController unit);
    [Signal] public delegate void UnitDestroyedEventHandler(UnitController unit);
    [Signal] public delegate void UnitClickedEventHandler(UnitController unit);

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

    private Polygon2D? _hexHitboxIndicator;
    private ColorRect? _visualRect;
    private ColorRect? _borderRect;
    private Label? _unitLabel;
    private Sprite2D? _surrenderFlagSprite;
    private Sprite2D? _unitSprite;

    private Tween? _surrenderTween;
    private Tween? _flagBobTween;
    private Tween? _movementTween;
    private Tween? _selectionBounceTween;

    public override void _Ready()
    {
        _hexHitboxIndicator = GetNodeOrNull<Polygon2D>("HexHitboxIndicator");
        if (_hexHitboxIndicator != null) _hexHitboxIndicator.Visible = false;

        _borderRect = GetNodeOrNull<ColorRect>("VisualBorder");
        _visualRect = GetNodeOrNull<ColorRect>("VisualBorder/Visual") ?? GetNodeOrNull<ColorRect>("Visual");
        _unitLabel = GetNodeOrNull<Label>("UnitLabel");

        EnsureUnitSprite();
        EnsureMoraleFlagVisual();
        QueueRedraw();
    }

    public void Bind(UnitData data)
    {
        if (Data != null)
        {
            Unbind();
        }

        Data = data;
        Data.OnHpChanged += HandleHpChanged;
        Data.OnMoraleChanged += HandleMoraleChanged;
        Data.OnSurrenderStateChanged += HandleSurrenderState;
        Data.OnFactionChanged += HandleFactionChanged;
        Data.OnDestroyed += HandleDestroyed;

        SnapToGrid(data.GridPosition);
        UpdateUnitTexture();
        RefreshVisuals();
        QueueRedraw();
    }

    public void Unbind()
    {
        if (Data != null)
        {
            UnbindEvents(Data);
        }
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
        CancelMovement();
        if (Data != null)
        {
            Unbind();
        }

        _surrenderTween?.Kill();
        _flagBobTween?.Kill();
        _selectionBounceTween?.Kill();
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
        UpdateUnitTexture();
        RefreshVisuals();
        PlayRecaptureFlash();
        QueueRedraw();
    }

    private void HandleDestroyed()
    {
        _movementTween?.Kill();
        _surrenderTween?.Kill();
        _flagBobTween?.Kill();

        Unbind();
        EmitSignal(SignalName.UnitDestroyed, this);
        QueueFree();
    }

    public void SetSelected(bool isSelected)
    {
        IsSelected = isSelected;

        _hexHitboxIndicator ??= GetNodeOrNull<Polygon2D>("HexHitboxIndicator");
        if (_hexHitboxIndicator != null)
        {
            _hexHitboxIndicator.Visible = isSelected;
        }

        _selectionBounceTween?.Kill();
        if (isSelected)
        {
            _selectionBounceTween = CreateTween();
            _selectionBounceTween.TweenProperty(this, "scale", new Vector2(1.1f, 1.1f), 0.08)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            _selectionBounceTween.TweenProperty(this, "scale", Vector2.One, 0.08)
                .SetTrans(Tween.TransitionType.Linear);
        }
        else
        {
            Scale = Vector2.One;
        }
    }
}
