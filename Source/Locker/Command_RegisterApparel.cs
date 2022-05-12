using UnityEngine;
using Verse;

namespace Locker;

internal class Command_RegisterApparel : Command
{
    public CompLocker compLocker;

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        Find.WindowStack.Add(new Dialog_RegisterItem(compLocker.Map, compLocker.parent));
    }
}