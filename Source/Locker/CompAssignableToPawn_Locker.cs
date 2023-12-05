using RimWorld;
using Verse;

namespace Locker;

public class CompAssignableToPawn_Locker : CompAssignableToPawn
{
    public new Building_RegistableContainer parent => (Building_RegistableContainer)base.parent;

    public new CompProperties_AssignableToPawn_OnlyOne Props => (CompProperties_AssignableToPawn_OnlyOne)props;

    public override bool AssignedAnything(Pawn pawn)
    {
        foreach (var item in Util.AllMapBuildings<Building_RegistableContainer>(pawn.Map))
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

    public override void TryAssignPawn(Pawn newOwner)
    {
        if (Assigned(newOwner))
        {
            return;
        }

        var oldOwner = AssignedPawn();
        assignedPawns.Clear();
        foreach (var item in Util.AllMapBuildings<Building_RegistableContainer>(newOwner.Map))
        {
            if (item.GetType() != Props.buildingClass)
            {
                continue;
            }

            var compAssignableToPawn_Locker = item.TryGetComp<CompAssignableToPawn_Locker>();
            compAssignableToPawn_Locker.TryUnassignPawn(newOwner);
        }

        assignedPawns.Add(newOwner);
        parent.ChangeOwner(oldOwner, newOwner);
    }

    public override void TryUnassignPawn(Pawn oldOwner, bool sort = true, bool uninstall = false)
    {
        if (!Assigned(oldOwner))
        {
            return;
        }

        assignedPawns.Remove(oldOwner);
        parent.ChangeOwner(oldOwner, null);
    }

    public override string GetAssignmentGizmoDesc()
    {
        return "EKAI_Desc_AssignPawn".Translate(parent.def.label);
    }
}