using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace SRTS
{
    /* Akreedz original patch */
    [HarmonyPatch(typeof (ActiveDropPod), "PodOpen", new Type[] {})]
    public static class HarmonyTest_dp
    {
        public static void Prefix(ActiveDropPod __instance)
        {
            ThingOwner ship =null;
            ActiveDropPodInfo activeDropPodInfo = Traverse.Create((object) __instance).Field("contents").GetValue<ActiveDropPodInfo>();
            for (int index = activeDropPodInfo.innerContainer.Count - 1; index >= 0; --index)
            {
                Thing thing = activeDropPodInfo.innerContainer[index];
                if(thing?.TryGetComp<CompLaunchableSRTS>() != null)
                {
                     ship= GenSpawn.Spawn(thing, __instance.Position, __instance.Map, thing.Rotation).TryGetComp<CompTransporter>().GetDirectlyHeldThings();
                    break;
                }
            }
            Log.Message("Ship info: " + (ship == null) + ", " + ship.ToString());
            if(ship!=null&&(!__instance.Map.IsPlayerHome))
                for (int i = activeDropPodInfo.innerContainer.Count - 1; i >= 0; i--)
                {
                    Thing thingToTransfer = activeDropPodInfo.innerContainer[i];
                    if(thingToTransfer is not Pawn)
                    {
                        activeDropPodInfo.innerContainer.TryTransferToContainer(thingToTransfer, ship);
                    }
                }
        }
    }
}
