using System.Reflection;
using HarmonyLib;
using Verse;

namespace Locker;

[StaticConstructorOnStartup]
public class Patcher
{
    static Patcher()
    {
        new Harmony("Ekai.FavLocker").PatchAll(Assembly.GetExecutingAssembly());
    }
}