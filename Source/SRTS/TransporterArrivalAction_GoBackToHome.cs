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
    public class TransporterArrivalAction_GoBackToHome : TransportersArrivalAction_LandInSpecificCell
    {
        public override bool GeneratesMap => false;
        public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
        {
            if (this.cell == default && mapParent == default)
            {
                var srts = pods.SelectMany(x => x.GetDirectlyHeldThings()).Single(x => x.HasComp<CompLaunchableSRTS>());
                if (srts.TryGetComp<CompLaunchableSRTS>().TryFindHomePoint(out var homeMap, out cell, out rotation))
                {
                    mapParent = homeMap.Parent;
                }
            }
            return base.StillValid(pods, destinationTile);
        }

        public override void Arrived(List<ActiveDropPodInfo> transporters, RimWorld.Planet.PlanetTile tile)
        {
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tile, null);
            var srts = transporters.SelectMany(x => x.innerContainer).Single(x => x.HasComp<CompLaunchableSRTS>());
            if (srts.TryGetComp<CompLaunchableSRTS>().TryFindHomePoint(out _, out cell, out rotation))
            {
                this.mapParent = map.Parent;
                landInShuttle = true;
            }
            base.Arrived(transporters, tile);

        }
    }
}
