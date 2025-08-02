using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace SRTS
{
    public class TransporterArrivalOption_BombRun : TransportPodsArrivalAction
    {
        private Map map;
        private IntVec3 start;
        private IntVec3 end;
        private List<IntVec3> bombCells;
        private List<Thing> bombs;
        private BombingType bombingType;

        public TransporterArrivalOption_BombRun(Map map, IntVec3 start, IntVec3 end, List<IntVec3> bombCells, List<Thing> bombs, BombingType bombingType)
        {
            this.map = map;
            this.start = start;
            this.end = end;
            this.bombCells = bombCells;
            this.bombs = bombs;
            this.bombingType = bombingType;
        }

        public override bool GeneratesMap => false;

        public override void Arrived(List<ActiveDropPodInfo> transporters, RimWorld.Planet.PlanetTile tile)
        {
            var srts = transporters.SelectMany(x => x.innerContainer).Single(x => x.HasComp<CompLaunchableSRTS>());
            var compLaunchable = srts.TryGetComp<CompLaunchable>();
            ActiveTransporter activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(compLaunchable.Props.activeTransporterDef ?? ThingDefOf.ActiveDropPod, null);
            activeTransporter.Contents = new ActiveTransporterInfo();
            Log.Message($"Stage 2\n{string.Join("\n", transporters.Single().innerContainer.Select(x => x.def.defName))}\nRecursive:\n{string.Join("\n", ThingOwnerUtility.GetAllThingsRecursively(transporters.Single()).Select(x => x.def.defName))}");
            activeTransporter.Contents.innerContainer.TryAddRangeOrTransfer(transporters.Single().innerContainer, true, true);
            activeTransporter.Contents.sentTransporterDef = srts.def;
            activeTransporter.Rotation = srts.Rotation;
            activeTransporter.Contents.SetShuttle(srts);
            Log.Message($"{(srts.TryGetComp<CompLaunchableSRTS>().Props.shipDef as BomberShipDef) == null}, {(srts.TryGetComp<CompLaunchableSRTS>().Props.shipDef as BomberShipDef)?.bomberSkyfaller == null}");
            BomberSkyfaller bomberLeaving = (BomberSkyfaller)SkyfallerMaker.MakeSkyfaller((srts.TryGetComp<CompLaunchableSRTS>().Props.shipDef as BomberShipDef).bomberSkyfaller, activeTransporter);
            bomberLeaving.destinationTile = tile;
            bomberLeaving.arrivalAction = new TransportPodsArrivalAction_FormCaravan();
            bomberLeaving.worldObjectDef = (compLaunchable.Props.worldObjectDef ?? WorldObjectDefOf.TravellingTransporters);
            bomberLeaving.groupID = 9999;
            bomberLeaving.bombCells = bombCells;
            bomberLeaving.bombs = bombs;
            bomberLeaving.bombType = bombingType;
            bomberLeaving.enterPos = Utils.GetEdgeCell(map, end.ToVector3Shifted() - start.ToVector3Shifted(), start);
            GenSpawn.Spawn(bomberLeaving, Utils.GetEdgeCell(map, start.ToVector3Shifted() - end.ToVector3Shifted(), end), this.map, WipeMode.Vanish);
        }
    }
}
