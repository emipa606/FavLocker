using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

internal class JobDriver_RemoveApparelWithLocker : JobDriver, IGettableDestination
{
    private int duration;

    private int unequipBuffer;

    private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.A).Thing;

    private ThingWithComps Locker => (ThingWithComps)job.GetTarget(TargetIndex.B).Thing;

    public Thing GetDestination()
    {
        return Locker;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref duration, "duration");
        Scribe_Values.Look(ref unequipBuffer, "unequipBuffer");
    }

    public override string GetReport()
    {
        return "EKAI_Report_RemoveAndStore".Translate(TargetThingA.Label, TargetThingB.Label);
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        duration = (int)(Apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
        this.FailOnOwnerStatus(TargetIndex.B, false);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
        var toil = new Toil();
        toil.tickAction = delegate
        {
            toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget(TargetIndex.B));
            unequipBuffer++;
            tryUnequip();
        };
        toil.WithProgressBarToilDelay(TargetIndex.A);
        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.defaultDuration = duration;
        yield return toil;
    }

    private void tryUnequip()
    {
        var apparel = Apparel;
        if (unequipBuffer < duration - 1)
        {
            return;
        }

        if (pawn.apparel.IsLocked(apparel))
        {
            return;
        }

        pawn.apparel.Remove(apparel);
        Locker.GetComp<CompLocker>().AddApparel(apparel);
    }
}