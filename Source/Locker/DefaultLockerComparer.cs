using System;
using System.Collections.Generic;
using RimWorld;

namespace Locker.order;

public class DefaultLockerComparer : IComparer<Transferable>
{
    public int Compare(Transferable x, Transferable y)
    {
        var anyThing = x?.AnyThing;
        var anyThing2 = y?.AnyThing;
        if (anyThing?.Stuff != anyThing2?.Stuff)
        {
            if (anyThing?.Stuff == null)
            {
                return -1;
            }

            if (anyThing2?.Stuff == null)
            {
                return 1;
            }

            return string.Compare(anyThing.Stuff.label, anyThing2.Stuff.label, StringComparison.Ordinal);
        }

        var num = new TransferableComparer_Quality().Compare(x, y);
        if (num != 0)
        {
            return num;
        }

        num = new TransferableComparer_HitPointsPercentage().Compare(x, y);
        if (num != 0)
        {
            return num;
        }

        num = new TransferableComparer_MarketValue().Compare(x, y);
        if (num != 0)
        {
            return num;
        }

        return 0;
    }
}