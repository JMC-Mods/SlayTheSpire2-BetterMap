using BetterMap.Core;
using Godot;
using HarmonyLib;
using JmcModLib.Utils;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace BetterMap.Patches;

[HarmonyPatch(typeof(NMapDrawings))]
public static class NMapDrawingsPatch
{
    private static bool _hasLoggedRemapError;

    [HarmonyPatch(nameof(NMapDrawings.BeginLineLocal))]
    [HarmonyPrefix]
    public static void BeginLineLocal_Prefix(NMapDrawings __instance, ref Vector2 position)
    {
        TryRemap(__instance, ref position);
    }

    [HarmonyPatch(nameof(NMapDrawings.UpdateCurrentLinePositionLocal))]
    [HarmonyPrefix]
    public static void UpdateCurrentLinePositionLocal_Prefix(NMapDrawings __instance, ref Vector2 position)
    {
        TryRemap(__instance, ref position);
    }

    private static void TryRemap(NMapDrawings drawings, ref Vector2 position)
    {
        try
        {
            if (MinimapDrawingRedirector.TryRemap(drawings, position, out var remappedPosition))
            {
                position = remappedPosition;
            }
        }
        catch (System.Exception ex)
        {
            if (_hasLoggedRemapError) return;

            _hasLoggedRemapError = true;
            ModLogger.Warn($"小地图涂鸦坐标重定向异常: {ex.Message}");
        }
    }
}
