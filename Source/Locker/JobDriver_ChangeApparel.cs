using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

public abstract class JobDriver_ChangeApparel : JobDriver, IGettableDestination
{
    public enum Progress
    {
        NONE,
        REMOVING,
        WEARING
    }

    protected const TargetIndex TargetApparel = TargetIndex.A;

    protected const TargetIndex TargetIndex = Verse.AI.TargetIndex.B;

    protected const TargetIndex TargetLinkedContainer = TargetIndex.C;

    protected const TargetIndex TargetQueueWornApparelsAtExec = TargetIndex.A;

    private int duration;

    public Progress progress = Progress.NONE;

    protected Building_RegistableContainer TargetContainer =>
        (Building_RegistableContainer)job.GetTarget(TargetIndex).Thing;

    protected CompLocker TargetCompLocker => TargetContainer.GetComp<CompLocker>();

    protected Building_RegistableContainer LinkedContainer =>
        (Building_RegistableContainer)job.GetTarget(TargetLinkedContainer).Thing;

    protected CompLocker LinkedCompLocker => LinkedContainer?.GetComp<CompLocker>();

    public Thing GetDestination()
    {
        return TargetContainer;
    }

    public override string GetReport()
    {
        var thing = job.GetTarget(TargetApparel).Thing;
        switch (progress)
        {
            case Progress.REMOVING when thing != null:
                return "EKAI_Report_Remove".Translate(thing.Label);
            case Progress.WEARING when thing != null:
                return "EKAI_Report_Wear".Translate(thing.Label);
            default:
                return GetReportDefault();
        }
    }

    protected virtual string GetReportDefault()
    {
        return "";
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        var targetQueue = job.GetTargetQueue(TargetQueueWornApparelsAtExec);
        if (targetQueue.Any())
        {
            return;
        }

        job.countQueue = [];
        foreach (var item in pawn.apparel.UnlockedApparel)
        {
            targetQueue.Add(item);
            var valueOrDefault = (pawn.outfits?.forcedHandler?.IsForced(item)).GetValueOrDefault();
            job.countQueue.Add(valueOrDefault ? 1 : 0);
        }
    }

    public virtual bool NeedGoto(TargetIndex containerInd)
    {
        if (containerInd == TargetIndex)
        {
            return true;
        }

        if (containerInd == TargetLinkedContainer)
        {
            return false;
        }

        throw new ArgumentException();
    }

    public virtual Apparel GetNextWearApparel(TargetIndex containerInd)
    {
        throw new NotImplementedException();
    }

    public virtual Apparel GetNextRemoveApparel(TargetIndex containerInd)
    {
        throw new NotImplementedException();
    }

    public void SetCurrentDuration(int duration)
    {
        this.duration = duration;
    }

    public int GetCurrentDuration()
    {
        return duration;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref duration, "duration");
        Scribe_Values.Look(ref progress, "progress");
    }
}