using System.Collections.Generic;
using System.Text;
using Verse;

namespace Locker;

public abstract class Building_RegistableContainer : Building
{
    public CompAssignableToPawn_Locker CompAssignableToPawn => GetComp<CompAssignableToPawn_Locker>();

    public List<Pawn> OwnersForReading => CompAssignableToPawn.AssignedPawnsForReading;

    private bool PlayerCanSeeOwners => CompAssignableToPawn.PlayerCanSeeAssignments;

    protected CompLocker CompLocker => GetComp<CompLocker>();

    public Thing FirstThingLeftToLoad
    {
        get
        {
            var pawn = GetComp<CompAssignableToPawn_Locker>().AssignedPawn();
            var comp = GetComp<CompLocker>();
            foreach (var item in comp.RegisteredApparelsReadOnly())
            {
                var hasItem = comp.InnerApparelsReadOnly().Contains(item) ||
                              (pawn?.apparel.WornApparel.Contains(item) ?? false);

                if (!hasItem)
                {
                    return item;
                }
            }

            return null;
        }
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (!PlayerCanSeeOwners)
        {
            return stringBuilder.ToString();
        }

        if (OwnersForReading.Count == 0)
        {
            stringBuilder.AppendInNewLine("Owner".Translate() + ": " + "Nobody".Translate());
        }
        else if (OwnersForReading.Count == 1)
        {
            stringBuilder.AppendInNewLine("Owner".Translate() + ": " + OwnersForReading[0].Label);
        }

        return stringBuilder.ToString();
    }

    public abstract IReadOnlyList<JobDef> GetAllJobDefs();

    public bool AnyLinkedContainer<T>() where T : Building_RegistableContainer
    {
        return GetLinkedContainer<T>() != null;
    }

    public T GetLinkedContainer<T>() where T : Building_RegistableContainer
    {
        if (!CompAssignableToPawn.AnyAssigned())
        {
            return null;
        }

        var pawn = CompAssignableToPawn.AssignedPawn();
        foreach (var item in Util.AllMapBuildings<T>(Map))
        {
            if (item == this || !item.CompAssignableToPawn.Assigned(pawn))
            {
                continue;
            }

            return item;
        }

        return null;
    }

    public abstract void SetGizmosWhenSelectedOwner(Pawn owner, ref IEnumerable<Gizmo> gizmos);

    public virtual void ChangeContents()
    {
    }

    public virtual void ChangeOwner(Pawn oldOwner, Pawn newOwner)
    {
    }
}