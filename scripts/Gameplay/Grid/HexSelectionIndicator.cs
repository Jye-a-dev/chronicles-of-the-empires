using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Renders a crisp, glowing hexagonal highlight outline over the currently active/selected hex tile.
/// </summary>
public partial class HexSelectionIndicator : Line2D
{
    [Export] public Color OutlineColor = new(1.0f, 0.86f, 0.28f, 0.95f); // Imperial Gold
    [Export] public float PulseSpeed = 2.5f;

    private Tween? _pulseTween;

    public override void _Ready()
    {
        Width = 2.0f;
        JointMode = LineJointMode.Round;
        BeginCapMode = LineCapMode.Round;
        EndCapMode = LineCapMode.Round;
        TextureFilter = TextureFilterEnum.Nearest;
        DefaultColor = OutlineColor;
        ZIndex = 8; // Render above terrain and under units
        Visible = false;

        BuildHexagonPoints();
    }

    private void BuildHexagonPoints()
    {
        ClearPoints();

        // Exact 6 vertices of a 32x32 pointy-topped hexagon (centered at 0,0)
        AddPoint(new Vector2(0f, -16f));       // Top vertex
        AddPoint(new Vector2(16f, -8f));       // Top-right
        AddPoint(new Vector2(16f, 8f));        // Bottom-right
        AddPoint(new Vector2(0f, 16f));        // Bottom
        AddPoint(new Vector2(-16f, 8f));       // Bottom-left
        AddPoint(new Vector2(-16f, -8f));      // Top-left
        AddPoint(new Vector2(0f, -16f));       // Close loop
    }

    public void SelectHex(Vector2 worldCenter)
    {
        GlobalPosition = worldCenter;
        Visible = true;

        _pulseTween?.Kill();
        Modulate = Colors.White;
        _pulseTween = CreateTween().SetLoops();
        _pulseTween.TweenProperty(this, "modulate:a", 0.45f, 0.5f / PulseSpeed);
        _pulseTween.TweenProperty(this, "modulate:a", 1.0f, 0.5f / PulseSpeed);
    }

    public void ClearSelection()
    {
        _pulseTween?.Kill();
        Visible = false;
    }
}

