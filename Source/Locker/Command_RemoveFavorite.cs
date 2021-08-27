using UnityEngine;
using Verse;
using Verse.AI;

namespace Locker
{
    internal class Command_RemoveFavorite : Command
    {
        public Building_Locker favLocker;

        public Pawn pawn;
        public Building_PowerArmorStation powerArmorStation;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (powerArmorStation != null)
            {
                var job = JobMaker.MakeJob(JobDefOf.EKAI_RemoveFavoriteOnPowerArmorStation, LocalTargetInfo.Invalid,
                    powerArmorStation, favLocker);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
            else
            {
                var job2 = JobMaker.MakeJob(JobDefOf.EKAI_RemoveRegisteredApparelWithLocker, LocalTargetInfo.Invalid,
                    favLocker);
                pawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
            }

            Util.NotifyCommand(this);
        }
    }
}