using System.Reflection;
using HarmonyLib;
using Verse;

namespace Locker;

[StaticConstructorOnStartup]
public static class Patcher
{
    static Patcher()
    {
        new Harmony("Ekai.FavLocker").PatchAll(Assembly.GetExecutingAssembly());
    }
}