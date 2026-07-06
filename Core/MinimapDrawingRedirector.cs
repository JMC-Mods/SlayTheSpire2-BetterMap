using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace BetterMap.Core;

internal static class MinimapDrawingRedirector
{
    private static MapOverviewPanel? _activePanel;

    public static void Register(MapOverviewPanel panel)
    {
        _activePanel = panel;
    }

    public static void Unregister(MapOverviewPanel panel)
    {
        if (ReferenceEquals(_activePanel, panel))
        {
            _activePanel = null;
        }
    }

    public static bool TryRemap(NMapDrawings drawings, Vector2 position, out Vector2 remappedPosition)
    {
        remappedPosition = position;
        if (_activePanel == null || !GodotObject.IsInstanceValid(_activePanel)) return false;

        return _activePanel.TryProjectDrawingPosition(drawings, position, out remappedPosition);
    }
}
