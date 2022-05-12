using HarmonyLib;
using Verse;

namespace Locker;

[StaticConstructorOnStartup]
public class Patcher
{
    static Patcher()
    {
        var harmony = new Harmony("Ekai.FavLocker");
        harmony.PatchAll();
    }
}