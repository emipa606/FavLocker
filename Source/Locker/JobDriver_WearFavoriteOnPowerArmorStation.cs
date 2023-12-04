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

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TARGET_CONTAINER);
        this.FailOnOwnerStatus(TARGET_CONTAINER);
        if (FavLocker != null)
        {
            this.FailOnDespawnedNullOrForbidden(TARGET_LINKED_CONTAINER);
            this.FailOnOwnerStatus(TARGET_LINKED_CONTAINER);
        }

        var gotoStation = Toils_Goto.GotoThing(TARGET_CONTAINER, PathEndMode.Touch);
        yield return Toils.JumpIfNeedNotGoto(gotoStation, TARGET_LINKED_CONTAINER);
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
        yield return Toils.TakeApparelFromContainerAndWear(TARGET_APPAREL, TARGET_LINKED_CONTAINER, true);
        yield return Toils_Jump.Jump(setWearOnLocker);
        yield return putToil;
        yield return gotoStation;
        var setRemove = Toils.SetRemoveApparel(TARGET_APPAREL, TARGET_CONTAINER);
        yield return setRemove;
        var gotoStationPos = Toils_Goto.GotoThing(TARGET_CONTAINER, PowerArmorStation.GetStandbyPosition());
        yield return Toils_Jump.JumpIfTargetInvalid(TARGET_APPAREL, gotoStationPos);
        yield return Toils.SetProgress(Progress.REMOVING);
        yield return Toils.WaitEquipDelay(TARGET_APPAREL, TARGET_CONTAINER,
            Global.FACTOR_EQUIPDELAY_WHEN_WEAR_FAVORITE);
        yield return Toils.RemoveAndDropApparel(TARGET_APPAREL);
        yield return Toils_Jump.Jump(setRemove);
        yield return gotoStationPos;
        yield return Toils.WaitForStationToChange(TARGET_CONTAINER, 180);
        yield return Toils.TakeAllApparelFromLockerAndWear(TARGET_CONTAINER);
    }

    public override Apparel GetNextWearApparel(TargetIndex containerInd)
    {
        if (containerInd != TARGET_LINKED_CONTAINER)
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
        if (containerInd == TARGET_CONTAINER)
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

        if (containerInd != TARGET_LINKED_CONTAINER)
        {
            throw new ArgumentException();
        }

        var apparelsRegisterdAndInner2 = CompLockerFavLocker.GetApparelsRegisterdAndInner();
        foreach (var item2 in list)
        {
            if (Util.AnyCantWearTogetherApparels(apparelsRegisterdAndInner2, item2))
            {
                return item2;
            }

            if (Util.AnyCantWearTogetherApparels(CompLockerStation.GetApparelsRegisterdAndInner(), item2))
            {
                return item2;
            }
        }

        return null;
    }

    public override bool NeedGoto(TargetIndex containerInd)
    {
        if (containerInd == TARGET_CONTAINER)
        {
            return true;
        }

        if (containerInd == TARGET_LINKED_CONTAINER)
        {
            return NeedGoToLocker();
        }

        throw new ArgumentException();
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

        return GetNextRemoveApparel(TARGET_LINKED_CONTAINER) != null;
    }
}