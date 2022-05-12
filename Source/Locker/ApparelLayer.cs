using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Locker;

public class ApparelLayer : IComparable<ApparelLayer>
{
    private static readonly List<ApparelLayer> allList;

    private static readonly ApparelLayer UNKNOWN;

    private readonly ApparelLayerDef def;

    private readonly int order;

    static ApparelLayer()
    {
        allList = new List<ApparelLayer>();
        UNKNOWN = new ApparelLayer(9999, null);
        allList.Add(new ApparelLayer(0, ApparelLayerDefOf.Overhead));
        allList.Add(new ApparelLayer(1, ApparelLayerDefOf.Shell));
        allList.Add(new ApparelLayer(2, ApparelLayerDefOf.Middle));
        allList.Add(new ApparelLayer(3, ApparelLayerDefOf.OnSkin));
        allList.Add(new ApparelLayer(4, ApparelLayerDefOf.Belt));
    }

    private ApparelLayer(int order, ApparelLayerDef def)
    {
        this.order = order;
        this.def = def;
    }

    public int CompareTo(ApparelLayer other)
    {
        return order - other.order;
    }

    public static List<ApparelLayer> Get(List<ApparelLayerDef> layers)
    {
        var list = new List<ApparelLayer>();
        foreach (var layer in layers)
        {
            list.Add(Get(layer));
        }

        return list;
    }

    public static ApparelLayer Get(ApparelLayerDef def)
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
}