using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace BetterMap.Core;

public partial class MinimapDrawingInputLayer : Control
{
    private MapOverviewPanel? _panel;
    private NMapDrawings? _activeDrawings;
    private bool _isDrawingFromMinimap;
    private MouseButton _activeButton = MouseButton.None;
    private ulong _lastHandledEventId;
    private ulong _lastHandledEventFrame;

    public void Configure(MapOverviewPanel panel)
    {
        _panel = panel;
    }

    public void CancelDrawing()
    {
        if (!_isDrawingFromMinimap) return;

        StopDrawing();
    }

    internal bool TryHandleInput(InputEvent @event)
    {
        var eventId = @event.GetInstanceId();
        var eventFrame = Engine.GetProcessFrames();
        if (_lastHandledEventId == eventId && _lastHandledEventFrame == eventFrame) return true;

        var handled = @event switch
        {
            InputEventMouseButton button => HandleMouseButton(button),
            InputEventMouseMotion motion => HandleMouseMotion(motion),
            _ => false
        };

        if (handled)
        {
            _lastHandledEventId = eventId;
            _lastHandledEventFrame = eventFrame;
            GetViewport()?.SetInputAsHandled();
        }

        return handled;
    }

    private bool HandleMouseButton(InputEventMouseButton button)
    {
        if (_isDrawingFromMinimap)
        {
            if (!button.Pressed && button.ButtonIndex == _activeButton)
            {
                StopDrawing();
            }

            return true;
        }

        if (!button.Pressed) return false;
        if (!TryGetDrawingPosition(button.GlobalPosition, out var drawings, out var drawingsPosition)) return false;

        return TryStartDrawing(button, drawings, drawingsPosition);
    }

    private bool HandleMouseMotion(InputEventMouseMotion motion)
    {
        if (!_isDrawingFromMinimap) return false;
        if (_activeDrawings == null || !GodotObject.IsInstanceValid(_activeDrawings))
        {
            StopDrawing();
            return true;
        }

        if (!TryGetDrawingPosition(motion.GlobalPosition, out var drawings, out var drawingsPosition))
        {
            return true;
        }

        if (drawings == null || drawings.GetInstanceId() != _activeDrawings.GetInstanceId())
        {
            return true;
        }

        _activeDrawings.UpdateCurrentLinePositionLocal(drawingsPosition);
        return true;
    }

    private bool TryStartDrawing(InputEventMouseButton button, NMapDrawings? drawings, Vector2 drawingsPosition)
    {
        if (_isDrawingFromMinimap) return true;
        if (drawings == null) return false;

        var mode = GetDrawingModeForButton(drawings, button.ButtonIndex);
        if (mode == DrawingMode.None) return false;

        _activeDrawings = drawings;
        _activeButton = button.ButtonIndex;
        var overrideMode = button.ButtonIndex is MouseButton.Right or MouseButton.Middle ? mode : (DrawingMode?)null;
        _activeDrawings.BeginLineLocal(drawingsPosition, overrideMode);
        _isDrawingFromMinimap = true;
        return true;
    }

    private DrawingMode GetDrawingModeForButton(NMapDrawings drawings, MouseButton button)
    {
        return button switch
        {
            MouseButton.Right => DrawingMode.Drawing,
            MouseButton.Middle => DrawingMode.Erasing,
            MouseButton.Left => drawings.GetLocalDrawingMode(),
            _ => DrawingMode.None
        };
    }

    private void StopDrawing()
    {
        try
        {
            if (_activeDrawings != null && GodotObject.IsInstanceValid(_activeDrawings) && _activeDrawings.IsLocalDrawing())
            {
                _activeDrawings.StopLineLocal();
            }
        }
        finally
        {
            _activeDrawings = null;
            _activeButton = MouseButton.None;
            _isDrawingFromMinimap = false;
        }
    }

    private bool TryGetDrawingPosition(Vector2 screenPosition, out NMapDrawings? drawings, out Vector2 drawingsPosition)
    {
        drawings = null;
        drawingsPosition = Vector2.Zero;

        return _panel != null
            && GodotObject.IsInstanceValid(_panel)
            && _panel.TryGetDrawingPositionFromScreenPosition(screenPosition, out drawings, out drawingsPosition);
    }
}
