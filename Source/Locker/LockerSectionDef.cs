using System;
using System.Collections.Generic;
using Verse;

namespace Locker;

public class LockerSectionDef : Def, IComparable<LockerSectionDef>
{
    private const int LowestOrder = 9999;

    private readonly bool derivation = false;

    private int order;

    private ThingCategoryDef thingCategoryDef;

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
        return derivation ? $"{thingCategoryDef.parent.label}({thingCategoryDef.label})" : thingCategoryDef.label;
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
            order = LowestOrder,
            thingCategoryDef = thingCategorys[0]
        };
        return lockerSectionDef;
    }
}