using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace SRTS
{
    [HarmonyLib.HarmonyPatch]
    internal static class TransporterArrivalAction_SwapArrivalMode_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var subClass in typeof(TransportersArrivalAction).AllSubclassesNonAbstract())
            {
                if (subClass.GetField("arrivalMode", BindingFlags.Instance | BindingFlags.NonPublic) != null)
                {
                    yield return AccessTools.Method(subClass, nameof(TransportersArrivalAction.Arrived));
                }
            }
        }
        public static bool Prefix(List<ActiveTransporterInfo> __0, PlanetTile tile, ref PawnsArrivalModeDef ___arrivalMode)
        {
            if (__0.SelectMany(t => ThingOwnerUtility.GetAllThingsRecursively(t)).Any(x => x.HasComp<CompLaunchableSRTS>()))
            {
                Find.World.GetComponent<WorldComponent_SRTSLanding>().StartSelectingFor(tile, __0);
                return false;
            }
            return true;
        }
    }
}
