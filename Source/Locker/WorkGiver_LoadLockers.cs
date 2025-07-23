using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker;

public class WorkGiver_LoadLockers : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        return Util.AllMapBuildings<Building_RegistableContainer>(pawn.Map);
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var compLocker = t.TryGetComp<CompLocker>();
        return HasJobOnTransporter(pawn, compLocker);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var compLocker = t.TryGetComp<CompLocker>();
        var thing = findRemoveApparel(pawn, compLocker);
        if (thing != null)
        {
            var job = JobMaker.MakeJob(JobDefOf.EKAI_RemoveApparelWithLocker, thing, compLocker.parent);
            job.ignoreForbidden = true;
            return job;
        }

        var thing2 = findThingToLoad(pawn, compLocker);
        var job2 = JobMaker.MakeJob(JobDefOf.EKAI_HaulToLocker, thing2, compLocker.parent);
        job2.ignoreForbidden = true;
        return job2;
    }

    public static bool HasJobOnTransporter(Pawn pawn, CompLocker compLocker)
    {
        if (compLocker.parent.IsForbidden(pawn))
        {
            return false;
        }

        if (!compLocker.AnythingLeftToLoad)
        {
            return false;
        }

        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
        {
            return false;
        }

        if (!pawn.CanReach((LocalTargetInfo)compLocker.parent, PathEndMode.Touch, pawn.NormalMaxDanger()))
        {
            return false;
        }

        if (findRemoveApparel(pawn, compLocker) != null)
        {
            return true;
        }

        return findThingToLoad(pawn, compLocker) != null;
    }

    private static Thing findRemoveApparel(Pawn p, CompLocker compLocker)
    {
        if (compLocker.parent.GetComp<CompAssignableToPawn_Locker>().Assigned(p))
        {
            return null;
        }

        foreach (var item in p.apparel.UnlockedApparel)
        {
            if (compLocker.RegisteredApparelsReadOnly().Contains(item))
            {
                return item;
            }
        }

        return null;
    }

    private static Thing findThingToLoad(Pawn p, CompLocker compLocker)
    {
        var neededThings = new HashSet<Thing>();
        var assignedRegisterdApparel = JobUtil.GetAssignedRegisterdApparel(compLocker.parent);
        foreach (var item in compLocker.RegisteredApparelsReadOnly())
        {
            if (!compLocker.InnerApparelsReadOnly().Contains(item) && !assignedRegisterdApparel.Contains(item))
            {
                neededThings.Add(item);
            }
        }

        if (!neededThings.Any())
        {
            return null;
        }

        return GenClosest.ClosestThingReachable(p.Position, p.Map,
            ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p),
            9999f,
            thing => neededThings.Contains(thing) && p.CanReserve(thing) &&
                     compLocker.HealthRange.Includes((float)thing.HitPoints / thing.MaxHitPoints));
    }
}