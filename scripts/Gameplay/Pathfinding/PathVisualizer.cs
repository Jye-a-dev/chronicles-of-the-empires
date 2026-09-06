using System;
using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Renders visual tactical path guidance (Line2D) with pixel-sharp routing and reachability color states.
/// </summary>
public partial class PathVisualizer : Line2D
{
    [Export] public Color ValidPathColor = new(0.36f, 0.85f, 0.45f, 0.85f); // Soft green
    [Export] public Color InvalidPathColor = new(0.92f, 0.28f, 0.28f, 0.85f); // Crimson warning

    public override void _Ready()
    {
        Width = 2.0f;
        JointMode = LineJointMode.Round;
        BeginCapMode = LineCapMode.Round;
        EndCapMode = LineCapMode.Round;
        TextureFilter = TextureFilterEnum.Nearest;
        Visible = false;
        ZIndex = 5; // Render above terrain and beneath UI
    }

    public void ShowPath(ReadOnlySpan<Vector2I> gridPath, bool isReachable)
    {
        ClearPoints();

        if (gridPath.Length < 2)
        {
            Visible = false;
            return;
        }

        DefaultColor = isReachable ? ValidPathColor : InvalidPathColor;

        for (int i = 0; i < gridPath.Length; i++)
        {
            AddPoint(GridMapManager.GridToWorldCenter(gridPath[i]));
        }

        Visible = true;
    }

    public void ShowPath(Vector2I[] gridPath, bool isReachable) =>
        ShowPath(new ReadOnlySpan<Vector2I>(gridPath), isReachable);

    public void ClearPath()
    {
        ClearPoints();
        Visible = false;
    }
}

