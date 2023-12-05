using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

internal class JobDriver_HaulToLocker : JobDriver_HaulToContainer, IGettableDestination
{
    private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.A).Thing;

    public CompLocker Locker => Container?.TryGetComp<CompLocker>();

    public Thing GetDestination()
    {
        return TargetThingB;
    }

    public override string GetReport()
    {
        return "ReportHaulingTo".Translate(TargetThingA.Named("THING"), TargetThingB.Label.Named("DESTINATION"));
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Apparel, job, 1, -1, null, errorOnFailed);
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        job.count = job.targetA.Thing.stackCount;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true);
        yield return Toils_Haul.CarryHauledThingToContainer();
        yield return Toils.DepositHauledThingInContainer(TargetIndex.B);
    }
}