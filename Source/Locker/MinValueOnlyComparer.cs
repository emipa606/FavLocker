using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Locker;

public class MinValueOnlyComparer<T> : IComparer<List<T>> where T : IComparable<T>
{
    public int Compare(List<T> x, List<T> y)
    {
        if (x.NullOrEmpty() && y.NullOrEmpty())
        {
            return 0;
        }

        if (x == null || x.NullOrEmpty())
        {
            return 1;
        }

        if (y == null || y.NullOrEmpty())
        {
            return -1;
        }

        x = x.OrderBy(e => e).ToList();
        y = y.OrderBy(e => e).ToList();
        return x[0].CompareTo(y[0]);
    }
}