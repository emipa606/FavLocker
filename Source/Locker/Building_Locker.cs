using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Locker;

public class Building_Locker : Building_RegistableContainer
{
    private static readonly ReadOnlyCollection<JobDef> allDefs;

    static Building_Locker()
    {
        allDefs = new ReadOnlyCollection<JobDef>(new List<JobDef>
        {
            JobDefOf.EKAI_HaulToLocker,
            JobDefOf.EKAI_RemoveApparelWithLocker,
            JobDefOf.EKAI_WearRegisteredApparelWithLocker,
            JobDefOf.EKAI_RemoveRegisteredApparelWithLocker
        });
    }

    public override IReadOnlyList<JobDef> GetAllJobDefs()
    {
        return allDefs;
    }

    public override void SetGizmosWhenSelectedOwner(Pawn owner, ref IEnumerable<Gizmo> gizmos)
    {
        var list = gizmos.ToList();
        setCommandOfWearFavorite(owner, list);
        setCommandOfRemoveFavorite(owner, list);
        gizmos = list.AsEnumerable();
    }

    private void setCommandOfWearFavorite(Pawn owner, List<Gizmo> gizmoList)
    {
        if (AnyLinkedContainer<Building_PowerArmorStation>() ||
            CompLocker.AnyRegisteredApparel && CompLocker.AllWear(owner))
        {
            return;
        }

        var commandWearFavorite = new Command_WearFavorite
        {
            defaultLabel = "EKAI_WearRegisterApparel".Translate(),
            defaultDesc = "EKAI_Desc_WearRegisterApparel".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/WearFav"),
            activateSound = SoundDefOf.Tick_Low,
            favLocker = this,
            pawn = owner
        };
        if (!canCommandOfWearFavorite(owner, out var reason))
        {
            commandWearFavorite.Disable(reason);
        }

        gizmoList.Add(commandWearFavorite);
    }

    private bool canCommandOfWearFavorite(Pawn owner, out string reason)
    {
        reason = "";
        var comp = GetComp<CompLocker>();
        if (!comp.RegisteredApparelsReadOnly().Any())
        {
            reason = "EKAI_Msg_NoRegister".Translate(def.label);
            return false;
        }

        if (!comp.AllStoredOrWear(owner))
        {
            reason = "EKAI_Msg_InCompleteWearOrStore".Translate(def.label, owner.Label);
            return false;
        }

        if (owner.MapHeld.reachability.CanReach(owner.PositionHeld, this, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors)))
        {
            return true;
        }

        reason = "EKAI_Msg_NoPath".Translate(def.label);
        return false;
    }

    private void setCommandOfRemoveFavorite(Pawn owner, List<Gizmo> gizmoList)
    {
        if (AnyLinkedContainer<Building_PowerArmorStation>() || !CompLocker.AnyWear(owner))
        {
            return;
        }

        var commandRemoveFavorite = new Command_RemoveFavorite
        {
            defaultLabel = "EKAI_RemoveRegisterApparel".Translate(),
            defaultDesc = "EKAI_Desc_RemoveRegisterApparel".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/RemoveFav"),
            activateSound = SoundDefOf.Tick_Low,
            favLocker = this,
            pawn = owner
        };
        if (!canCommandOfRemoveFavorite(owner, out var reason))
        {
            commandRemoveFavorite.Disable(reason);
        }

        gizmoList.Add(commandRemoveFavorite);
    }

    private bool canCommandOfRemoveFavorite(Pawn owner, out string reason)
    {
        reason = "";
        if (owner.MapHeld.reachability.CanReach(owner.PositionHeld, this, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors)))
        {
            return true;
        }

        reason = "EKAI_Msg_NoPath".Translate(def.label);
        return false;
    }
}