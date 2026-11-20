using RimWorld;
using Verse;

namespace Locker;

public class CompAssignableToPawn_Locker : CompAssignableToPawn
{
    private new Building_RegistableContainer parent => (Building_RegistableContainer)base.parent;

    private new CompProperties_AssignableToPawn_OnlyOne Props => (CompProperties_AssignableToPawn_OnlyOne)props;

    public override bool AssignedAnything(Pawn pawn)
    {
        var map = pawn.MapHeld;
        if (map == null)
        {
            return false;
        }

        foreach (var item in Util.AllMapBuildings<Building_RegistableContainer>(map))
        {
            if (item.GetType() != Props.buildingClass)
            {
                continue;
            }

            var list = item.TryGetComp<CompAssignableToPawn_Locker>().assignedPawns;
            if (!list.NullOrEmpty() && list.Contains(pawn))
            {
                return true;
            }
        }

        return false;
    }

    public bool AnyAssigned()
    {
        return AssignedPawn() != null;
    }

    public Pawn AssignedPawn()
    {
        return assignedPawns.Any() ? assignedPawns[0] : null;
    }

    public bool Assigned(Pawn pawn)
    {
        return assignedPawns.Contains(pawn);
    }

    public override void ForceAddPawn(Pawn pawn)
    {
        TryAssignPawn(pawn);
    }

    public override void ForceRemovePawn(Pawn pawn)
    {
        TryUnassignPawn(pawn);
    }

    public override void TryAssignPawn(Pawn pawn)
    {
        if (Assigned(pawn))
        {
            return;
        }

        var oldOwner = AssignedPawn();
        assignedPawns.Clear();
        var map = pawn.MapHeld;
        if (map != null)
        {
            foreach (var item in Util.AllMapBuildings<Building_RegistableContainer>(map))
            {
                if (item.GetType() != Props.buildingClass)
                {
                    continue;
                }

                var compAssignableToPawn_Locker = item.TryGetComp<CompAssignableToPawn_Locker>();
                compAssignableToPawn_Locker.TryUnassignPawn(pawn);
            }
        }

        assignedPawns.Add(pawn);
        parent.ChangeOwner(oldOwner, pawn);
    }

    public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
    {
        if (!Assigned(pawn))
        {
            return;
        }

        assignedPawns.Remove(pawn);
        parent.ChangeOwner(pawn, null);
    }

    public override string GetAssignmentGizmoDesc()
    {
        return "EKAI_Desc_AssignPawn".Translate(parent.def.label);
    }
}