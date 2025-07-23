using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Locker;

public class Building_PowerArmorStation : Building_RegistableContainer
{
    private static readonly ReadOnlyCollection<JobDef> allDefs;

    private ContentsRenderer contentsRenderer;

    static Building_PowerArmorStation()
    {
        allDefs = new ReadOnlyCollection<JobDef>(new List<JobDef>
        {
            JobDefOf.EKAI_HaulToLocker,
            JobDefOf.EKAI_RemoveApparelWithLocker,
            JobDefOf.EKAI_WearFavoriteOnPowerArmorStation,
            JobDefOf.EKAI_RemoveFavoriteOnPowerArmorStation
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
        if (CompLocker.AnyRegisteredApparel && CompLocker.AllWear(owner))
        {
            return;
        }

        var key = AnyLinkedContainer<Building_Locker>()
            ? "EKAI_Desc_WearFavoritePowerArmor"
            : "EKAI_Desc_WearFavoritePowerArmorNotLink";
        var commandWearFavorite = new Command_WearFavorite
        {
            defaultLabel = "EKAI_WearRegisterApparel".Translate(),
            defaultDesc = key.Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/WearFav"),
            activateSound = SoundDefOf.Tick_Low,
            powerArmorStation = this,
            favLocker = GetLinkedContainer<Building_Locker>(),
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

        if (!owner.MapHeld.reachability.CanReach(owner.PositionHeld, this, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors)))
        {
            reason = "EKAI_Msg_NoPath".Translate(def.label);
            return false;
        }

        var linkedContainer = GetLinkedContainer<Building_Locker>();
        if (linkedContainer == null)
        {
            return true;
        }

        var comp2 = linkedContainer.GetComp<CompLocker>();
        if (!comp2.AllStoredOrWear(owner))
        {
            reason = "EKAI_Msg_InCompleteWearOrStore".Translate(linkedContainer.def.label, owner.Label);
            return false;
        }

        if (owner.MapHeld.reachability.CanReach(owner.PositionHeld, linkedContainer, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors)))
        {
            return true;
        }

        reason = "EKAI_Msg_NoPath".Translate(linkedContainer.def.label);
        return false;
    }

    private void setCommandOfRemoveFavorite(Pawn owner, List<Gizmo> gizmoList)
    {
        if (!CompLocker.RegisteredApparelsReadOnly().Any() || !CompLocker.AnyWear(owner))
        {
            return;
        }

        var key = AnyLinkedContainer<Building_Locker>()
            ? "EKAI_Desc_RemoveFavoritePowerArmor"
            : "EKAI_Desc_RemoveFavoritePowerArmorNotLink";
        var commandRemoveFavorite = new Command_RemoveFavorite
        {
            defaultLabel = "EKAI_RemoveRegisterApparel".Translate(),
            defaultDesc = key.Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/RemoveFav"),
            activateSound = SoundDefOf.Tick_Low,
            powerArmorStation = this,
            favLocker = GetLinkedContainer<Building_Locker>(),
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
        if (!owner.MapHeld.reachability.CanReach(owner.PositionHeld, this, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors)))
        {
            reason = "EKAI_Msg_NoPath".Translate(def.label);
            return false;
        }

        var linkedContainer = GetLinkedContainer<Building_Locker>();
        if (linkedContainer == null)
        {
            return true;
        }

        if (owner.MapHeld.reachability.CanReach(owner.PositionHeld, linkedContainer, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors)))
        {
            return true;
        }

        reason = "EKAI_Msg_NoPath".Translate(linkedContainer.def.label);
        return false;
    }

    public IntVec3 GetStandbyPosition()
    {
        var result = new IntVec3(Position.x, Position.y, Position.z);
        if (Rotation == Rot4.North)
        {
            return result;
        }

        if (Rotation == Rot4.South)
        {
            result.z--;
        }
        else if (Rotation == Rot4.East)
        {
            result.x++;
        }
        else if (Rotation == Rot4.West)
        {
            result.x--;
        }

        return result;
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        contentsRenderer.RenderContents(drawLoc);
    }

    public override void ChangeContents()
    {
        LongEventHandler.ExecuteWhenFinished(delegate { contentsRenderer.ResolveApparelGraphics(); });
    }

    public override void ChangeOwner(Pawn oldOwner, Pawn newOwner)
    {
        base.ChangeOwner(oldOwner, newOwner);
        LongEventHandler.ExecuteWhenFinished(delegate { contentsRenderer.ResolveApparelGraphics(); });
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        contentsRenderer = new ContentsRenderer(this);
        Util.SyncTechSetting();
    }

    public override void Tick()
    {
        base.Tick();

        if (GenTicks.TicksGame % Math.Round(GenTicks.TickRareInterval * 1.25) != 0)
        {
            return;
        }

        var locker = this.TryGetComp<CompLocker>();
        if (locker == null)
        {
            return;
        }

        if (!ResearchProjectDef.Named("AdvancedFabrication").IsFinished)
        {
            return;
        }

        var things = locker.InnerApparelsReadOnly();
        if (!things.Any())
        {
            return;
        }

        var thingsToRepair =
            things.Where(apparel => apparel.def.useHitPoints && apparel.HitPoints < apparel.MaxHitPoints);
        if (!thingsToRepair.Any())
        {
            return;
        }

        var powerComp = this.TryGetComp<CompPowerTrader>();
        if (!powerComp.PowerOn)
        {
            return;
        }

        var fuelComp = this.TryGetComp<CompRefuelable>();
        if (fuelComp.Fuel < 1f)
        {
            return;
        }

        foreach (var apparel in thingsToRepair)
        {
            if (fuelComp.Fuel < 1f)
            {
                return;
            }

            apparel.hitPointsInt = Math.Min(apparel.hitPointsInt + 5, apparel.MaxHitPoints);
            fuelComp.ConsumeFuel(1);
        }
    }
}