using HarmonyLib;
using RimWorld;

namespace Locker;

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ReapplyAllMods))]
internal static class ResearchManager_ReapplyAllMods
{
    public static void Postfix()
    {
        Util.SyncTechSetting();
    }
}