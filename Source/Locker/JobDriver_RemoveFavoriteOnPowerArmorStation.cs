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

        var targetQueue = job.GetTargetQueue(TargetQueueWornApparelsAtExec);
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
        this.FailOnDespawnedNullOrForbidden(TargetIndex);
        this.FailOnOwnerStatus(TargetIndex);
        if (FavLocker != null)
        {
            this.FailOnDespawnedNullOrForbidden(TargetLinkedContainer);
            this.FailOnOwnerStatus(TargetLinkedContainer);
        }

        var label_End = Toils_General.Label();
        var check_NeedGotoLocker = Toils.JumpIfNeedNotGoto(label_End, TargetLinkedContainer);
        yield return Toils.JumpIfNeedNotGoto(check_NeedGotoLocker, TargetIndex);
        yield return Toils_Goto.GotoThing(TargetIndex, PowerArmorStation.GetStandbyPosition());
        yield return Toils.WaitForStationToChange(TargetIndex, 180);
        yield return Toils.RemoveAllApparelAndStoreContainer(TargetIndex);
        yield return check_NeedGotoLocker;
        yield return Toils_Goto.GotoThing(TargetLinkedContainer, PathEndMode.Touch);
        var setRemoveOnLocker = Toils.SetRemoveApparel(TargetApparel, TargetLinkedContainer);
        yield return setRemoveOnLocker;
        var setWearOnLocker = Toils.SetWearApparel(TargetApparel, TargetLinkedContainer);
        yield return Toils_Jump.JumpIfTargetInvalid(TargetApparel, setWearOnLocker);
        yield return Toils.SetProgress(Progress.REMOVING);
        yield return Toils.WaitEquipDelay(TargetApparel, TargetLinkedContainer,
            Global.FactorEquipdelayWhenWearFavorite);
        yield return Toils.RemoveAndDropApparel(TargetApparel);
        yield return Toils_Jump.Jump(setRemoveOnLocker);
        yield return setWearOnLocker;
        var putToil = Toils.PutApparelInTheLocker(TargetQueueWornApparelsAtExec, TargetLinkedContainer);
        yield return Toils_Jump.JumpIfTargetInvalid(TargetApparel, putToil);
        yield return Toils.SetProgress(Progress.WEARING);
        yield return Toils.WaitEquipDelay(TargetApparel, TargetLinkedContainer,
            Global.FactorEquipdelayWhenWearFavorite);
        yield return Toils.TakeApparelFromContainerAndWear(TargetApparel, TargetLinkedContainer);
        yield return Toils_Jump.Jump(setWearOnLocker);
        yield return putToil;
        yield return label_End;
    }

    public override Apparel GetNextWearApparel(TargetIndex containerInd)
    {
        if (containerInd != TargetLinkedContainer)
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
        if (containerInd != TargetLinkedContainer)
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

    public override bool NeedGoto(TargetIndex containerInd)
    {
        switch (containerInd)
        {
            case TargetIndex:
                return needGoToStation();
            case TargetLinkedContainer:
                return needGoToLocker();
            default:
                throw new ArgumentException();
        }
    }

    private bool needGoToStation()
    {
        return CompLockerStation.AnyWear(pawn);
    }

    private bool needGoToLocker()
    {
        if (FavLocker == null)
        {
            return false;
        }

        if (GetNextWearApparel(TargetLinkedContainer) != null)
        {
            return true;
        }

        return GetNextRemoveApparel(TargetLinkedContainer) != null;
    }
}