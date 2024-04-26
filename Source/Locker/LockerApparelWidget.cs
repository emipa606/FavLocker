using System;
using System.Collections.Generic;
using System.Linq;
using Locker.order;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Locker;

[StaticConstructorOnStartup]
public class LockerApparelWidget
{
    private const float titleRowHeight = 30f;
    private static readonly float SortButtonAreaWidth = 350f;

    private static readonly Color cantSelectColor = new Color(0.5f, 0.5f, 0.5f);

    public static readonly Color MouseoverColorInactive = new Color(0.6f, 0.6f, 0.6f);

    public static readonly Color ColorInactive = new Color(0.4f, 0.4f, 0.4f);

    private readonly TransferableSorterDef bodyPartGroupAndLayerSorterDef;

    private readonly CompLocker compLocker;

    private readonly float extraHeaderSpace;

    private readonly List<Section> sections = [];

    private readonly Texture2D TexCaution = ContentFinder<Texture2D>.Get("UI/Icons/Caution");

    private readonly Texture2D TexChecked = ContentFinder<Texture2D>.Get("UI/Icons/Check");

    private readonly Texture2D TexQuestion = ContentFinder<Texture2D>.Get("UI/Icons/Question");

    private readonly Texture2D TexUnChecked = ContentFinder<Texture2D>.Get("UI/Icons/UnCheck");

    private readonly TransferableSorterDef thingDefNameSorterDef;

    private readonly bool unableDropArrow;

    private Vector2 scrollPosition;

    private bool shouldDisplayHasCaution;

    private bool shouldDisplayOtherPawnEquip;

    private TransferableSorterDef sorter1;

    private TransferableSorterDef sorter2;

    private bool transferablesCached;

    public LockerApparelWidget(CompLocker compLocker, IEnumerable<LockerApparel> transferables,
        float extraHeaderSpace = 0f)
    {
        this.compLocker = compLocker;
        if (transferables != null)
        {
            AddSection(null, transferables);
        }

        this.extraHeaderSpace = extraHeaderSpace;
        sorter1 = bodyPartGroupAndLayerSorterDef = GetBodyPartGropuAndLayerSorterDef();
        sorter2 = thingDefNameSorterDef = GetThingDefNameSorterDef();
        shouldDisplayOtherPawnEquip = false;
        shouldDisplayHasCaution = true;
        unableDropArrow = GetUnableDropArrow();
    }

    private bool AnyTransferable
    {
        get
        {
            if (!transferablesCached)
            {
                CacheTransferables();
            }

            for (var i = 0; i < sections.Count; i++)
            {
                if (sections[i].cachedTransferables.Any())
                {
                    return true;
                }
            }

            return false;
        }
    }

    private TransferableSorterDef GetBodyPartGropuAndLayerSorterDef()
    {
        var transferableSorterDef = new TransferableSorterDef
        {
            defName = "Ekai_TransferableSorter_BodyPartGroupAndLayer",
            label = "EKAI_PartsAndLayerOrder".Translate(),
            comparerClass = typeof(TransferableComparer_BodyPartGroupAndLayer)
        };
        return transferableSorterDef;
    }

    private TransferableSorterDef GetThingDefNameSorterDef()
    {
        var transferableSorterDef = new TransferableSorterDef
        {
            defName = "Ekai_TransferableSorter_ThingDefName",
            label = "EKAI_KindOrder".Translate(),
            comparerClass = typeof(TransferableComparer_ThingDefName)
        };
        return transferableSorterDef;
    }

    private bool GetUnableDropArrow()
    {
        var array = new[]
        {
            JobDefOf.EKAI_WearRegisteredApparelWithLocker,
            JobDefOf.EKAI_RemoveRegisteredApparelWithLocker,
            JobDefOf.EKAI_WearFavoriteOnPowerArmorStation,
            JobDefOf.EKAI_RemoveFavoriteOnPowerArmorStation
        };
        var linkedContainer = compLocker.parent.GetLinkedContainer<Building_RegistableContainer>();
        var hasJob = false;
        foreach (var jobDef in array)
        {
            hasJob |= JobUtil.AnyDoingOrQueuedJobOnThing(compLocker.parent, jobDef);
            if (linkedContainer != null)
            {
                hasJob |= JobUtil.AnyDoingOrQueuedJobOnThing(linkedContainer, jobDef);
            }
        }

        return hasJob;
    }

    public void AddSection(string title, IEnumerable<LockerApparel> transferables)
    {
        var item = default(Section);
        item.title = title;
        item.transferables = transferables;
        item.cachedTransferables = [];
        sections.Add(item);
        transferablesCached = false;
    }

    private void CacheTransferables()
    {
        transferablesCached = true;
        for (var i = 0; i < sections.Count; i++)
        {
            var cachedTransferables = sections[i].cachedTransferables;
            cachedTransferables.Clear();
            cachedTransferables.AddRange(sections[i].transferables
                .OrderBy(tr => tr, new LockerApparelComparer_State())
                .ThenBy(Util.Transform, sorter1.Comparer)
                .ThenBy(Util.Transform, sorter2.Comparer)
                .ThenBy(Util.Transform, new DefaultLockerComparer())
                .ToList());
            foreach (var item in cachedTransferables)
            {
                item.shouldDisplay = ShouldDisplay(item);
            }
        }
    }

    private bool ShouldDisplay(LockerApparel lApparel)
    {
        if (!shouldDisplayOtherPawnEquip && lApparel.OtherPawnWearing && !lApparel.Registerd)
        {
            return false;
        }

        return shouldDisplayHasCaution || lApparel.CautionMessage == null || lApparel.Registerd ||
               lApparel.Owner == compLocker;
    }

    public void OnGUI(Rect inRect)
    {
        if (!transferablesCached)
        {
            CacheTransferables();
        }

        DoTransferableSorters(inRect, sorter1, sorter2, delegate(TransferableSorterDef x)
        {
            sorter1 = x;
            CacheTransferables();
        }, delegate(TransferableSorterDef x)
        {
            sorter2 = x;
            CacheTransferables();
        });
        var mainRect = new Rect(inRect.x, inRect.y + 37f + extraHeaderSpace, inRect.width,
            inRect.height - 37f - extraHeaderSpace);
        FillMainRect(mainRect);
    }

    public void DoTransferableSorters(Rect inRect, TransferableSorterDef sorterDef1,
        TransferableSorterDef sorterDef2,
        Action<TransferableSorterDef> sorter1Setter, Action<TransferableSorterDef> sorter2Setter)
    {
        GUI.BeginGroup(new Rect(0f, 0f, SortButtonAreaWidth, titleRowHeight));
        Text.Font = GameFont.Tiny;
        var rect = new Rect(0f, 0f, 60f, titleRowHeight);
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect, "SortBy".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        var rect2 = new Rect(rect.xMax + 10f, 0f, 130f, titleRowHeight);
        if (Widgets.ButtonText(rect2, sorterDef1.LabelCap))
        {
            OpenSorterChangeFloatMenu(sorter1Setter);
        }

        var rect3 = new Rect(rect2.xMax + 10f, 0f, 130f, titleRowHeight);
        if (Widgets.ButtonText(rect3, sorterDef2.LabelCap))
        {
            OpenSorterChangeFloatMenu(sorter2Setter);
        }

        GUI.EndGroup();
        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.BeginGroup(new Rect(rect3.xMax + 20f, 0f, inRect.width - SortButtonAreaWidth - 20f, titleRowHeight));
        var rect4 = new Rect(0f, 0f, 150f, titleRowHeight);
        Widgets.Label(rect4, "EKAI_ShowNonOwnerEquip".Translate());
        var rect5 = new Rect(rect4.xMax + 10f, 0f, 24f, titleRowHeight);
        var displayOtherPawnEquip = shouldDisplayOtherPawnEquip;
        Widgets.Checkbox(rect5.position, ref shouldDisplayOtherPawnEquip, 24f, false, true);
        if (displayOtherPawnEquip != shouldDisplayOtherPawnEquip)
        {
            CacheTransferables();
        }

        var rect6 = new Rect(rect5.xMax + 20f, 0f, 150f, titleRowHeight);
        Widgets.Label(rect6, "EKAI_ShowHasCaution".Translate());
        var rect7 = new Rect(rect6.xMax + 10f, 0f, 24f, titleRowHeight);
        displayOtherPawnEquip = shouldDisplayHasCaution;
        Widgets.Checkbox(rect7.position, ref shouldDisplayHasCaution, 24f, false, true);
        if (displayOtherPawnEquip != shouldDisplayHasCaution)
        {
            CacheTransferables();
        }

        var rect8 = new Rect(rect7.xMax + 10f, 0f, 200f, titleRowHeight);
        var allowedHitPointsPercents = compLocker.HealthRange;
        Widgets.FloatRange(rect8, 1, ref allowedHitPointsPercents, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
        compLocker.HealthRange = allowedHitPointsPercents;
        TooltipHandler.TipRegion(rect8, "EKAI_AllowedHPRange".Translate());
        GUI.EndGroup();
    }

    private void OpenSorterChangeFloatMenu(Action<TransferableSorterDef> sorterSetter)
    {
        var list = new List<FloatMenuOption>();
        var list2 = new List<TransferableSorterDef>(DefDatabase<TransferableSorterDef>.AllDefsListForReading);
        list2.Insert(1, bodyPartGroupAndLayerSorterDef);
        list2.Insert(2, thingDefNameSorterDef);
        foreach (var def in list2)
        {
            if (def.defName is "Category" or "Mass")
            {
                continue;
            }

            var def1 = def;
            list.Add(new FloatMenuOption(def.LabelCap, delegate { sorterSetter(def1); }));
        }

        Find.WindowStack.Add(new FloatMenu(list));
    }

    private void FillMainRect(Rect mainRect)
    {
        Text.Font = GameFont.Small;
        if (AnyTransferable)
        {
            var num = 6f;
            foreach (var section in sections)
            {
                num += section.GetContentsHeight();
            }

            var curY = 6f;
            var viewRect = new Rect(0f, 0f, mainRect.width - 16f, num);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            var num2 = scrollPosition.y - 30f;
            var num3 = scrollPosition.y + mainRect.height;
            for (var i = 0; i < sections.Count; i++)
            {
                var cachedTransferables = sections[i].cachedTransferables;
                if (!cachedTransferables.Any())
                {
                    continue;
                }

                if (sections[i].title != null)
                {
                    Widgets.ListSeparator(ref curY, viewRect.width, sections[i].title);
                    curY += 5f;
                }

                var j = 0;
                var num4 = 0;
                for (; j < cachedTransferables.Count; j++)
                {
                    if (!cachedTransferables[j].shouldDisplay)
                    {
                        continue;
                    }

                    if (curY > num2 && curY < num3)
                    {
                        var rect = new Rect(0f, curY, viewRect.width, 30f);
                        _ = cachedTransferables[j].Registerd;
                        DoRow(rect, cachedTransferables[j], num4);
                        num4++;
                    }

                    curY += 30f;
                }
            }

            Widgets.EndScrollView();
        }
        else
        {
            GUI.color = Color.gray;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(mainRect, "NoneBrackets".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }

    private void DoRow(Rect rect, LockerApparel trad, int index)
    {
        if (index % 2 == 1)
        {
            Widgets.DrawLightHighlight(rect);
        }

        Text.Font = GameFont.Small;
        GUI.BeginGroup(rect);
        var width = rect.width;
        width -= 10f;
        var rect2 = new Rect(width - 30f, 0f, 30f, rect.height);
        DrawCaution(rect2, trad);
        width -= 30f;
        var rect3 = new Rect(width - 360f, 0f, 360f, rect.height);
        DoRegisterInterfaceInternal(rect3, trad);
        width -= 360f;
        var dropArrowRect = new Rect(width - 60f, 0f, 60f, rect.height);
        DrawDropArrow(dropArrowRect, trad);
        width -= 60f;
        var stateRect = new Rect(width - 30f, 0f, 30f, rect.height);
        DrawState(stateRect, trad);
        width -= 30f;
        var rect4 = new Rect(width - 90f, 0f, 90f, rect.height);
        Text.Anchor = TextAnchor.MiddleLeft;
        DrawMarketValue(rect4, trad);
        width -= 90f;
        var idRect = new Rect(0f, 0f, width, rect.height);
        DrawTransferableInfo(trad, idRect, Color.white);
        GenUI.ResetLabelAlign();
        GUI.EndGroup();
    }

    private void DrawCaution(Rect rect, LockerApparel lApparel)
    {
        Widgets.DrawHighlightIfMouseover(rect);
        if (lApparel.CautionMessage == null)
        {
            return;
        }

        Widgets.DrawTextureFitted(rect, TexCaution, 0.5f);
        TooltipHandler.TipRegion(rect, lApparel.CautionMessage);
    }

    private void DoRegisterInterfaceInternal(Rect rect, LockerApparel trad)
    {
        rect = rect.Rounded();
        var rect2 = new Rect(rect.x, rect.center.y - 12.5f, 25f, 25f).Rounded();
        var rect3 = new Rect(rect).Rounded();
        var checkOn = trad.Registerd;
        if (trad.Unknown)
        {
            if (checkOn)
            {
                Widgets.Checkbox(rect2.position, ref checkOn, 24f, false, true, TexChecked, TexUnChecked);
                trad.Registerd = checkOn;
                rect3 = new Rect(rect2.xMax + 10f, 0f, rect.width - 90f, rect.height);
            }

            GUI.color = Color.red;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect3, "EKAI_Msg_Unknown".Translate());
        }
        else if (!CanWearTogetherPlanToRegister(trad))
        {
            GUI.color = cantSelectColor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, "EKAI_Msg_Duplication".Translate());
        }
        else
        {
            Widgets.Checkbox(rect2.position, ref checkOn, 24f, false, true, TexChecked, TexUnChecked);
            trad.Registerd = checkOn;
        }

        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
    }

    private void DrawDropArrow(Rect dropArrowRect, LockerApparel trad)
    {
        if (trad.Owner != compLocker)
        {
            return;
        }

        GUI.BeginGroup(dropArrowRect);
        var rect = new Rect(0f, 0f, 24f, 24f);
        if (unableDropArrow)
        {
            var colorInactive = ColorInactive;
            var mouseoverColorInactive = MouseoverColorInactive;
            TooltipHandler.TipRegion(rect, "EKAI_Msg_CantDropFromContainer".Translate(compLocker.parent.def.label));
            Widgets.ButtonImage(rect, TexButton.Drop, colorInactive, mouseoverColorInactive);
        }
        else
        {
            var white = Color.white;
            var mouseoverColor = GenUI.MouseoverColor;
            TooltipHandler.TipRegion(rect, "EKAI_Msg_DropFromContainer".Translate(compLocker.parent.def.label));
            if (Widgets.ButtonImage(rect, TexButton.Drop, white, mouseoverColor))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                compLocker.DropApparel(trad.Contents);
                trad.Owner = null;
            }
        }

        GUI.EndGroup();
    }

    private void DrawState(Rect stateRect, LockerApparel trad)
    {
        Widgets.DrawHighlightIfMouseover(stateRect);
        GUI.BeginGroup(stateRect);
        var rect = new Rect(0f, 0f, titleRowHeight, titleRowHeight);
        if (trad.Unknown)
        {
            GUI.DrawTexture(rect, TexQuestion);
            TooltipHandler.TipRegion(rect, "EKAI_Msg_UnknownDetail".Translate());
        }
        else if (trad.WearingPawn != null)
        {
            Widgets.ThingIcon(rect, trad.WearingPawn);
            TooltipHandler.TipRegion(rect, "EKAI_Msg_WearPawn".Translate(trad.WearingPawn.LabelNoCount));
        }
        else if (trad.Owner == compLocker)
        {
            Widgets.ThingIcon(rect, compLocker.parent);
            TooltipHandler.TipRegion(rect, "EKAI_Msg_Store".Translate(compLocker.parent.def.label));
        }

        GUI.EndGroup();
    }

    private void DrawMarketValue(Rect rect, LockerApparel trad)
    {
        Widgets.DrawHighlightIfMouseover(rect);
        Widgets.Label(rect, trad.Contents.MarketValue.ToStringMoney());
        TooltipHandler.TipRegionByKey(rect, "MarketValueTip");
    }

    private bool CanWearTogetherPlanToRegister(LockerApparel trad)
    {
        foreach (var section in sections)
        {
            foreach (var transferable in section.transferables)
            {
                if (trad == transferable || !transferable.Registerd ||
                    ApparelUtility.CanWearTogether(trad.ThingDef, transferable.ThingDef, BodyDefOf.Human))
                {
                    continue;
                }

                return false;
            }
        }

        return true;
    }

    private void DrawTransferableInfo(LockerApparel trad, Rect idRect, Color labelColor)
    {
        if (Mouse.IsOver(idRect))
        {
            Widgets.DrawHighlight(idRect);
        }

        var rect = new Rect(0f, 0f, titleRowHeight, titleRowHeight);
        Widgets.ThingIcon(rect, trad.Contents);
        Widgets.InfoCardButton(40f, 0f, trad.Contents);
        Text.Anchor = TextAnchor.MiddleLeft;
        var rect2 = new Rect(80f, 0f, idRect.width - 80f, idRect.height);
        Text.WordWrap = false;
        GUI.color = labelColor;
        Widgets.Label(rect2, trad.LabelCap);
        GUI.color = Color.white;
        Text.WordWrap = true;
        if (!Mouse.IsOver(idRect))
        {
            return;
        }

        TooltipHandler.TipRegion(idRect, new TipSignal(delegate
        {
            var text = trad.LabelCap;
            var tipDescription = trad.TipDescription;
            if (!tipDescription.NullOrEmpty())
            {
                text = $"{text}: {tipDescription}";
            }

            return text;
        }, trad.GetHashCode()));
    }

    private struct Section
    {
        public string title;

        public IEnumerable<LockerApparel> transferables;

        public List<LockerApparel> cachedTransferables;

        public float GetContentsHeight()
        {
            var num = 0f;
            foreach (var cachedTransferable in cachedTransferables)
            {
                if (cachedTransferable.shouldDisplay)
                {
                    num += 30f;
                }
            }

            if (title != null)
            {
                num += 30f;
            }

            return num;
        }
    }
}