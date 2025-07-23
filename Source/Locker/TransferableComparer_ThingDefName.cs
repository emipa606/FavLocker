using System;
using RimWorld;
using Verse;

namespace Locker;

public class TransferableComparer_ThingDefName : TransferableComparer
{
    public override int Compare(Transferable lhs, Transferable rhs)
    {
        return compare(lhs?.ThingDef, rhs?.ThingDef);
    }

    private static int compare(ThingDef lhsTh, ThingDef rhsTh)
    {
        return string.Compare(lhsTh.defName, rhsTh.defName, StringComparison.Ordinal);
    }
}