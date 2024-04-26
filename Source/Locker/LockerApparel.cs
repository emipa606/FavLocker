using RimWorld;
using Verse;

namespace Locker;

public class LockerApparel(Apparel apparel)
{
    public readonly Apparel apparel = apparel;


    public Apparel Contents => apparel;

    public ThingDef ThingDef => apparel?.def;

    public string TipDescription => apparel == null ? "" : apparel.DescriptionDetailed;

    public string Label => apparel.LabelNoCount;

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