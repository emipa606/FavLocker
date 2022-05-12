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
        if (containerInd != TARGET_CONTAINER)
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
        if (containerInd != TARGET_CONTAINER)
        {
            throw new ArgumentException();
        }

        var list = Util.SortApparelListForDraw(pawn.apparel.WornApparel, true);
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

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TARGET_CONTAINER);
        this.FailOnOwnerStatus(TARGET_CONTAINER);
        yield return Toils_Goto.GotoThing(TARGET_CONTAINER, PathEndMode.Touch);
        var setRemove = Toils.SetRemoveApparel(TARGET_APPAREL, TARGET_CONTAINER);
        yield return setRemove;
        var setWear = Toils.SetWearApparel(TARGET_APPAREL, TARGET_CONTAINER);
        yield return Toils_Jump.JumpIfTargetInvalid(TARGET_APPAREL, setWear);
        yield return Toils.SetProgress(Progress.REMOVING);
        yield return Toils.WaitEquipDelay(TARGET_APPAREL, TARGET_CONTAINER,
            Global.FACTOR_EQUIPDELAY_WHEN_WEAR_FAVORITE);
        yield return Toils.RemoveAndDropApparel(TARGET_APPAREL);
        yield return Toils_Jump.Jump(setRemove);
        yield return setWear;
        var putToil = Toils.PutApparelInTheLocker(TARGET_QUEUE_WORN_APPARELS_AT_EXEC, TARGET_CONTAINER);
        yield return Toils_Jump.JumpIfTargetInvalid(TARGET_APPAREL, putToil);
        yield return Toils.SetProgress(Progress.WEARING);
        yield return Toils.WaitEquipDelay(TARGET_APPAREL, TARGET_CONTAINER,
            Global.FACTOR_EQUIPDELAY_WHEN_WEAR_FAVORITE);
        yield return Toils.TakeApparelFromContainerAndWear(TARGET_APPAREL, TARGET_CONTAINER, true);
        yield return Toils_Jump.Jump(setWear);
        yield return putToil;
    }
}