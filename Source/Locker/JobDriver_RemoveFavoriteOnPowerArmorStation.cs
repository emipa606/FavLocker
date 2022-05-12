using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

public class JobDriver_RemoveFavoriteOnPowerArmorStation : JobDriver_ChangeApparel
{
    private Building_PowerArmorStation PowerArmorStation => (Building_PowerArmorStation)TargetContainer;

    private CompLocker CompLockerStation => TargetCompLocker;

    private Building_Locker FavLocker => (Building_Locker)LinkedContainer;

    private CompLocker CompLockerFavLocker => LinkedCompLocker;

    protected override string GetReportDefault()
    {
        return "EKAI_Report_Remove_Default".Translate();
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        if (CompLockerFavLocker == null)
        {
            return;
        }

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

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TARGET_CONTAINER);
        this.FailOnOwnerStatus(TARGET_CONTAINER);
        if (FavLocker != null)
        {
            this.FailOnDespawnedNullOrForbidden(TARGET_LINKED_CONTAINER);
            this.FailOnOwnerStatus(TARGET_LINKED_CONTAINER);
        }

        var label_End = Toils_General.Label();
        var check_NeedGotoLocker = Toils.JumpIfNeedNotGoto(label_End, TARGET_LINKED_CONTAINER);
        yield return Toils.JumpIfNeedNotGoto(check_NeedGotoLocker, TARGET_CONTAINER);
        yield return Toils_Goto.GotoThing(TARGET_CONTAINER, PowerArmorStation.GetStandbyPosition());
        yield return Toils.WaitForStationToChange(TARGET_CONTAINER, 180);
        yield return Toils.RemoveAllApparelAndStoreContainer(TARGET_CONTAINER);
        yield return check_NeedGotoLocker;
        yield return Toils_Goto.GotoThing(TARGET_LINKED_CONTAINER, PathEndMode.Touch);
        var setRemoveOnLocker = Toils.SetRemoveApparel(TARGET_APPAREL, TARGET_LINKED_CONTAINER);
        yield return setRemoveOnLocker;
        var setWearOnLocker = Toils.SetWearApparel(TARGET_APPAREL, TARGET_LINKED_CONTAINER);
        yield return Toils_Jump.JumpIfTargetInvalid(TARGET_APPAREL, setWearOnLocker);
        yield return Toils.SetProgress(Progress.REMOVING);
        yield return Toils.WaitEquipDelay(TARGET_APPAREL, TARGET_LINKED_CONTAINER,
            Global.FACTOR_EQUIPDELAY_WHEN_WEAR_FAVORITE);
        yield return Toils.RemoveAndDropApparel(TARGET_APPAREL);
        yield return Toils_Jump.Jump(setRemoveOnLocker);
        yield return setWearOnLocker;
        var putToil = Toils.PutApparelInTheLocker(TARGET_QUEUE_WORN_APPARELS_AT_EXEC, TARGET_LINKED_CONTAINER);
        yield return Toils_Jump.JumpIfTargetInvalid(TARGET_APPAREL, putToil);
        yield return Toils.SetProgress(Progress.WEARING);
        yield return Toils.WaitEquipDelay(TARGET_APPAREL, TARGET_LINKED_CONTAINER,
            Global.FACTOR_EQUIPDELAY_WHEN_WEAR_FAVORITE);
        yield return Toils.TakeApparelFromContainerAndWear(TARGET_APPAREL, TARGET_LINKED_CONTAINER);
        yield return Toils_Jump.Jump(setWearOnLocker);
        yield return putToil;
        yield return label_End;
    }

    public override Apparel GetNextWearApparel(TargetIndex containerInd)
    {
        if (containerInd != TARGET_LINKED_CONTAINER)
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
        if (containerInd != TARGET_LINKED_CONTAINER)
        {
            throw new ArgumentException();
        }

        var list = Util.SortApparelListForDraw(pawn.apparel.WornApparel, true);
        foreach (var item in list)
        {
            if (CompLockerFavLocker.RegisteredApparelsReadOnly().Contains(item))
            {
                return item;
            }
        }

        return null;
    }

    public override bool NeedGoto(TargetIndex containerInd)
    {
        if (containerInd == TARGET_CONTAINER)
        {
            return NeedGoToStation();
        }

        if (containerInd == TARGET_LINKED_CONTAINER)
        {
            return NeedGoToLocker();
        }

        throw new ArgumentException();
    }

    private bool NeedGoToStation()
    {
        return CompLockerStation.AnyWear(pawn);
    }

    private bool NeedGoToLocker()
    {
        if (FavLocker == null)
        {
            return false;
        }

        if (GetNextWearApparel(TARGET_LINKED_CONTAINER) != null)
        {
            return true;
        }

        if (GetNextRemoveApparel(TARGET_LINKED_CONTAINER) != null)
        {
            return true;
        }

        return false;
    }
}