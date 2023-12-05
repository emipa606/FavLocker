using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

internal class JobDriver_RemoveRegisteredApparelWithLocker : JobDriver_ChangeApparel
{
    private CompLocker CompLockerFavLocker => TargetCompLocker;

    protected override string GetReportDefault()
    {
        return "EKAI_Report_Remove_Default".Translate();
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        var targetQueue = job.GetTargetQueue(TARGET_QUEUE_WORN_APPARELS_AT_EXEC);
        var countQueue = job.countQueue;
        for (var num = targetQueue.Count - 1; num >= 0; num--)
        {
            var thing = targetQueue[num].Thing;
            if (CompLockerFavLocker.RegisteredApparelsReadOnly().Contains(thing))
            {
                continue;
            }

            targetQueue.RemoveAt(num);
            countQueue.RemoveAt(num);
        }
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TARGET_CONTAINER);
        this.FailOnOwnerStatus(TARGET_CONTAINER);
        yield return Toils_Goto.GotoThing(TARGET_CONTAINER, PathEndMode.Touch);
        var setRemove = Toils.SetRemoveApparel(TARGET_APPAREL, TARGET_CONTAINER);
        yield return setRemove;
        var setWear = Toils.SetWearApparel(TARGET_APPAREL, TARGET_CONTAINER);
        yield return Toils_Jump.JumpIfTargetInvalid(TARGET_APPAREL, setWear);
        yield return Toils.SetProgress(Progress.REMOVING);
        yield return Toils.WaitEquipDelay(TARGET_APPAREL, TARGET_CONTAINER);
        yield return Toils.RemoveAndDropApparel(TARGET_APPAREL);
        yield return Toils_Jump.Jump(setRemove);
        yield return setWear;
        var putToil = Toils.PutApparelInTheLocker(TARGET_QUEUE_WORN_APPARELS_AT_EXEC, TARGET_CONTAINER);
        yield return Toils_Jump.JumpIfTargetInvalid(TARGET_APPAREL, putToil);
        yield return Toils.SetProgress(Progress.WEARING);
        yield return Toils.WaitEquipDelay(TARGET_APPAREL, TARGET_CONTAINER);
        yield return Toils.TakeApparelFromContainerAndWear(TARGET_APPAREL, TARGET_CONTAINER);
        yield return Toils_Jump.Jump(setWear);
        yield return putToil;
    }

    public override Apparel GetNextWearApparel(TargetIndex containerInd)
    {
        if (containerInd != TARGET_CONTAINER)
        {
            throw new ArgumentException();
        }

        var list = Util.SubstractList(CompLockerFavLocker.InnerApparelsReadOnly(),
            CompLockerFavLocker.RegisteredApparelsReadOnly());
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

        var list = Util.SortApparelListForDraw(pawn.apparel.UnlockedApparel, true);
        foreach (var item in list)
        {
            if (CompLockerFavLocker.RegisteredApparelsReadOnly().Contains(item))
            {
                return item;
            }
        }

        return null;
    }
}