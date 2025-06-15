using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace SRTS
{
    /* Akreedz original patch */
    [HarmonyPatch(typeof (TransportPodsArrivalAction_LandInSpecificCell), "Arrived")]
    public static class HarmonyTest_AJ
    {
        public static bool Prefix(TransportPodsArrivalAction_LandInSpecificCell __instance, List<ActiveDropPodInfo>
#if RELEASE1_6
            transporters
#else
            pods
#endif
            )
        {
#if RELEASE1_6
            var pods = transporters;
#endif
            foreach (ActiveDropPodInfo pod in pods)
            {
                for (int index = 0; index < pod.innerContainer.Count; index++)
                {
                    if(pod.innerContainer[index].TryGetComp<CompLaunchableSRTS>() != null || DefDatabase<ThingDef>.GetNamed(pod.innerContainer[index]?.def?.defName?.Split('_')[0], false)?.GetCompProperties<CompProperties_LaunchableSRTS>() != null)
                    {
                        Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
                        Traverse traverse = Traverse.Create((object)__instance);
                        IntVec3 c = traverse.Field("cell").GetValue<IntVec3>();
                        Map map = traverse.Field("mapParent").GetValue<MapParent>().Map;
                        TransportPodsArrivalActionUtility.RemovePawnsFromWorldPawns(pods);
                        for (int i = 0; i < pods.Count; ++i)
                        {
                            pods[i].openDelay = 0;
                            DropPodUtility.MakeDropPodAt(c, map, pods[i]);
                        }
                        Messages.Message("MessageTransportPodsArrived".Translate(), (LookTargets)lookTarget, MessageTypeDefOf.TaskCompletion, true);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
