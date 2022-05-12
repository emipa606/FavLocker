using System.Collections.Generic;
using Verse;

namespace Locker;

public class CompProperties_Locker : CompProperties
{
    public string commandDescKey;
    public string commandLabelKey;

    public string dialogTitleKey;

    public List<ThingDef> storableThingDefs;

    public CompProperties_Locker()
    {
        compClass = typeof(CompLocker);
    }
}