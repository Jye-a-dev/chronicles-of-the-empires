using Godot;

#nullable enable

namespace ChroniclesOfTheEmpires.Gameplay;

/// <summary>
/// Camera2D optimized for 2D pixel-art strategy sa bàn.
/// Handles WASD/arrow pan, middle-click drag, and clamped mouse-wheel zoom with pixel-snap alignment.
/// </summary>
public partial class RTSCamera2D : Camera2D
{
    [Export] public float PanSpeed = 250.0f;
    [Export] public float MinZoom = 0.75f;
    [Export] public float MaxZoom = 3.0f;
    [Export] public float ZoomStep = 0.2f;
    [Export] public float ZoomLerpSpeed = 15.0f;

    private Vector2 _targetZoom = Vector2.One;
    private bool _isDragging;
    private Vector2 _dragStartMousePos;
    private Vector2 _dragStartCameraPos;

    public override void _Ready()
    {
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = 12.0f;
        _targetZoom = Zoom;
    }

    public override void _Process(double delta)
    {
        HandleKeyboardPan((float)delta);

        if (Zoom != _targetZoom)
        {
            Zoom = Zoom.Lerp(_targetZoom, (float)delta * ZoomLerpSpeed);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp && mb.Pressed)
            {
                ApplyZoom(ZoomStep);
                GetViewport().SetInputAsHandled();
            }
            else if (mb.ButtonIndex == MouseButton.WheelDown && mb.Pressed)
            {
                ApplyZoom(-ZoomStep);
                GetViewport().SetInputAsHandled();
            }
            else if (mb.ButtonIndex == MouseButton.Middle)
            {
                if (mb.Pressed)
                {
                    _isDragging = true;
                    _dragStartMousePos = mb.Position;
                    _dragStartCameraPos = Position;
                }
                else
                {
                    _isDragging = false;
                }
                GetViewport().SetInputAsHandled();
            }
        }
        else if (@event is InputEventMouseMotion mm && _isDragging)
        {
            Vector2 mouseDelta = (mm.Position - _dragStartMousePos) / Zoom;
            Position = ClampPosition(_dragStartCameraPos - mouseDelta);
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleKeyboardPan(float delta)
    {
        if (_isDragging) return;

        Vector2 inputDir = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) inputDir.Y -= 1.0f;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) inputDir.Y += 1.0f;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) inputDir.X -= 1.0f;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) inputDir.X += 1.0f;

        if (inputDir != Vector2.Zero)
        {
            inputDir = inputDir.Normalized();
            Vector2 newPos = Position + inputDir * (PanSpeed / Zoom.X) * delta;
            Position = ClampPosition(newPos);
        }
    }

    private void ApplyZoom(float deltaZoom)
    {
        float nextZoom = Mathf.Clamp(_targetZoom.X + deltaZoom, MinZoom, MaxZoom);
        _targetZoom = new Vector2(nextZoom, nextZoom);
    }

    public void SetBounds(int widthInPixels, int heightInPixels)
    {
        const int pad = 48; // Allows panning left and top past (0,0) to easily inspect and click border hexes
        LimitLeft = -pad;
        LimitTop = -pad;
        LimitRight = widthInPixels + pad;
        LimitBottom = heightInPixels + pad;
        LimitSmoothed = true;

        // Ensure user cannot zoom out further than map extents to prevent void exposure
        Vector2 viewSize = GetViewportRect().Size;
        float totalW = widthInPixels + pad * 2;
        float totalH = heightInPixels + pad * 2;
        float minZoomX = viewSize.X / Mathf.Max(totalW, 1);
        float minZoomY = viewSize.Y / Mathf.Max(totalH, 1);
        MinZoom = Mathf.Max(0.75f, Mathf.Max(minZoomX, minZoomY));

        float clampedZoom = Mathf.Clamp(_targetZoom.X, MinZoom, MaxZoom);
        _targetZoom = new Vector2(clampedZoom, clampedZoom);
        Zoom = _targetZoom;
        Position = ClampPosition(Position);
    }

    private Vector2 ClampPosition(Vector2 pos)
    {
        Vector2 viewportHalf = (GetViewportRect().Size * 0.5f) / Zoom;

        float clampedX;
        if ((LimitRight - LimitLeft) <= viewportHalf.X * 2.0f)
        {
            clampedX = (LimitLeft + LimitRight) * 0.5f;
        }
        else
        {
            clampedX = Mathf.Clamp(pos.X, LimitLeft + viewportHalf.X, LimitRight - viewportHalf.X);
        }

        float clampedY;
        if ((LimitBottom - LimitTop) <= viewportHalf.Y * 2.0f)
        {
            clampedY = (LimitTop + LimitBottom) * 0.5f;
        }
        else
        {
            clampedY = Mathf.Clamp(pos.Y, LimitTop + viewportHalf.Y, LimitBottom - viewportHalf.Y);
        }

        return new Vector2(clampedX, clampedY);
    }
}

