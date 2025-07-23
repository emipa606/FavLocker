using RimWorld;
using Verse;

namespace Locker;

public class LockerApparel(Apparel apparel)
{
    public Apparel Contents { get; } = apparel;

    public ThingDef ThingDef => Contents?.def;

    public string TipDescription => Contents == null ? "" : Contents.DescriptionDetailed;

    private string Label => Contents.LabelNoCount;

    public string LabelCap => Label.CapitalizeFirst(ThingDef);

    public bool Registerd { get; set; }


    public CompLocker Owner { get; set; }

    public Pawn WearingPawn { get; set; }

    public bool OtherPawnWearing { get; set; }


    public bool Unknown { get; set; }


    public bool ConflictWithApparelsRegisterdLinkedContainer { get; set; }


    public string CautionMessage { get; set; }


    public bool shouldDisplay { get; set; } = true;
}