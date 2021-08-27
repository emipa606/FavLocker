using System;
using RimWorld;
using Verse;

namespace Locker
{
    public class TransferableComparer_ThingDefName : TransferableComparer
    {
        public override int Compare(Transferable lhs, Transferable rhs)
        {
            return Compare(lhs?.ThingDef, rhs?.ThingDef);
        }

        public static int Compare(ThingDef lhsTh, ThingDef rhsTh)
        {
            return string.Compare(lhsTh.defName, rhsTh.defName, StringComparison.Ordinal);
        }
    }
}