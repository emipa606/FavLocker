using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Locker;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("GetGizmos")]
internal class Patch_Pawn_GetGizmos
{
    private static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
    {
        Util.GetOwnsContainer<Building_Locker>(__instance)?.SetGizmosWhenSelectedOwner(__instance, ref __result);
        Util.GetOwnsContainer<Building_PowerArmorStation>(__instance)
            ?.SetGizmosWhenSelectedOwner(__instance, ref __result);
    }
}