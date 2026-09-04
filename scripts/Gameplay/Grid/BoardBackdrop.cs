using Godot;
using System;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Renders an authentic ancient Dong Son / military sandbox war table backdrop behind the hex terrain.
/// Eliminates raw empty engine canvas by framing the battlefield in dark lacquered wood and antique bronze borders.
/// </summary>
public partial class BoardBackdrop : Node2D
{
    private Rect2 _boardRect = new(-128, -128, 1280, 1280);
    private Rect2 _mapRect = new(0, 0, 1024, 1024);

    public override void _Ready()
    {
        ZIndex = -20; // Ensure rendered beneath TileMapLayer and all gameplay items
    }

    public void SetDimensions(float mapWidth, float mapHeight)
    {
        const float padding = 160f;
        const float borderMargin = 24f;

        _mapRect = new Rect2(
            -borderMargin,
            -borderMargin,
            mapWidth + borderMargin * 2f,
            mapHeight + borderMargin * 2f
        );

        _boardRect = new Rect2(
            _mapRect.Position.X - padding,
            _mapRect.Position.Y - padding,
            _mapRect.Size.X + padding * 2f,
            _mapRect.Size.Y + padding * 2f
        );

        QueueRedraw();
    }

    public override void _Draw()
    {
        // 1. Vast tactical table surface (ebony lacquer / military desk)
        Color tableColor = new("#0e0b11");
        DrawRect(_boardRect, tableColor, filled: true);

        // 2. Subtle military grid hatch / ancient parchment grain
        Color hatchColor = new(0.12f, 0.09f, 0.14f, 0.45f);
        for (float x = _boardRect.Position.X; x < _boardRect.End.X; x += 32f)
        {
            DrawLine(new Vector2(x, _boardRect.Position.Y), new Vector2(x, _boardRect.End.Y), hatchColor, 1.0f);
        }
        for (float y = _boardRect.Position.Y; y < _boardRect.End.Y; y += 32f)
        {
            DrawLine(new Vector2(_boardRect.Position.X, y), new Vector2(_boardRect.End.Y, y), hatchColor, 1.0f);
        }

        // 3. Map Inlay Basin (parchment/sandbed)
        Color inlayColor = new("#141017");
        DrawRect(_mapRect, inlayColor, filled: true);

        // 4. Antique Bronze Framing Borders
        Color outerBorderColor = new("#2a1d13");
        Color bronzeGoldColor = new("#c99a3e");
        Color innerShadowColor = new(0f, 0f, 0f, 0.55f);

        // Outer wooden rim
        DrawRect(new Rect2(_mapRect.Position - new Vector2(10, 10), _mapRect.Size + new Vector2(20, 20)), outerBorderColor, filled: false, width: 4.0f);

        // Main bronze trim line
        DrawRect(_mapRect, bronzeGoldColor, filled: false, width: 2.0f);

        // Inner drop shadow
        DrawRect(new Rect2(_mapRect.Position + new Vector2(2, 2), _mapRect.Size - new Vector2(4, 4)), innerShadowColor, filled: false, width: 2.0f);

        // 5. Four Corner Dong Son Bronze Studs (Mộng đồng cổ)
        Vector2[] corners =
        {
            _mapRect.Position,
            new(_mapRect.End.X, _mapRect.Position.Y),
            _mapRect.End,
            new(_mapRect.Position.X, _mapRect.End.Y)
        };

        foreach (var c in corners)
        {
            DrawCircle(c, 5.0f, new Color("#1a140f"));
            DrawCircle(c, 3.5f, bronzeGoldColor);
            DrawCircle(c, 1.5f, new Color("#f5c842"));
        }
    }
}
