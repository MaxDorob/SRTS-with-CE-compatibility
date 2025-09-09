using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    [HarmonyLib.HarmonyPatch(typeof(TransportersArrivalAction_TransportShip), nameof(TransportersArrivalAction_TransportShip.Arrived))]
    internal static class TransporterArrivalAction_TransportShip_Patch
    {
        public static bool Prefix(List<ActiveTransporterInfo> __0, PlanetTile tile, IntVec3 ___cell)
        {
            if (!___cell.IsValid && __0.SelectMany(t => ThingOwnerUtility.GetAllThingsRecursively(t)).Any(x => x.HasComp<CompLaunchableSRTS>()))
            {
                Find.World.GetComponent<WorldComponent_SRTSLanding>().StartSelectingFor(tile, __0);
                return false;
            }
            return true;
        }
    }
}
