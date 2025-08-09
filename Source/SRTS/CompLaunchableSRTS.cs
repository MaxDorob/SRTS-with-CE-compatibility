using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace SRTS
{
    [StaticConstructorOnStartup]
    public class CompLaunchableSRTS : CompShuttle
    {
        public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);
        private static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);
        private CompTransporter cachedCompTransporter;
        private Caravan carr;
        public float BaseFuelPerTile => SRTSMod.GetStatFor<float>(this.parent.def.defName, StatName.fuelPerTile);

        public CompProperties_LaunchableSRTS SRTSProps => (CompProperties_LaunchableSRTS)this.props;




        public bool LoadingInProgressOrReadyToLaunch => Transporter.LoadingInProgressOrReadyToLaunch;

        public bool TryFindHomePoint(out Map map, out IntVec3 cell, out Rot4 rotation)
        {
            if (this.homeMap != null && this.homePoint.IsValid)
            {
                cell = this.homePoint;
                rotation = homeRotation;
                map = this.homeMap;
                return true;
            }
            cell = IntVec3.Invalid;
            rotation = Rot4.Invalid;
            map = null;
            return false;
        }


        public void AddThingsToSRTS(Thing thing)
        {
            thingsInsideShip.Add(thing);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (parent.Map?.IsPlayerHome ?? false)
                {
                    homeMap = parent.Map;
                    homePoint = parent.Position;
                    homeRotation = parent.Rotation;
                }
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                Gizmo g = gizmo;
                yield return g;
                g = null;
            }
            //if (this.lk)
            //{
            //    Command_Action launch = new Command_Action();
            //    launch.defaultLabel = "CommandLaunchGroup".Translate();
            //    launch.defaultDesc = "CommandLaunchGroupDesc".Translate();
            //    launch.icon = LaunchCommandTex;
            //    launch.alsoClickIfOtherInGroupClicked = false;
            //    launch.action = (Action)(() =>
            //    {
            //        int num = 0;
            //        foreach (Thing t in this.Transporter.innerContainer)
            //        {
            //            if (t is Pawn && (t as Pawn).IsColonist)
            //            {
            //                num++;
            //            }
            //        }
            //        if (SRTSMod.mod.settings.passengerLimits)
            //        {
            //            if (num < SRTSMod.GetStatFor<int>(this.parent.def.defName, StatName.minPassengers))
            //            {
            //                Messages.Message("NotEnoughPilots".Translate(), MessageTypeDefOf.RejectInput, false);
            //                return;
            //            }
            //            else if (num > SRTSMod.GetStatFor<int>(this.parent.def.defName, StatName.maxPassengers))
            //            {
            //                Messages.Message("TooManyPilots".Translate(), MessageTypeDefOf.RejectInput, false);
            //                return;
            //            }
            //        }

            //        if (this.AnyInGroupHasAnythingLeftToLoad)
            //            Find.WindowStack.Add((Window)Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(this.FirstThingLeftToLoadInGroup.LabelCapNoCount), StartChoosingDestination));
            //        else
            //            this.StartChoosingDestination();
            //    });
            //    if (!this.AllInGroupConnectedToFuelingPort)
            //        launch.Disable("CommandLaunchGroupFailNotConnectedToFuelingPort".Translate());
            //    else if (!this.AllFuelingPortSourcesInGroupHaveAnyFuel)
            //        launch.Disable("CommandLaunchGroupFailNoFuel".Translate());
            //    else if (this.AnyInGroupIsUnderRoof && !this.parent.Position.GetThingList(this.parent.Map).Any(x => x.def.defName == "ShipShuttleBay"))
            //        launch.Disable("CommandLaunchGroupFailUnderRoof".Translate());
            //    yield return launch;
            //}
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.Dead && !pawn.InMentalState && pawn.Faction == Faction.OfPlayerSilentFail)
            {
                if (pawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, mode: TraverseMode.ByPawn))
                {
                    if (this.LoadingInProgressOrReadyToLaunch)
                    {
                        yield return new FloatMenuOption("BoardSRTS".Translate(this.parent.Label), delegate ()
                        {
                            Job job = new Job(JobDefOf.EnterTransporter, this.parent);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
                return (string)null;
            return "ReadyForLaunch".Translate();
        }









        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref homeMap, nameof(homeMap));
            Scribe_Values.Look(ref homePoint, nameof(homePoint), IntVec3.Invalid);
            Scribe_Values.Look(ref homeRotation, nameof(homeRotation));
        }

        List<Thing> thingsInsideShip = new List<Thing>();
        private Map homeMap;
        private IntVec3 homePoint;
        private Rot4 homeRotation;
    }
}
