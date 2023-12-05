using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Locker;

internal class Dialog_RegisterItem : Window
{
    private static readonly List<TabRecord> tabsList = [];

    private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

    private readonly CompAssignableToPawn_Locker compAssignable;

    private readonly CompLocker compLocker;

    private readonly Building_RegistableContainer container;
    private readonly Map map;

    private List<LockerApparel> allApparel;

    private LockerApparelWidget itemsTransfer;

    public Dialog_RegisterItem(Map map, Building_RegistableContainer container)
    {
        this.map = map;
        this.container = container;
        compLocker = container.GetComp<CompLocker>();
        compAssignable = container.GetComp<CompAssignableToPawn_Locker>();
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

    public override float Margin => 0f;

    public override void PostOpen()
    {
        base.PostOpen();
        CalculateAndRecacheTransferables();
    }

    public override void DoWindowContents(Rect inRect)
    {
        var rect = new Rect(0f, 0f, inRect.width, 35f);
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleCenter;
        var text = compAssignable.AssignedPawn()?.Label ?? "Nobody".Translate();
        var label = compLocker.Props.dialogTitleKey.Translate(text);
        Widgets.Label(rect, label);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        tabsList.Clear();
        tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate { }, true));
        inRect.yMin += 67f;
        Widgets.DrawMenuSection(inRect);
        TabDrawer.DrawTabs(inRect, tabsList);
        inRect = inRect.ContractedBy(17f);
        GUI.BeginGroup(inRect);
        var rect2 = inRect.AtZero();
        DoBottomButtons(rect2);
        var inRect2 = rect2;
        inRect2.yMax -= 59f;
        itemsTransfer.OnGUI(inRect2);
        GUI.EndGroup();
    }

    public override bool CausesMessageBackground()
    {
        return true;
    }

    private void DoBottomButtons(Rect rect)
    {
        var rect2 = new Rect((rect.width / 2f) - (BottomButtonSize.x / 2f), rect.height - 55f, BottomButtonSize.x,
            BottomButtonSize.y);
        if (Widgets.ButtonText(rect2, "EKAI_Register".Translate()) && TryAccept())
        {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            Close(false);
        }

        if (Widgets.ButtonText(
                new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y),
                "ResetButton".Translate()))
        {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            CalculateAndRecacheTransferables();
        }

        if (Widgets.ButtonText(new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y),
                "CancelButton".Translate()))
        {
            Close();
        }
    }

    private void CalculateAndRecacheTransferables()
    {
        allApparel = [];
        AddApparelOnMap();
        AddApparelInner();
        AddApparelLoading();
        AddWornApparel();
        MarkRegisteredApparel();
        SetCautionMessage();
        RemoveRegisteredApparelsByOtherContainer();
        InitWidget();
    }

    private bool TryAccept()
    {
        if (!CheckForErrors())
        {
            return false;
        }

        compLocker.UnregisterAll();
        foreach (var item in allApparel)
        {
            if (item.Registerd)
            {
                compLocker.RegisterApparel(item.Contents);
            }
        }

        compLocker.resetNotifiedCantLoadMore();
        JobUtil.EndCurrentAndQueuedJobOnContainer(container, true);
        return true;
    }

    private void AddApparelOnMap()
    {
        var source = CaravanFormingUtility.AllReachableColonyItems(map, false, true);
        foreach (var item in source.Where(t =>
                     t is Apparel apparel && (apparel.Wearer == null || !apparel.Wearer.apparel.IsLocked(apparel))))
        {
            allApparel.Add(new LockerApparel((Apparel)item));
        }
    }

    private void AddApparelInner()
    {
        foreach (var item in compLocker.InnerApparelsReadOnly())
        {
            var lockerApparel = new LockerApparel(item)
            {
                Owner = compLocker
            };
            allApparel.Add(lockerApparel);
        }
    }

    private void AddApparelLoading()
    {
        foreach (var item in JobUtil.GetCarryThingsToDest(compLocker.parent))
        {
            allApparel.Add(new LockerApparel((Apparel)item));
        }
    }

    private void AddWornApparel()
    {
        foreach (var wearingPawn in Util.AllPawnsPotentialOwner(compLocker.parent.Map))
        {
            var otherPawnWearing =
                wearingPawn != compLocker.parent.GetComp<CompAssignableToPawn_Locker>().AssignedPawn();
            foreach (var apparelToCheck in wearingPawn.apparel.UnlockedApparel)
            {
                var lockerApparel = new LockerApparel(apparelToCheck)
                {
                    WearingPawn = wearingPawn,
                    OtherPawnWearing = otherPawnWearing
                };
                allApparel.Add(lockerApparel);
            }
        }
    }

    private void MarkRegisteredApparel()
    {
        foreach (var regApparel in compLocker.RegisteredApparelsReadOnly())
        {
            var lockerApparel = allApparel.Find(la => la.Contents == regApparel);
            if (lockerApparel == null)
            {
                var lockerApparel2 = new LockerApparel(regApparel)
                {
                    Unknown = true,
                    Registerd = true
                };
                allApparel.Add(lockerApparel2);
            }
            else
            {
                lockerApparel.Registerd = true;
            }
        }
    }

    private void SetCautionMessage()
    {
        var linkedContainer = container.GetLinkedContainer<Building_RegistableContainer>();
        if (linkedContainer == null)
        {
            return;
        }

        var comp = linkedContainer.GetComp<CompLocker>();
        foreach (var item in allApparel)
        {
            if (!Util.AnyCantWearTogetherApparels(comp.RegisteredApparelsReadOnly(), item.Contents))
            {
                continue;
            }

            item.ConflictWithApparelsRegisterdLinkedContainer = true;
            item.CautionMessage =
                "EKAI_Msg_DuplicationWithApparelRegisterdOwnedContainer".Translate(linkedContainer.def.label);
        }
    }

    private void RemoveRegisteredApparelsByOtherContainer()
    {
        var list = new List<LockerApparel>();
        foreach (var item in allApparel)
        {
            var containersRegisteredApparel =
                Util.GetContainersRegisteredApparel<Building_RegistableContainer>(compLocker.Map, item.Contents);
            if (containersRegisteredApparel.Any(buildingRegistableContainer =>
                    buildingRegistableContainer != compLocker.parent))
            {
                list.Add(item);
            }
        }

        foreach (var item2 in list)
        {
            if (!compLocker.InnerApparelsReadOnly().Contains(item2.Contents))
            {
                allApparel.Remove(item2);
            }
        }
    }

    private void InitWidget()
    {
        itemsTransfer = new LockerApparelWidget(compLocker, null);
        if (!compLocker.Props.storableThingDefs.NullOrEmpty())
        {
            var storableThingDefs = compLocker.Props.storableThingDefs;
            allApparel.RemoveAll(apparel => !storableThingDefs.Contains(apparel.Contents.def));
        }

        var sortedDictionary = new SortedDictionary<LockerSectionDef, List<LockerApparel>>();
        foreach (var item in allApparel)
        {
            var key = LockerSectionDef.Get(item.ThingDef.thingCategories);
            if (!sortedDictionary.TryGetValue(key, out var value))
            {
                value = [];
                sortedDictionary.Add(key, value);
            }

            value.Add(item);
        }

        foreach (var key2 in sortedDictionary.Keys)
        {
            itemsTransfer.AddSection(key2.GetLabel(), sortedDictionary[key2]);
        }
    }

    private bool CheckForErrors()
    {
        var parentMap = compLocker.parent.Map;
        foreach (var item in allApparel)
        {
            if (!item.Registerd)
            {
                continue;
            }

            var hasFoundItem = false;
            Thing t = item.Contents;
            var pawn_CarryTracker = t.ParentHolder as Pawn_CarryTracker;
            if (Util.AllPawnsPotentialOwner(compLocker.Map).Any(p => p.apparel.UnlockedApparel.Contains(t)) ||
                compLocker.InnerApparelsReadOnly().Contains(t) ||
                parentMap.reachability.CanReach(t.Position, compLocker.parent, PathEndMode.Touch,
                    TraverseParms.For(TraverseMode.PassDoors)) ||
                (pawn_CarryTracker?.pawn.MapHeld.reachability.CanReach(pawn_CarryTracker.pawn.PositionHeld,
                    compLocker.parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)) ?? false))
            {
                hasFoundItem = true;
            }

            if (hasFoundItem && !item.Unknown)
            {
                continue;
            }

            Messages.Message(
                "EKAI_Msg_NoPathWithItem".Translate(compLocker.parent.def.label, item.ThingDef.label),
                MessageTypeDefOf.RejectInput, false);
            return false;
        }

        return true;
    }
}