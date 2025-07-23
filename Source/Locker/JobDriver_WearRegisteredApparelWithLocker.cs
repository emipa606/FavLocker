using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

internal class JobDriver_WearRegisteredApparelWithLocker : JobDriver_ChangeApparel
{
    private CompLocker CompLockerFavLocker => TargetCompLocker;

    protected override string GetReportDefault()
    {
        return "EKAI_Report_Wear_Default".Translate();
    }

    public override Apparel GetNextWearApparel(TargetIndex containerInd)
    {
        if (containerInd != TargetIndex)
        {
            throw new ArgumentException();
        }

        var list = new List<Apparel>(CompLockerFavLocker.GetApparelsRegisterdAndInner());
        if (!list.Any())
        {
            return null;
        }

        list = Util.SortApparelListForDraw(list);
        return list.First();
    }

    public override Apparel GetNextRemoveApparel(TargetIndex containerInd)
    {
        if (containerInd != TargetIndex)
        {
            throw new ArgumentException();
        }

        var list = Util.SortApparelListForDraw(pawn.apparel.UnlockedApparel, true);
        var apparelsRegisterdAndInner = CompLockerFavLocker.GetApparelsRegisterdAndInner();
        foreach (var item in list)
        {
            if (Util.AnyCantWearTogetherApparels(apparelsRegisterdAndInner, item))
            {
                return item;
            }
        }

        return null;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex);
        this.FailOnOwnerStatus(TargetIndex);
        yield return Toils_Goto.GotoThing(TargetIndex, PathEndMode.Touch);
        var setRemove = Toils.SetRemoveApparel(TargetApparel, TargetIndex);
        yield return setRemove;
        var setWear = Toils.SetWearApparel(TargetApparel, TargetIndex);
        yield return Toils_Jump.JumpIfTargetInvalid(TargetApparel, setWear);
        yield return Toils.SetProgress(Progress.REMOVING);
        yield return Toils.WaitEquipDelay(TargetApparel, TargetIndex,
            Global.FactorEquipdelayWhenWearFavorite);
        yield return Toils.RemoveAndDropApparel(TargetApparel);
        yield return Toils_Jump.Jump(setRemove);
        yield return setWear;
        var putToil = Toils.PutApparelInTheLocker(TargetQueueWornApparelsAtExec, TargetIndex);
        yield return Toils_Jump.JumpIfTargetInvalid(TargetApparel, putToil);
        yield return Toils.SetProgress(Progress.WEARING);
        yield return Toils.WaitEquipDelay(TargetApparel, TargetIndex,
            Global.FactorEquipdelayWhenWearFavorite);
        yield return Toils.TakeApparelFromContainerAndWear(TargetApparel, TargetIndex, true);
        yield return Toils_Jump.Jump(setWear);
        yield return putToil;
    }
}