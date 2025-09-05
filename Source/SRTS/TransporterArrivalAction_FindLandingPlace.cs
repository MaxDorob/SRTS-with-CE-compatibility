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
            Find.World.GetComponent<WorldComponent_SRTSLanding>().StartSelectingFor(tile, transporters);
        }
    }
}
