using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CombatExtended;

namespace SRTS
{
    public class SRTSCEArrivalActionShelling : TransportPodsArrivalAction
    {
        public Map Map
        {
            get;
            private set;
        }
#if RELEASE1_6
        public override bool GeneratesMap => true;
#endif

        public SRTSCEArrivalActionShelling(Map sourceMap, IntVec3 originalLandingSpot, BombingType bombType)
        {
            Map = sourceMap;
            bombingType = bombType;
            sourceLandingSpot = originalLandingSpot;
        }
        private BombingType bombingType;
        private IntVec3 sourceLandingSpot;
        public Thing Ship(List<ActiveDropPodInfo> pods) => pods.SelectMany(x => x.innerContainer).Single(x => x.TryGetComp<CompLaunchableSRTS>() != null);
        public Thing Ship(ActiveDropPodInfo pod) => pod.innerContainer.Single(x => x.TryGetComp<CompLaunchableSRTS>() != null);
        public IEnumerable<Thing> Shells(ActiveDropPodInfo pod) => pod.innerContainer.Where(y => SRTSMod.mod.settings.allowedBombs.Contains(y.def.defName));
        public IEnumerable<Thing> Shells(IEnumerable<ActiveDropPodInfo> pods) => Shells(pods.Single());
        public override void Arrived(List<ActiveDropPodInfo> pods,
#if RELEASE1_6
            PlanetTile tile
#else
            int tile
#endif
            )
        {
            Arrived(pods.Single(), tile);
            GoBack(pods.Single(), tile);
        }
        private void Arrived(ActiveDropPodInfo pod, int tile)
        {
            var ship = Ship(pod);
            var dropBombsAmount = bombingType == BombingType.carpet ? ship.TryGetComp<CompBombFlyer>().Props.numberBombs : ship.TryGetComp<CompBombFlyer>().Props.precisionBombingNumBombs;
            Thing containedShell;
            while (dropBombsAmount >= 0 && (containedShell = pod.innerContainer.FirstOrDefault(y => SRTSMod.mod.settings.allowedBombs.Contains(y.def.defName))) != null)
            {
                containedShell = pod.innerContainer.Take(containedShell, 1);
                var shellDef = containedShell.def;
                TravelingShell shell = (TravelingShell)WorldObjectMaker.MakeWorldObject(CE_WorldObjectDefOf.TravelingShell);
                shell.SetFaction(Faction.OfPlayer);
                shell.Tile = tile;
                shell.SpawnSetup();
                Find.World.worldObjects.Add(shell);
                shell.launcher = ship;
                shell.equipmentDef = ship.def;
                shell.globalSource = new GlobalTargetInfo(sourceLandingSpot, Map);
                shell.globalSource.tileInt = Map.Tile;
                shell.globalSource.mapInt = Map;
                shell.globalSource.worldObjectInt = Map.Parent;
                shell.shellDef = shellDef;
                shell.globalTarget = new GlobalTargetInfo(tile);
                shell.startingTile = Map.Tile;
                shell.Arrived();
                //if (!shell.TryTravel(tile, tile))
                //{
                //    Log.Error($"CE: Travling shell {shellDef} failed to launch!");
                //    shell.Destroy();
                //    return;
                //}
                containedShell.Destroy();
                dropBombsAmount--;
                if (dropBombsAmount <= 0)
                {
                    Messages.Message("BombRunStarted".Translate(), MessageTypeDefOf.CautionInput, true);
                    return;
                }

            }
            Messages.Message("BombRunStarted".Translate(), MessageTypeDefOf.CautionInput, true);
        }
        public void GoBack(ActiveDropPodInfo pod, int tile)
        {
            var ship = Ship(pod);
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDef.Named(ship.def.defName.Split('_')[0] + "_Active"), null);
            activeDropPod.Contents = new ActiveDropPodInfo();
            activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(pod.innerContainer, true, true);

            TravelingSRTS travelingTransportPods = (TravelingSRTS)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("TravelingSRTS", true));
            travelingTransportPods.Tile = tile;
            travelingTransportPods.SetFaction(Faction.OfPlayer);
            travelingTransportPods.destinationTile = Map.Tile;
            travelingTransportPods.arrivalAction = new TransportPodsArrivalAction_LandInSpecificCell(Map.Parent, this.sourceLandingSpot);
            travelingTransportPods.flyingThing = ship;
            Find.WorldObjects.Add((WorldObject)travelingTransportPods);
            travelingTransportPods.AddPod(activeDropPod.Contents, true);
        }
    }
}
