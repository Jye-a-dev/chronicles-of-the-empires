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

    public void Configure(HexCell cell)
    {
        AssociatedCell = cell;
        GridPosition = cell.Coords;
        Position = cell.WorldPosition;
        SetTerrain(cell.Terrain);
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
            TerrainType.Ocean => new Color(0.09f, 0.23f, 0.41f),
            TerrainType.Hill => new Color(0.32f, 0.40f, 0.20f),
            _ => new Color(0.2f, 0.2f, 0.2f)
        };

        Color borderCol = _terrain switch
        {
            TerrainType.River => new Color(0.45f, 0.29f, 0.13f, 0.6f),
            TerrainType.Mountain => new Color(0.55f, 0.58f, 0.61f, 0.6f),
            TerrainType.Ocean => new Color(0.05f, 0.14f, 0.26f, 0.6f),
            TerrainType.Hill => new Color(0.25f, 0.30f, 0.15f, 0.5f),
            _ => new Color(0.15f, 0.18f, 0.12f, 0.4f)
        };

        if (_polygon != null)
        {
            _polygon.Polygon = HexVertices;
            // Keep transparent at runtime so TileMapLayer sprite textures show through cleanly
            _polygon.Color = Engine.IsEditorHint() ? baseCol : new Color(0, 0, 0, 0);
        }

        if (_border != null)
        {
            _border.Points = HexOutlinePoints;
            _border.Width = 1.0f;
            _border.DefaultColor = new Color(0.06f, 0.08f, 0.06f, 0.52f);
        }

        if (AssociatedCell != null && AssociatedCell.OwnerFactionId != -1)
        {
            UpdateOwnerVisual(AssociatedCell.OwnerFactionId);
        }
    }

    /// <summary>
    /// Dynamically tints the hex border according to the controlling faction.
    /// Imperial Gold for Player (0), Crimson Red for Rivals (>0), Muted Gray for Neutral (-1).
    /// </summary>
    public void UpdateOwnerVisual(int ownerFactionId)
    {
        _border ??= GetNodeOrNull<Line2D>("HexBorder");
        if (_border == null) return;

        if (ownerFactionId == 0)
        {
            _border.DefaultColor = new Color(0.96f, 0.78f, 0.26f, 0.75f);
            _border.Width = 1.5f;
        }
        else if (ownerFactionId > 0)
        {
            _border.DefaultColor = new Color(0.88f, 0.23f, 0.14f, 0.75f);
            _border.Width = 1.5f;
        }
        else
        {
            _border.DefaultColor = new Color(0.06f, 0.08f, 0.06f, 0.52f);
            _border.Width = 1.0f;
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

