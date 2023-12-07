using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace Locker;

public static class Util
{
    public static IEnumerable<T> AllMapBuildings<T>(Map map) where T : Building
    {
        return map.listerBuildings.AllBuildingsColonistOfClass<T>();
    }

    public static IEnumerable<Pawn> AllPawnsPotentialOwner(Map m)
    {
        return m.mapPawns.AllPawnsSpawned.Where(p => p.IsColonist);
    }

    public static IEnumerable<Pawn> AllPawnsPotentialLoad(Map m)
    {
        return m.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
    }

    public static T GetOwnsContainer<T>(Pawn pawn) where T : Building_RegistableContainer
    {
        foreach (var item in AllMapBuildings<T>(pawn.Map))
        {
            if (item.GetComp<CompAssignableToPawn_Locker>().Assigned(pawn))
            {
                return item;
            }
        }

        return null;
    }

    public static List<T> GetContainersRegisteredApparel<T>(Map map, Thing thing)
        where T : Building_RegistableContainer
    {
        var list = new List<T>();
        foreach (var item in AllMapBuildings<T>(map))
        {
            if (item.GetComp<CompLocker>().RegisteredApparelsReadOnly().Contains(thing))
            {
                list.Add(item);
            }
        }

        return list;
    }

    public static void NotifyCommand(Gizmo gizmo)
    {
        var typeFromHandle = typeof(GizmoGridDrawer);
        var field = typeFromHandle.GetField("firstGizmos", BindingFlags.Static | BindingFlags.NonPublic);
        var list = (List<Gizmo>)field?.GetValue(typeFromHandle);
        if (list != null && !list.Contains(gizmo))
        {
            return;
        }

        var field2 = typeFromHandle.GetField("gizmoGroups", BindingFlags.Static | BindingFlags.NonPublic);
        var list2 = (List<List<Gizmo>>)field2?.GetValue(typeFromHandle);
        List<Gizmo> list3 = null;
        if (list2 != null)
        {
            foreach (var gizmos in list2)
            {
                if (!gizmos.Contains(gizmo))
                {
                    continue;
                }

                list3 = gizmos;
                break;
            }
        }

        var stringBuilder = new StringBuilder();
        var num = 0;
        if (list3 != null)
        {
            foreach (var item in list3)
            {
                if (item == gizmo || !item.disabled && gizmo.InheritInteractionsFrom(item))
                {
                    continue;
                }

                num++;
                if (num <= Global.DISPLAY_NUM_CANT_CHANGE_CLOTHES_PAWN_NAME)
                {
                    if (num > 1)
                    {
                        stringBuilder.Append("  ");
                    }

                    string value = null;
                    if (item is Command_WearFavorite command_WearFavorite)
                    {
                        value = command_WearFavorite.pawn.LabelNoCount;
                    }

                    if (item is Command_RemoveFavorite command_RemoveFavorite)
                    {
                        value = command_RemoveFavorite.pawn.LabelNoCount;
                    }

                    stringBuilder.Append(value);
                }
                else if (num == Global.DISPLAY_NUM_CANT_CHANGE_CLOTHES_PAWN_NAME + 1)
                {
                    stringBuilder.Append("...");
                }
            }
        }

        if (num > 0)
        {
            Messages.Message("EKAI_Msg_CantWear".Translate(num, stringBuilder.ToString()),
                MessageTypeDefOf.RejectInput, false);
        }
    }

    public static List<Apparel> AndList(IEnumerable<Apparel> apparels1, IEnumerable<Apparel> apparels2)
    {
        var list = new List<Apparel>();
        foreach (var item in apparels1)
        {
            if (apparels2.Contains(item))
            {
                list.Add(item);
            }
        }

        return list;
    }

    public static List<Apparel> SubstractList(IEnumerable<Apparel> apparels1, IEnumerable<Apparel> apparels2)
    {
        var list = apparels1.ToList();
        list.RemoveAll(apparels2.Contains);
        return list;
    }

    public static List<Apparel> GetCantWearTogetherApparels(IEnumerable<Apparel> target,
        IEnumerable<Apparel> checkApparels)
    {
        var list = new List<Apparel>();
        foreach (var item in target)
        {
            if (AnyCantWearTogetherApparels(checkApparels, item))
            {
                list.Add(item);
            }
        }

        return list;
    }

    public static List<Apparel> GetCantWearTogetherApparels(IEnumerable<Apparel> target, Thing t)
    {
        var list = new List<Apparel>();
        foreach (var item in target)
        {
            if (!ApparelUtility.CanWearTogether(t.def, item.def, BodyDefOf.Human))
            {
                list.Add(item);
            }
        }

        return list;
    }

    public static bool AnyCantWearTogetherApparels(IEnumerable<Apparel> target, Thing t)
    {
        return GetCantWearTogetherApparels(target, t).Count > 0;
    }

    public static bool AnyCantWearTogetherApparelsExcludingItSelf(IEnumerable<Apparel> target, Thing t)
    {
        var cantWearTogetherApparels = GetCantWearTogetherApparels(target, t);
        var num = cantWearTogetherApparels.Count;
        if (cantWearTogetherApparels.Contains(t))
        {
            num--;
        }

        return num > 0;
    }

    public static List<Apparel> SortApparelListForDraw(IEnumerable<Apparel> orgApparels, bool reverse = false)
    {
        var list = new List<Apparel>(orgApparels);
        if (reverse)
        {
            list.Sort((a, b) =>
                b.def.apparel.LastLayer.drawOrder.CompareTo(a.def.apparel.LastLayer.drawOrder));
        }
        else
        {
            list.Sort((a, b) =>
                a.def.apparel.LastLayer.drawOrder.CompareTo(b.def.apparel.LastLayer.drawOrder));
        }

        return list;
    }

    public static Transferable Transform(LockerApparel source)
    {
        var transferableOneWay = new TransferableOneWay();
        transferableOneWay.things.Add(source.Contents);
        return transferableOneWay;
    }

    public static void SyncTechSetting()
    {
        CompProperties refuelableComp;
        var def = ThingDef.Named("EKAI_PowerArmorStation");
        var updateBuildings = false;
        if (ResearchProjectDef.Named("AdvancedFabrication").IsFinished)
        {
            if (!def.HasComp(typeof(CompPowerTrader)))
            {
                var powerComp = new CompProperties_Power
                    { compClass = typeof(CompPowerTrader), basePowerConsumption = 500 };
                def.comps.Add(powerComp);
                updateBuildings = true;
            }

            if (!def.HasComp(typeof(CompFlickable)))
            {
                var flickableComp = new CompProperties_Flickable
                    { compClass = typeof(CompFlickable) };
                def.comps.Add(flickableComp);
                updateBuildings = true;
            }

            if (!def.HasComp(typeof(CompRefuelable)))
            {
                refuelableComp = new CompProperties_Refuelable
                {
                    compClass = typeof(CompRefuelable),
                    fuelConsumptionRate = 0,
                    initialFuelPercent = 0,
                    showAllowAutoRefuelToggle = true,
                    consumeFuelOnlyWhenUsed = true,
                    initialAllowAutoRefuel = false,
                    fuelCapacity = 150,
                    fuelFilter = new ThingFilter { thingDefs = [ThingDefOf.Plasteel] },
                    fuelIconPath = "UI/Icons/Fuel",
                    fuelLabel = "EKAI_Fuel".Translate()
                };
                def.comps.Add(refuelableComp);
                updateBuildings = true;
            }

            if (!updateBuildings)
            {
                return;
            }

            updateBuiltStations(def, true);
            Messages.Message("EKAI_Msg_CanRepair".Translate(), MessageTypeDefOf.PositiveEvent, false);

            return;
        }

        if (def.HasComp(typeof(CompPowerTrader)))
        {
            var powerComp =
                def.comps.First(properties => properties.GetType() == typeof(CompProperties_Power));
            def.comps.Remove(powerComp);
            updateBuildings = true;
        }

        if (def.HasComp(typeof(CompFlickable)))
        {
            var flickableComp =
                def.comps.First(properties => properties.GetType() == typeof(CompProperties_Flickable));
            def.comps.Remove(flickableComp);
            updateBuildings = true;
        }

        if (def.HasComp(typeof(CompRefuelable)))
        {
            refuelableComp = def.comps.First(properties => properties.GetType() == typeof(CompProperties_Refuelable));
            def.comps.Remove(refuelableComp);
            updateBuildings = true;
        }

        if (updateBuildings)
        {
            updateBuiltStations(def, false);
        }
    }

    private static void updateBuiltStations(ThingDef stationDef, bool adding)
    {
        foreach (var map in Current.Game.Maps)
        {
            var stationsInMap = map.listerBuildings.AllBuildingsColonistOfDef(stationDef);
            if (!stationsInMap.Any())
            {
                continue;
            }

            foreach (var station in stationsInMap)
            {
                if (adding)
                {
                    foreach (var compProperties in station.def.comps)
                    {
                        if (compProperties.GetType() != typeof(CompProperties_Power) &&
                            compProperties.GetType() != typeof(CompProperties_Flickable) &&
                            compProperties.GetType() != typeof(CompProperties_Refuelable))
                        {
                            continue;
                        }

                        ThingComp thingComp = null;
                        try
                        {
                            thingComp = (ThingComp)Activator.CreateInstance(compProperties.compClass);
                            thingComp.parent = station;
                            station.comps.Add(thingComp);
                            thingComp.Initialize(compProperties);
                        }
                        catch (Exception arg)
                        {
                            Log.Error($"Could not instantiate or initialize a ThingComp: {arg}");
                            station.comps.Remove(thingComp);
                        }
                    }

                    continue;
                }

                station.comps = station.comps.Where(comp => comp.GetType() != typeof(CompPowerTrader) &&
                                                            comp.GetType() != typeof(CompFlickable) &&
                                                            comp.GetType() != typeof(CompRefuelable))
                    .ToList();
            }
        }
    }
}