using HarmonyLib;
using RimWorld;

namespace Locker;

[HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ReapplyAllMods))]
internal class Patch_ResearchManager_ReapplyAllMods
{
    private static void Postfix()
    {
        Util.SyncTechSetting();
    }
}