using System;
using System.Collections.Generic;
using Verse;

namespace Locker;

public class BodyPartGroup : IComparable<BodyPartGroup>
{
    private static readonly List<BodyPartGroup> allList;

    private static readonly BodyPartGroup UNKNOWN;

    private readonly BodyPartGroupDef def;

    private readonly int order;

    static BodyPartGroup()
    {
        allList = new List<BodyPartGroup>();
        UNKNOWN = new BodyPartGroup(9999, null);
        allList.Add(new BodyPartGroup(0, DefDatabase<BodyPartGroupDef>.GetNamed("UpperHead")));
        allList.Add(new BodyPartGroup(0, DefDatabase<BodyPartGroupDef>.GetNamed("FullHead")));
        allList.Add(new BodyPartGroup(2, DefDatabase<BodyPartGroupDef>.GetNamed("Neck")));
        allList.Add(new BodyPartGroup(3, DefDatabase<BodyPartGroupDef>.GetNamed("Torso")));
        allList.Add(new BodyPartGroup(4, DefDatabase<BodyPartGroupDef>.GetNamed("Shoulders")));
        allList.Add(new BodyPartGroup(5, DefDatabase<BodyPartGroupDef>.GetNamed("Arms")));
        allList.Add(new BodyPartGroup(6, DefDatabase<BodyPartGroupDef>.GetNamed("Waist")));
        allList.Add(new BodyPartGroup(7, DefDatabase<BodyPartGroupDef>.GetNamed("Legs")));
    }

    private BodyPartGroup(int order, BodyPartGroupDef def)
    {
        this.order = order;
        this.def = def;
    }

    public int CompareTo(BodyPartGroup other)
    {
        return order - other.order;
    }

    public static List<BodyPartGroup> Get(List<BodyPartGroupDef> bodyPartGroups)
    {
        var list = new List<BodyPartGroup>();
        foreach (var bodyPartGroup in bodyPartGroups)
        {
            list.Add(Get(bodyPartGroup));
        }

        return list;
    }

    public static BodyPartGroup Get(BodyPartGroupDef def)
    {
        foreach (var all in allList)
        {
            if (all.def.defName == def.defName)
            {
                return all;
            }
        }

        return UNKNOWN;
    }

    public int GetOrder()
    {
        return order;
    }
}