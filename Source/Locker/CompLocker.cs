using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locker;

[StaticConstructorOnStartup]
public class CompLocker : ThingComp, IThingHolder
{
    private List<Apparel> forcedApparels;
    private FloatRange healthRange;

    private ThingOwner<Apparel> innerContainer;

    private bool notifiedCantLoadMore;

    private List<Apparel> registeredApparels;

    public CompLocker()
    {
        innerContainer = new ThingOwner<Apparel>(this);
        registeredApparels = [];
        forcedApparels = [];
        healthRange = FloatRange.ZeroToOne;
    }

    public new Building_RegistableContainer parent => (Building_RegistableContainer)base.parent;

    public Map Map => parent.MapHeld;

    public CompProperties_Locker Props => (CompProperties_Locker)props;

    public bool AnythingLeftToLoad => FirstThingLeftToLoad != null;

    public FloatRange HealthRange
    {
        get => healthRange;
        set => healthRange = value;
    }

    public Thing FirstThingLeftToLoad => parent.FirstThingLeftToLoad;

    public bool AnyPawnCanLoadAnythingNow
    {
        get
        {
            if (!AnythingLeftToLoad)
            {
                return false;
            }

            if (!parent.Spawned)
            {
                return false;
            }

            if (JobUtil.AnyDoingJobOnThing(parent, JobDefOf.EKAI_HaulToLocker))
            {
                return true;
            }

            return JobUtil.AnyDoingJobOnThing(parent, JobDefOf.EKAI_RemoveApparelWithLocker) ||
                   JobUtil.AnyPawnHasHaulJob(parent);
        }
    }

    public bool AnyRegisteredApparel => registeredApparels.Any();

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public void resetNotifiedCantLoadMore()
    {
        notifiedCantLoadMore = false;
    }

    public ReadOnlyCollection<Apparel> InnerApparelsReadOnly()
    {
        return new ReadOnlyCollection<Apparel>(innerContainer);
    }

    public void AddApparel(Apparel newApparel, bool forced = false)
    {
        foreach (var cantWearTogetherApparel in Util.GetCantWearTogetherApparels(innerContainer, newApparel))
        {
            DropApparel(cantWearTogetherApparel);
        }

        innerContainer.TryAdd(newApparel);
        if (forced)
        {
            forcedApparels.Add(newApparel);
        }

        parent.ChangeContents();
    }

    public void RemoveApparel(Apparel apparel)
    {
        innerContainer.Remove(apparel);
        forcedApparels.Remove(apparel);
        parent.ChangeContents();
    }

    public void DropApparel(Apparel apparel)
    {
        innerContainer.TryDrop(apparel, ThingPlaceMode.Near, out var _);
        forcedApparels.Remove(apparel);
        parent.ChangeContents();
    }

    public bool IsForced(Apparel apparel)
    {
        return forcedApparels.Contains(apparel);
    }

    public bool FindStoredApparel(Thing t)
    {
        return innerContainer.Contains(t);
    }

    public ReadOnlyCollection<Apparel> RegisteredApparelsReadOnly()
    {
        return new ReadOnlyCollection<Apparel>(registeredApparels);
    }

    public void RegisterApparel(Apparel t)
    {
        if (!registeredApparels.Contains(t))
        {
            registeredApparels.Add(t);
        }
    }

    public void UnRegisterApparel(Apparel t)
    {
        if (registeredApparels.Contains(t))
        {
            registeredApparels.Remove(t);
        }
    }

    public void UnregisterAll()
    {
        registeredApparels.Clear();
    }

    public List<Apparel> GetApparelsRegisterdAndInner()
    {
        var list = new List<Apparel>();
        foreach (var registeredApparel in registeredApparels)
        {
            if (innerContainer.Contains(registeredApparel))
            {
                list.Add(registeredApparel);
            }
        }

        return list;
    }

    public bool AllStoredOrWear(Pawn p)
    {
        foreach (var registeredApparel in registeredApparels)
        {
            if (!innerContainer.Contains(registeredApparel) && !p.apparel.UnlockedApparel.Contains(registeredApparel))
            {
                return false;
            }
        }

        return true;
    }

    public bool AllWear(Pawn p)
    {
        foreach (var registeredApparel in registeredApparels)
        {
            if (!p.apparel.UnlockedApparel.Contains(registeredApparel))
            {
                return false;
            }
        }

        return true;
    }

    public bool AnyWear(Pawn p)
    {
        foreach (var registeredApparel in registeredApparels)
        {
            if (p.apparel.UnlockedApparel.Contains(registeredApparel))
            {
                return true;
            }
        }

        return false;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            var list = new List<Apparel>();
            foreach (var registeredApparel in registeredApparels)
            {
                if (registeredApparel.Destroyed)
                {
                    list.Add(registeredApparel);
                }
            }

            foreach (var item in list)
            {
                registeredApparels.Remove(item);
            }
        }

        Scribe_Collections.Look(ref registeredApparels, "registeredApparels", LookMode.Reference);
        Scribe_Collections.Look(ref forcedApparels, "forcedApparels", LookMode.Reference);
        Scribe_Values.Look(ref notifiedCantLoadMore, "notifiedCantLoadMore");
        Scribe_Values.Look(ref healthRange, "healthRange", FloatRange.ZeroToOne);
    }

    public override void CompTick()
    {
        base.CompTick();
        innerContainer.ThingOwnerTick();
        if (!parent.IsHashIntervalTick(60) || !parent.Spawned || !AnythingLeftToLoad || notifiedCantLoadMore ||
            AnyPawnCanLoadAnythingNow)
        {
            return;
        }

        notifiedCantLoadMore = true;
        var text = parent.GetComp<CompAssignableToPawn_Locker>().AssignedPawn()?.Label ?? "Nobody".Translate();
        Messages.Message(
            "EKAI_Msg_CantComplete".Translate(text, FirstThingLeftToLoad.LabelNoCount, parent.def.label,
                Faction.OfPlayer.def.pawnsPlural), parent, MessageTypeDefOf.CautionInput);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_RegisterApparel
        {
            defaultLabel = Props.commandLabelKey.Translate(),
            defaultDesc = Props.commandDescKey.Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/RegisterFav"),
            compLocker = this
        };
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        var list = new List<Apparel>();
        foreach (var registeredApparel in registeredApparels)
        {
            var containersRegisteredApparel =
                Util.GetContainersRegisteredApparel<Building_RegistableContainer>(Map, registeredApparel);
            if (containersRegisteredApparel.Any(container => container != parent))
            {
                list.Add(registeredApparel);
            }
        }

        foreach (var item in list)
        {
            registeredApparels.Remove(item);
        }
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
    }

    public override string CompInspectStringExtra()
    {
        return "Contents".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
    }
}