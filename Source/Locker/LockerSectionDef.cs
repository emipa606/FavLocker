using System;
using System.Collections.Generic;
using Verse;

namespace Locker;

public class LockerSectionDef : Def, IComparable<LockerSectionDef>
{
    private static readonly int LOWEST_ORDER = 9999;

    public bool derivation = false;

    public int order;

    public ThingCategoryDef thingCategoryDef;

    public int CompareTo(LockerSectionDef other)
    {
        if (order != other.order)
        {
            return order - other.order;
        }

        return string.Compare(thingCategoryDef.label, other.thingCategoryDef.label, StringComparison.Ordinal);
    }

    public string GetLabel()
    {
        if (derivation)
        {
            return thingCategoryDef.parent.label + "(" + thingCategoryDef.label + ")";
        }

        return thingCategoryDef.label;
    }

    public static LockerSectionDef Get(List<ThingCategoryDef> thingCategorys)
    {
        foreach (var item in DefDatabase<LockerSectionDef>.AllDefsListForReading)
        {
            if (thingCategorys.Contains(item.thingCategoryDef))
            {
                return item;
            }
        }

        var lockerSectionDef = new LockerSectionDef
        {
            order = LOWEST_ORDER,
            thingCategoryDef = thingCategorys[0]
        };
        return lockerSectionDef;
    }
}