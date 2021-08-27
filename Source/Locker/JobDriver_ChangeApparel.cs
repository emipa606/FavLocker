using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locker
{
    public abstract class JobDriver_ChangeApparel : JobDriver, IGettableDestination
    {
        public enum Progress
        {
            NONE,
            REMOVING,
            WEARING
        }

        public static readonly TargetIndex TARGET_APPAREL = TargetIndex.A;

        public static readonly TargetIndex TARGET_CONTAINER = TargetIndex.B;

        public static readonly TargetIndex TARGET_LINKED_CONTAINER = TargetIndex.C;

        public static readonly TargetIndex TARGET_QUEUE_WORN_APPARELS_AT_EXEC = TargetIndex.A;

        protected int duration;

        public Progress progress = Progress.NONE;

        protected Building_RegistableContainer TargetContainer =>
            (Building_RegistableContainer)job.GetTarget(TARGET_CONTAINER).Thing;

        protected CompLocker TargetCompLocker => TargetContainer.GetComp<CompLocker>();

        protected Building_RegistableContainer LinkedContainer =>
            (Building_RegistableContainer)job.GetTarget(TARGET_LINKED_CONTAINER).Thing;

        protected CompLocker LinkedCompLocker => LinkedContainer?.GetComp<CompLocker>();

        public Thing GetDestination()
        {
            return TargetContainer;
        }

        public override string GetReport()
        {
            var thing = job.GetTarget(TARGET_APPAREL).Thing;
            if (progress == Progress.REMOVING && thing != null)
            {
                return "EKAI_Report_Remove".Translate(thing.Label);
            }

            if (progress == Progress.WEARING && thing != null)
            {
                return "EKAI_Report_Wear".Translate(thing.Label);
            }

            return GetReportDefault();
        }

        protected virtual string GetReportDefault()
        {
            return "";
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            var targetQueue = job.GetTargetQueue(TARGET_QUEUE_WORN_APPARELS_AT_EXEC);
            if (targetQueue.Any())
            {
                return;
            }

            job.countQueue = new List<int>();
            foreach (var item in pawn.apparel.WornApparel)
            {
                targetQueue.Add(item);
                var valueOrDefault = (pawn.outfits?.forcedHandler?.IsForced(item)).GetValueOrDefault();
                job.countQueue.Add(valueOrDefault ? 1 : 0);
            }
        }

        public virtual bool NeedGoto(TargetIndex containerInd)
        {
            if (containerInd == TARGET_CONTAINER)
            {
                return true;
            }

            if (containerInd == TARGET_LINKED_CONTAINER)
            {
                return false;
            }

            throw new ArgumentException();
        }

        public virtual Apparel GetNextWearApparel(TargetIndex containerInd)
        {
            throw new NotImplementedException();
        }

        public virtual Apparel GetNextRemoveApparel(TargetIndex containerInd)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentDuration(int duration)
        {
            this.duration = duration;
        }

        public int GetCurrentDuration()
        {
            return duration;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref duration, "duration");
            Scribe_Values.Look(ref progress, "progress");
        }
    }
}