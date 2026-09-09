using System;
using Godot;
using ChroniclesOfTheEmpires.Core.Config;
using ChroniclesOfTheEmpires.Core.Economy;
using ChroniclesOfTheEmpires.Core.UI;

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

    private static readonly Vector2[] CachedShadowPoly = new Vector2[16];

    static UnitController()
    {
        Vector2 shadowCenter = new(0f, 6.0f);
        for (int i = 0; i < 16; i++)
        {
            float angle = i * Mathf.Tau / 16;
            CachedShadowPoly[i] = shadowCenter + new Vector2(Mathf.Cos(angle) * 5.2f, Mathf.Sin(angle) * 2.2f);
        }
    }

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

    private void EnsureMoraleFlagVisual()
    {
        if (_surrenderFlagSprite != null && IsInstanceValid(_surrenderFlagSprite)) return;

        var flagTexture = IconManager.GetIcon("flag_surrender");
        _surrenderFlagSprite = new Sprite2D
        {
            Name = "SurrenderFlagSprite",
            Texture = flagTexture,
            TextureFilter = TextureFilterEnum.Nearest,
            Position = new Vector2(0, -42),
            Visible = IsSurrendered,
            ZIndex = 12
        };

        float w = flagTexture.GetWidth();
        float h = flagTexture.GetHeight();
        if (w > 0 && h > 0)
        {
            _surrenderFlagSprite.Scale = new Vector2(16f / w, 16f / h);
        }

        AddChild(_surrenderFlagSprite);

        if (IsSurrendered)
        {
            StartFlagBobbing();
        }
    }

    private void EnsureUnitSprite()
    {
        _unitSprite ??= GetNodeOrNull<Sprite2D>("Sprite2D");
        if (_unitSprite == null)
        {
            _unitSprite = new Sprite2D
            {
                Name = "Sprite2D",
                TextureFilter = TextureFilterEnum.Nearest,
                ZIndex = 1
            };
            AddChild(_unitSprite);
        }
        else
        {
            _unitSprite.TextureFilter = TextureFilterEnum.Nearest;
        }
    }

    public void UpdateUnitTexture()
    {
        if (Data == null) return;
        EnsureUnitSprite();
        if (_unitSprite == null) return;

        var texture = UnitTextureManager.GetUnitTexture(Data.UnitType, Data.FactionId);
        if (texture != null)
        {
            _unitSprite.Texture = texture;
            _unitSprite.TextureFilter = TextureFilterEnum.Nearest;
            _unitSprite.Visible = true;

            // Scale sprite to 1.5x tile dimension (48px) and nudge upward by 20px
            float targetDim = GridMapManager.CellDimension * 1.50f;
            float w = texture.GetWidth();
            float h = texture.GetHeight();
            if (w > 0 && h > 0)
            {
                _unitSprite.Scale = new Vector2(targetDim / w, targetDim / h);
                _unitSprite.Position = new Vector2(0f, -20.0f);
            }

            // Hide fallback geometry when high-res/pixel art sprite is active
            if (_visualRect != null) _visualRect.Visible = false;
            if (_borderRect != null) _borderRect.Visible = false;
            if (_unitLabel != null) _unitLabel.Visible = false;
        }
        else
        {
            _unitSprite.Visible = false;
            if (_visualRect != null) _visualRect.Visible = true;
            if (_borderRect != null) _borderRect.Visible = true;
            if (_unitLabel != null) _unitLabel.Visible = true;
        }
    }

    public void RefreshVisuals()
    {
        _borderRect ??= GetNodeOrNull<ColorRect>("VisualBorder");
        _visualRect ??= GetNodeOrNull<ColorRect>("VisualBorder/Visual") ?? GetNodeOrNull<ColorRect>("Visual");
        _unitLabel ??= GetNodeOrNull<Label>("UnitLabel");
        EnsureUnitSprite();

        if (IsSurrendered)
        {
            if (_unitSprite != null)
            {
                _unitSprite.Modulate = new Color(0.55f, 0.55f, 0.55f, 0.85f);
            }

            EnsureMoraleFlagVisual();
            if (_surrenderFlagSprite != null)
            {
                _surrenderFlagSprite.Visible = true;
                StartFlagBobbing();
            }

            if (_visualRect != null)
            {
                _visualRect.Color = new Color("#3c3c3c");
            }

            if (_unitLabel != null)
            {
                _unitLabel.Text = "";
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

        if (_unitSprite != null)
        {
            _unitSprite.Modulate = Colors.White;
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
        if (_surrenderFlagSprite == null) return;
        _flagBobTween?.Kill();
        _flagBobTween = CreateTween().SetLoops();
        _flagBobTween.TweenProperty(_surrenderFlagSprite, "position:y", -45.0f, 0.45).SetTrans(Tween.TransitionType.Sine);
        _flagBobTween.TweenProperty(_surrenderFlagSprite, "position:y", -39.0f, 0.45).SetTrans(Tween.TransitionType.Sine);
    }

    private void StopSurrenderVisuals()
    {
        _surrenderTween?.Kill();
        _surrenderTween = null;
        _flagBobTween?.Kill();
        _flagBobTween = null;

        if (_surrenderFlagSprite != null)
        {
            _surrenderFlagSprite.Visible = false;
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
        if (IsMoving || (Data != null && Data.IsMoving)) return;

        if (_unitSprite != null)
        {
            Vector2 basePos = new(0f, -20.0f);
            var tween = CreateTween();
            tween.TweenProperty(_unitSprite, "position", basePos + new Vector2(1.5f, 0), 0.05);
            tween.TweenProperty(_unitSprite, "position", basePos - new Vector2(1.5f, 0), 0.05);
            tween.TweenProperty(_unitSprite, "position", basePos, 0.05);
        }
    }

    public void PlayRecaptureFlash()
    {
        if (_borderRect == null) return;
        var tween = CreateTween();
        tween.TweenProperty(_borderRect, "scale", new Vector2(1.25f, 1.25f), 0.12);
        tween.TweenProperty(_borderRect, "scale", Vector2.One, 0.12);
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

    /// <summary>
    /// Cancels active movement tween and immediately snaps visual transform to the current Data.GridPosition center.
    /// </summary>
    public void CancelMovement()
    {
        if (_movementTween != null && _movementTween.IsRunning())
        {
            _movementTween.Kill();
            _movementTween = null;
        }

        if (Data != null)
        {
            Data.IsMoving = false;
            Position = GridMapManager.GridToWorldCenter(Data.GridPosition);
        }
        QueueRedraw();
    }

    public void MoveAlongPath(ReadOnlySpan<Vector2I> path, Action? onComplete = null)
    {
        if (Data == null || IsSurrendered)
        {
            onComplete?.Invoke();
            return;
        }

        if (path.Length <= 1)
        {
            Vector2I fallbackPos = path.Length == 1 ? path[0] : Data.GridPosition;
            Position = GridMapManager.GridToWorldCenter(fallbackPos);
            onComplete?.Invoke();
            return;
        }

        // Cancel previous running movement tween safely with fallback snap
        if (_movementTween != null && _movementTween.IsRunning())
        {
            _movementTween.Kill();
            _movementTween = null;
            Position = GridMapManager.GridToWorldCenter(Data.GridPosition);
        }

        Data.IsMoving = true;
        _movementTween = CreateTween().SetTrans(Tween.TransitionType.Linear);

        Vector2I originPos = path[0];
        Vector2I destinationPos = path[^1];
        Vector2 destinationWorldPos = GridMapManager.GridToWorldCenter(destinationPos);

        // Ensure starting position is aligned to origin tile center
        Position = GridMapManager.GridToWorldCenter(originPos);

        // Step-by-step tile movement animation starting from first next step
        for (int i = 1; i < path.Length; i++)
        {
            Vector2 targetWorld = (i == path.Length - 1) ? destinationWorldPos : GridMapManager.GridToWorldCenter(path[i]);
            _movementTween.TweenProperty(this, "position", targetWorld, 0.12f);
        }

        _movementTween.TweenCallback(Callable.From(() =>
        {
            Data.IsMoving = false;
            Position = destinationWorldPos; // Presentation View anchor only; WorldMap orchestrates Data.GridPosition
            QueueRedraw();

            EmitSignal(SignalName.UnitMoved, this, originPos, destinationPos);
            if (Data.MovementRemaining == 0)
            {
                EmitSignal(SignalName.MovementDepleted, this);
            }
            onComplete?.Invoke();
        }));
    }

    public void MoveAlongPath(Vector2I[] path, Action? onComplete = null) =>
        MoveAlongPath(new ReadOnlySpan<Vector2I>(path), onComplete);

    public void MoveAlongPath(ReadOnlySpan<Vector2I> path, int totalCost, Action? onComplete = null)
    {
        if (Data != null)
        {
            Data.MovementRemaining = Math.Max(0, Data.MovementRemaining - totalCost);
        }
        MoveAlongPath(path, onComplete);
    }

    public void MoveAlongPath(Vector2I[] path, int totalCost, Action? onComplete = null) =>
        MoveAlongPath(new ReadOnlySpan<Vector2I>(path), totalCost, onComplete);

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
        // 1. Drop shadow: compact subtle ellipse under feet
        Color shadowColor = new(0f, 0f, 0f, 0.28f);
        DrawColoredPolygon(CachedShadowPoly, shadowColor);

        // 2. Movement / turn readiness gem on head (floating above 1.5x character height)
        Vector2 gemCenter = new(0f, -44.0f);
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
