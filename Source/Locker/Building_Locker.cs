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
    private static readonly ReadOnlyCollection<JobDef> ALL_DEFS;

    static Building_Locker()
    {
        ALL_DEFS = new ReadOnlyCollection<JobDef>(new List<JobDef>
        {
            JobDefOf.EKAI_HaulToLocker,
            JobDefOf.EKAI_RemoveApparelWithLocker,
            JobDefOf.EKAI_WearRegisteredApparelWithLocker,
            JobDefOf.EKAI_RemoveRegisteredApparelWithLocker
        });
    }

    public override IReadOnlyList<JobDef> GetAllJobDefs()
    {
        return ALL_DEFS;
    }

    public override void SetGizmosWhenSelectedOwner(Pawn owner, ref IEnumerable<Gizmo> gizmos)
    {
        var list = gizmos.ToList();
        SetCommandOfWearFavorite(owner, list);
        SetCommandOfRemoveFavorite(owner, list);
        gizmos = list.AsEnumerable();
    }

    private void SetCommandOfWearFavorite(Pawn owner, List<Gizmo> gizmoList)
    {
        if (AnyLinkedContainer<Building_PowerArmorStation>() ||
            CompLocker.AnyRegisteredApparel && CompLocker.AllWear(owner))
        {
            return;
        }

        var command_WearFavorite = new Command_WearFavorite
        {
            defaultLabel = "EKAI_WearRegisterApparel".Translate(),
            defaultDesc = "EKAI_Desc_WearRegisterApparel".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/WearFav"),
            activateSound = SoundDefOf.Tick_Low,
            favLocker = this,
            pawn = owner
        };
        if (!CanCommandOfWearFavorite(owner, out var reason))
        {
            command_WearFavorite.Disable(reason);
        }

        gizmoList.Add(command_WearFavorite);
    }

    private bool CanCommandOfWearFavorite(Pawn owner, out string reason)
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

    private void SetCommandOfRemoveFavorite(Pawn owner, List<Gizmo> gizmoList)
    {
        if (AnyLinkedContainer<Building_PowerArmorStation>() || !CompLocker.AnyWear(owner))
        {
            return;
        }

        var command_RemoveFavorite = new Command_RemoveFavorite
        {
            defaultLabel = "EKAI_RemoveRegisterApparel".Translate(),
            defaultDesc = "EKAI_Desc_RemoveRegisterApparel".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/RemoveFav"),
            activateSound = SoundDefOf.Tick_Low,
            favLocker = this,
            pawn = owner
        };
        if (!CanCommandOfRemoveFavorite(owner, out var reason))
        {
            command_RemoveFavorite.Disable(reason);
        }

        gizmoList.Add(command_RemoveFavorite);
    }

    private bool CanCommandOfRemoveFavorite(Pawn owner, out string reason)
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