using RimWorld;
using Verse;

namespace Locker
{
    public class LockerApparel
    {
        public Apparel apparel;


        public LockerApparel(Apparel apparel)
        {
            this.apparel = apparel;
        }

        public Apparel Contents => apparel;

        public ThingDef ThingDef
        {
            get
            {
                if (apparel == null)
                {
                    return null;
                }

                return apparel.def;
            }
        }

        public string TipDescription
        {
            get
            {
                if (apparel == null)
                {
                    return "";
                }

                return apparel.DescriptionDetailed;
            }
        }

        public string Label => apparel.LabelNoCount;

        public string LabelCap => Label.CapitalizeFirst(ThingDef);

        public bool Registerd { get; set; } = false;


        public CompLocker Owner { get; set; }

        public Pawn WearingPawn { get; set; }

        public bool OtherPawnWearing { get; set; } = false;


        public bool Unknown { get; set; } = false;


        public bool ConflictWithApparelsRegisterdLinkedContainer { get; set; } = false;


        public string CautionMessage { get; set; } = null;


        public bool shouldDisplay { get; set; } = true;
    }
}