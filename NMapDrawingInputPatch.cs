using BetterMap.Core;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace BetterMap.Patches;

[HarmonyPatch]
public static class NMapDrawingInputPatch
{
    [HarmonyPatch(typeof(NMouseModeMapDrawingInput), nameof(NMouseModeMapDrawingInput._Input))]
    [HarmonyPrefix]
    public static bool MouseModeInput_Prefix(InputEvent inputEvent)
    {
        return !MapOverviewPanel.TryHandleActiveMinimapDrawingInput(inputEvent);
    }

    [HarmonyPatch(typeof(NMouseHeldMapDrawingInput), nameof(NMouseHeldMapDrawingInput._Input))]
    [HarmonyPrefix]
    public static bool MouseHeldInput_Prefix(InputEvent inputEvent)
    {
        return !MapOverviewPanel.TryHandleActiveMinimapDrawingInput(inputEvent);
    }
}
