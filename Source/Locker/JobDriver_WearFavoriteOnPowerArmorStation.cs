using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

public class JobDriver_WearFavoriteOnPowerArmorStation : JobDriver_ChangeApparel
{
    private Building_PowerArmorStation PowerArmorStation => (Building_PowerArmorStation)TargetContainer;

    private CompLocker CompLockerStation => TargetCompLocker;

    private Building_Locker FavLocker => (Building_Locker)LinkedContainer;

    private CompLocker CompLockerFavLocker => LinkedCompLocker;

    protected override string GetReportDefault()
    {
        return "EKAI_Report_Wear_Default".Translate();
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

        var gotoStation = Toils_Goto.GotoThing(TargetIndex, PathEndMode.Touch);
        yield return Toils.JumpIfNeedNotGoto(gotoStation, TargetLinkedContainer);
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
        yield return Toils.TakeApparelFromContainerAndWear(TargetApparel, TargetLinkedContainer, true);
        yield return Toils_Jump.Jump(setWearOnLocker);
        yield return putToil;
        yield return gotoStation;
        var setRemove = Toils.SetRemoveApparel(TargetApparel, TargetIndex);
        yield return setRemove;
        var gotoStationPos = Toils_Goto.GotoThing(TargetIndex, PowerArmorStation.GetStandbyPosition());
        yield return Toils_Jump.JumpIfTargetInvalid(TargetApparel, gotoStationPos);
        yield return Toils.SetProgress(Progress.REMOVING);
        yield return Toils.WaitEquipDelay(TargetApparel, TargetIndex,
            Global.FactorEquipdelayWhenWearFavorite);
        yield return Toils.RemoveAndDropApparel(TargetApparel);
        yield return Toils_Jump.Jump(setRemove);
        yield return gotoStationPos;
        yield return Toils.WaitForStationToChange(TargetIndex, 180);
        yield return Toils.TakeAllApparelFromLockerAndWear(TargetIndex);
    }

    public override Apparel GetNextWearApparel(TargetIndex containerInd)
    {
        if (containerInd != TargetLinkedContainer)
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
        var list = Util.SortApparelListForDraw(pawn.apparel.UnlockedApparel, true);
        if (containerInd == TargetIndex)
        {
            var apparelsRegisterdAndInner = CompLockerStation.GetApparelsRegisterdAndInner();
            foreach (var item in list)
            {
                if (Util.AnyCantWearTogetherApparels(apparelsRegisterdAndInner, item))
                {
                    return item;
                }
            }

            return null;
        }

        if (containerInd != TargetLinkedContainer)
        {
            throw new ArgumentException();
        }

        var apparelsRegisterdAndInner2 = CompLockerFavLocker.GetApparelsRegisterdAndInner();
        foreach (var item2 in list)
        {
            if (Util.AnyCantWearTogetherApparels(apparelsRegisterdAndInner2, item2) ||
                Util.AnyCantWearTogetherApparels(CompLockerStation.GetApparelsRegisterdAndInner(), item2))
            {
                return item2;
            }
        }

        return null;
    }

    public override bool NeedGoto(TargetIndex containerInd)
    {
        switch (containerInd)
        {
            case TargetIndex:
                return true;
            case TargetLinkedContainer:
                return needGoToLocker();
            default:
                throw new ArgumentException();
        }
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