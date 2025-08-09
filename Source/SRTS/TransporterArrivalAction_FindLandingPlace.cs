using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    public class TransporterArrivalAction_FindLandingPlace : TransportPodsArrivalAction
    {
        public override bool GeneratesMap => false;

        public override void Arrived(List<ActiveDropPodInfo> transporters, RimWorld.Planet.PlanetTile tile)
        {
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tile, null);
            var srts = transporters.SelectMany(x => x.innerContainer).Single(x => x.HasComp<CompLaunchableSRTS>());
            TravellingTransporters travellingTransporters = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(srts.TryGetComp<CompLaunchable>().Props.worldObjectDef ?? WorldObjectDefOf.TravellingTransporters);
            travellingTransporters.SetFaction(Faction.OfPlayer);

            travellingTransporters.destinationTile = tile;
            travellingTransporters.Tile = tile;
            travellingTransporters.arrivalAction = new TransportersArrivalAction_FormCaravan();


            var info = new ActiveTransporterInfo();
            info.innerContainer.TryAddRangeOrTransfer(transporters.Single().innerContainer, destroyLeftover: true);
            info.SetShuttle(srts);
            travellingTransporters.AddTransporter(info, false);

            Find.World.GetComponent<WorldComponent_SRTSLanding>().StartSelectingFor(map, travellingTransporters);
        }
    }
}
