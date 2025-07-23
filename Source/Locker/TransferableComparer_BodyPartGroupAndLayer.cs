using RimWorld;
using Verse;

namespace Locker;

public class TransferableComparer_BodyPartGroupAndLayer : TransferableComparer
{
    public override int Compare(Transferable lhs, Transferable rhs)
    {
        return compare(lhs?.ThingDef, rhs?.ThingDef);
    }

    private static int compare(ThingDef lhsTh, ThingDef rhsTh)
    {
        var minValueOnlyComparer = new MinValueOnlyComparer<BodyPartGroup>();
        var x = BodyPartGroup.Get(lhsTh.apparel.bodyPartGroups);
        var y = BodyPartGroup.Get(rhsTh.apparel.bodyPartGroups);
        var num = minValueOnlyComparer.Compare(x, y);
        if (num != 0)
        {
            return num;
        }

        var minValueOnlyComparer2 = new MinValueOnlyComparer<ApparelLayer>();
        var x2 = ApparelLayer.Get(lhsTh.apparel.layers);
        var y2 = ApparelLayer.Get(rhsTh.apparel.layers);
        return minValueOnlyComparer2.Compare(x2, y2);
    }
}