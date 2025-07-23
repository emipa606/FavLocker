using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Locker;

public static class JobUtil
{
    public static bool AnyPawnHasHaulJob(Building_RegistableContainer container)
    {
        foreach (var item in Util.AllPawnsPotentialOwner(container.Map))
        {
            if (WorkGiver_LoadLockers.HasJobOnTransporter(item, container.TryGetComp<CompLocker>()))
            {
                return true;
            }
        }

        return false;
    }

    public static void EndCurrentAndQueuedJobOnContainer(Building_RegistableContainer container,
        bool doLinkedContainerTogether = false)
    {
        EndCurrentJobOnContainer(container);
        EndQueuedJobOnContainer(container);
        if (!doLinkedContainerTogether || !container.AnyLinkedContainer<Building_RegistableContainer>())
        {
            return;
        }

        var linkedContainer = container.GetLinkedContainer<Building_RegistableContainer>();
        EndCurrentJobOnContainer(linkedContainer);
        EndQueuedJobOnContainer(linkedContainer);
    }

    private static void EndCurrentJobOnContainer(Building_RegistableContainer container)
    {
        foreach (var item in Util.AllPawnsPotentialOwner(container.Map))
        {
            foreach (var allJobDef in container.GetAllJobDefs())
            {
                if (!IsDoingJobOnThing(item, container, allJobDef))
                {
                    continue;
                }

                item.jobs.EndCurrentJob(JobCondition.InterruptForced);
                break;
            }
        }
    }

    private static void EndQueuedJobOnContainer(Building_RegistableContainer container)
    {
        foreach (var item in Util.AllPawnsPotentialOwner(container.Map))
        {
            foreach (var allJobDef in container.GetAllJobDefs())
            {
                EndQueuedJobOnContainer(container, item, allJobDef);
            }
        }
    }

    private static void EndQueuedJobOnContainer(Building_RegistableContainer container, Pawn pawn, JobDef jobDef)
    {
        while (true)
        {
            var jobFromQueue = getJobFromQueue(pawn, container, jobDef);
            if (jobFromQueue == null)
            {
                break;
            }

            pawn.jobs.EndCurrentOrQueuedJob(jobFromQueue, JobCondition.InterruptForced);
        }
    }

    public static bool AnyDoingOrQueuedJobOnThing(Thing target, JobDef jobDef)
    {
        return AnyDoingJobOnThing(target, jobDef) || AnyQueuedJobOnThing(target, jobDef);
    }

    private static bool AnyQueuedJobOnThing(Thing target, JobDef jobDef)
    {
        foreach (var item in Util.AllPawnsPotentialOwner(target.Map))
        {
            if (AnyQueuedJobOnThing(item, target, jobDef))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AnyQueuedJobOnThing(Pawn pawn, Thing target, JobDef jobDef)
    {
        return getJobFromQueue(pawn, target, jobDef) != null;
    }

    private static Job getJobFromQueue(Pawn pawn, Thing target, JobDef jobDef)
    {
        var jobQueue = pawn.jobs.jobQueue;
        foreach (var item in jobQueue)
        {
            var job = item.job;
            if (job.def != jobDef || !isTargetThing(job, target))
            {
                continue;
            }

            return job;
        }

        return null;
    }

    public static bool AnyDoingJobOnThing(Thing dest, JobDef jobDef)
    {
        return GetDoingJobOnThing(dest, jobDef).Count > 0;
    }

    private static List<Pawn> GetDoingJobOnThing(Thing dest, JobDef jobDef)
    {
        var list = new List<Pawn>();
        foreach (var item in Util.AllPawnsPotentialOwner(dest.Map))
        {
            if (IsDoingJobOnThing(item, dest, jobDef))
            {
                list.Add(item);
            }
        }

        return list;
    }

    private static bool IsDoingJobOnThing(Pawn pawn, Thing dest, JobDef jobDef)
    {
        if (pawn.CurJobDef != jobDef)
        {
            return false;
        }

        if (pawn.jobs.curDriver is not IGettableDestination gettableDestination)
        {
            return false;
        }

        return gettableDestination.GetDestination() == dest;
    }

    public static List<Thing> GetCarryThingsToDest(Thing dest)
    {
        var list = new List<Thing>();
        foreach (var item in Util.AllPawnsPotentialOwner(dest.Map))
        {
            var carryThingToDest = getCarryThingToDest(item, dest);
            if (carryThingToDest != null)
            {
                list.Add(carryThingToDest);
            }
        }

        return list;
    }

    private static Thing getCarryThingToDest(Pawn pawn, Thing dest)
    {
        return IsDoingJobOnThing(pawn, dest, JobDefOf.EKAI_HaulToLocker) ? pawn.carryTracker.CarriedThing : null;
    }

    public static List<Thing> GetAssignedRegisterdApparel(Thing dest)
    {
        var list = new List<Thing>();
        foreach (var item in Util.AllPawnsPotentialOwner(dest.Map))
        {
            var assignedThingToDest = getAssignedThingToDest(item, dest);
            if (assignedThingToDest != null)
            {
                list.Add(assignedThingToDest);
            }
        }

        return list;
    }

    private static Thing getAssignedThingToDest(Pawn pawn, Thing dest)
    {
        return IsDoingJobOnThing(pawn, dest, JobDefOf.EKAI_HaulToLocker)
            ? ((JobDriver_HaulToLocker)pawn.jobs.curDriver).ThingToCarry
            : null;
    }

    private static bool isTargetThing(Job job, Thing thing)
    {
        return job.targetA.Thing == thing || job.targetB.Thing == thing || job.targetC.Thing == thing;
    }
}