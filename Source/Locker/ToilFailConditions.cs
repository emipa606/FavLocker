using Verse;
using Verse.AI;

namespace Locker;

public static class ToilFailConditions
{
    public static T FailOnOwnerStatus<T>(this T f, TargetIndex containerInd, bool FailOnNotOwner = true)
        where T : IJobEndable
    {
        f.AddEndCondition(delegate
        {
            var actor = f.GetActor();
            var thing = f.GetActor().jobs.curJob.GetTarget(containerInd).Thing;
            var compAssignableToPawnLocker = thing.TryGetComp<CompAssignableToPawn_Locker>();
            if (compAssignableToPawnLocker.Assigned(actor) && !FailOnNotOwner)
            {
                return JobCondition.Incompletable;
            }

            return !(!compAssignableToPawnLocker.Assigned(actor) && FailOnNotOwner)
                ? JobCondition.Ongoing
                : JobCondition.Incompletable;
        });
        return f;
    }
}