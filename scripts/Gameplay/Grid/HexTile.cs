using System;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// First-class visual and interactive tile component for individual hexagonal cells.
/// Supports live preview in the Godot editor and runtime array synchronization.
/// </summary>
[Tool]
public partial class HexTile : Node2D
{
    [Signal]
    public delegate void TileClickedEventHandler(HexTile tile);

    [Signal]
    public delegate void TileHoveredEventHandler(HexTile tile, bool entered);

    [Export]
    public TerrainType Terrain
    {
        get => _terrain;
        set
        {
            _terrain = value;
            UpdateVisuals();
        }
    }

    [Export]
    public Vector2I GridPosition
    {
        get => _gridPosition;
        set
        {
            _gridPosition = value;
            Name = $"Tile_{_gridPosition.X}_{_gridPosition.Y}";
        }
    }

    private TerrainType _terrain = TerrainType.Plains;
    private Vector2I _gridPosition = Vector2I.Zero;

    private Polygon2D? _polygon;
    private Line2D? _border;
    private Area2D? _hitArea;

    public HexCell? AssociatedCell { get; set; }

    public static readonly Vector2[] HexVertices =
    [
        new Vector2(0, -16),
        new Vector2(16f, -8),
        new Vector2(16f, 8),
        new Vector2(0, 16),
        new Vector2(-16f, 8),
        new Vector2(-16f, -8)
    ];

    public static readonly Vector2[] HexOutlinePoints =
    [
        new Vector2(0, -16),
        new Vector2(16f, -8),
        new Vector2(16f, 8),
        new Vector2(0, 16),
        new Vector2(-16f, 8),
        new Vector2(-16f, -8),
        new Vector2(0, -16)
    ];

    public override void _Ready()
    {
        _polygon = GetNodeOrNull<Polygon2D>("HexPolygon");
        _border = GetNodeOrNull<Line2D>("HexBorder");
        _hitArea = GetNodeOrNull<Area2D>("HitArea");

        if (_hitArea != null)
        {
            _hitArea.InputEvent += OnHitAreaInput;
            _hitArea.MouseEntered += () => EmitSignal(SignalName.TileHovered, this, true);
            _hitArea.MouseExited += () => EmitSignal(SignalName.TileHovered, this, false);
        }

        UpdateVisuals();
    }

    public void SetTerrain(TerrainType terrain)
    {
        _terrain = terrain;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        _polygon ??= GetNodeOrNull<Polygon2D>("HexPolygon");
        _border ??= GetNodeOrNull<Line2D>("HexBorder");

        Color baseCol = _terrain switch
        {
            TerrainType.Plains => new Color(0.25f, 0.48f, 0.20f),
            TerrainType.Forest => new Color(0.12f, 0.30f, 0.11f),
            TerrainType.River => new Color(0.17f, 0.36f, 0.56f),
            TerrainType.Mountain => new Color(0.35f, 0.38f, 0.41f),
            _ => new Color(0.2f, 0.2f, 0.2f)
        };

        Color borderCol = _terrain switch
        {
            TerrainType.River => new Color(0.45f, 0.29f, 0.13f, 0.6f),
            TerrainType.Mountain => new Color(0.55f, 0.58f, 0.61f, 0.6f),
            _ => new Color(0.15f, 0.18f, 0.12f, 0.4f)
        };

        if (_polygon != null)
        {
            _polygon.Polygon = HexVertices;
            _polygon.Color = baseCol;
        }

        if (_border != null)
        {
            _border.Points = HexOutlinePoints;
            _border.DefaultColor = borderCol;
        }
    }

    private void OnHitAreaInput(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.TileClicked, this);
        }
    }
}

