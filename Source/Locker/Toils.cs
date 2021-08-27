using RimWorld;
using Verse;
using Verse.AI;

namespace Locker
{
    public static class Toils
    {
        public static Toil JumpIfNeedNotGoto(Toil jumpToil, TargetIndex gotoContainer)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var jobDriver_ChangeApparel = (JobDriver_ChangeApparel)toil.actor.jobs.curDriver;
                if (!jobDriver_ChangeApparel.NeedGoto(gotoContainer))
                {
                    toil.actor.jobs.curDriver.JumpToToil(jumpToil);
                }
            };
            return toil;
        }

        public static Toil DepositHauledThingInContainer(TargetIndex containerInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var carryTracker = toil.actor.carryTracker;
                var compLocker = toil.actor.jobs.curJob.GetTarget(containerInd).Thing.TryGetComp<CompLocker>();
                var apparel = (Apparel)carryTracker.CarriedThing;
                carryTracker.innerContainer.Remove(apparel);
                compLocker.AddApparel(apparel);
            };
            return toil;
        }

        public static Toil SetRemoveApparel(TargetIndex apparelInd, TargetIndex containerInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.CurJob;
                var jobDriver_ChangeApparel = (JobDriver_ChangeApparel)actor.jobs.curDriver;
                var nextRemoveApparel = jobDriver_ChangeApparel.GetNextRemoveApparel(containerInd);
                curJob.SetTarget(apparelInd,
                    nextRemoveApparel != null ? (LocalTargetInfo)nextRemoveApparel : LocalTargetInfo.Invalid);
            };
            return toil;
        }

        public static Toil RemoveAndDropApparel(TargetIndex apparelInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var apparel = (Apparel)(Thing)actor.CurJob.GetTarget(apparelInd);
                if (apparel.Destroyed)
                {
                    return;
                }

                actor.apparel.TryDrop(apparel);
                actor.Reserve(apparel, actor.CurJob);
            };
            return toil;
        }

        public static Toil SetWearApparel(TargetIndex apparelInd, TargetIndex containerInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.CurJob;
                var jobDriver_ChangeApparel = (JobDriver_ChangeApparel)actor.jobs.curDriver;
                var nextWearApparel = jobDriver_ChangeApparel.GetNextWearApparel(containerInd);
                curJob.SetTarget(apparelInd,
                    nextWearApparel != null ? (LocalTargetInfo)nextWearApparel : LocalTargetInfo.Invalid);
            };
            return toil;
        }

        public static Toil TakeApparelFromContainerAndWear(TargetIndex apparelInd, TargetIndex containerInd,
            bool? overwriteForced = null)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var apparel = (Apparel)(Thing)actor.CurJob.GetTarget(apparelInd);
                var compLocker = actor.CurJob.GetTarget(containerInd).Thing.TryGetComp<CompLocker>();
                var forced = overwriteForced ?? compLocker.IsForced(apparel);
                compLocker.RemoveApparel(apparel);
                actor.apparel.Wear(apparel);
                actor.outfits?.forcedHandler?.SetForced(apparel, forced);
            };
            return toil;
        }

        public static Toil WaitEquipDelay(TargetIndex apparelInd, TargetIndex targetInd, float facotrEquipDelay = 1f)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var jobDriver_ChangeApparel2 = (JobDriver_ChangeApparel)actor.jobs.curDriver;
                var thing = (Apparel)actor.CurJob.GetTarget(apparelInd).Thing;
                var statValue = thing.GetStatValue(StatDefOf.EquipDelay);
                var num = (int)(statValue * 60f * facotrEquipDelay);
                actor.pather.StopDead();
                jobDriver_ChangeApparel2.ticksLeftThisToil = num;
                jobDriver_ChangeApparel2.SetCurrentDuration(num);
            };
            toil.tickAction = delegate
            {
                var apparel = (Apparel)toil.actor.CurJob.GetTarget(apparelInd).Thing;
                if (apparel.Destroyed)
                {
                    toil.actor.jobs.curDriver.ReadyForNextToil();
                }

                toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget(targetInd));
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.WithProgressBar(apparelInd, delegate
            {
                var jobDriver_ChangeApparel = (JobDriver_ChangeApparel)toil.actor.jobs.curDriver;
                return 1f - (jobDriver_ChangeApparel.ticksLeftThisToil /
                             (float)jobDriver_ChangeApparel.GetCurrentDuration());
            });
            toil.handlingFacing = true;
            return toil;
        }

        public static Toil PutApparelInTheLocker(TargetIndex indOfDropApparelQueue, TargetIndex containerInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var targetQueue = actor.jobs.curJob.GetTargetQueue(indOfDropApparelQueue);
                var countQueue = actor.jobs.curJob.countQueue;
                var compLocker = actor.CurJob.GetTarget(containerInd).Thing.TryGetComp<CompLocker>();
                for (var i = 0; i < targetQueue.Count; i++)
                {
                    var apparel = (Apparel)targetQueue[i].Thing;
                    var forced = countQueue[i] != 0;
                    if (!apparel.Spawned)
                    {
                        continue;
                    }

                    apparel.DeSpawn();
                    compLocker.AddApparel(apparel, forced);
                }
            };
            return toil;
        }

        public static Toil TakeAllApparelFromLockerAndWear(TargetIndex containerInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var compLocker = actor.CurJob.GetTarget(containerInd).Thing.TryGetComp<CompLocker>();
                var apparelsRegisterdAndInner = compLocker.GetApparelsRegisterdAndInner();
                for (var num = apparelsRegisterdAndInner.Count - 1; num >= 0; num--)
                {
                    var apparel = apparelsRegisterdAndInner[num];
                    compLocker.RemoveApparel(apparel);
                    actor.apparel.Wear(apparel);
                    actor.outfits?.forcedHandler?.SetForced(apparel, true);
                }
            };
            return toil;
        }

        public static Toil RemoveAllApparelAndStoreContainer(TargetIndex containerInd)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var compLocker = actor.CurJob.GetTarget(containerInd).Thing.TryGetComp<CompLocker>();
                var list = Util.AndList(actor.apparel.WornApparel, compLocker.RegisteredApparelsReadOnly());
                for (var num = list.Count - 1; num >= 0; num--)
                {
                    var apparel = list[num];
                    actor.apparel.Remove(apparel);
                    compLocker.AddApparel(apparel);
                }
            };
            return toil;
        }

        public static Toil WaitForStationToChange(TargetIndex stationInd, int delayTick)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var jobDriver_ChangeApparel2 = (JobDriver_ChangeApparel)actor.jobs.curDriver;
                actor.pather.StopDead();
                jobDriver_ChangeApparel2.ticksLeftThisToil = delayTick;
                jobDriver_ChangeApparel2.SetCurrentDuration(delayTick);
            };
            toil.tickAction = delegate
            {
                var thing = toil.actor.CurJob.GetTarget(stationInd).Thing;
                toil.actor.Rotation = thing.Rotation;
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.WithProgressBar(TargetIndex.A, delegate
            {
                var jobDriver_ChangeApparel = (JobDriver_ChangeApparel)toil.actor.jobs.curDriver;
                return 1f - (jobDriver_ChangeApparel.ticksLeftThisToil /
                             (float)jobDriver_ChangeApparel.GetCurrentDuration());
            });
            toil.handlingFacing = true;
            return toil;
        }

        public static Toil SetProgress(JobDriver_ChangeApparel.Progress progress)
        {
            var toil = new Toil();
            toil.initAction = delegate { ((JobDriver_ChangeApparel)toil.actor.jobs.curDriver).progress = progress; };
            return toil;
        }
    }
}