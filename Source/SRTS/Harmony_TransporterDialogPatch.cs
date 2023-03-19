using HarmonyLib;
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
    [HarmonyPatch]
    public static class Harmony_TransporterDialogPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Dialog_LoadTransporters), nameof(Dialog_LoadTransporters.PostOpen))]
        public static void PostOpenPostfix(Dialog_LoadTransporters __instance, List<CompTransporter> ___transporters)
        {
            if (!__instance.LoadingInProgressOrReadyToLaunch&& ___transporters.Any(x => x.parent.TryGetComp<CompLaunchableSRTS>() != null))
            {
                Traverse.Create(__instance).Method("SetLoadedItemsToLoad").GetValue();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TransporterUtility), nameof(TransporterUtility.AllSendableItems), new Type[] { typeof(List<CompTransporter>), typeof(Map), typeof(bool) })]
        public static void AllSendableItemsPostfix(List<CompTransporter> transporters, Map map, bool autoLoot, ref IEnumerable<Thing> __result)
        {
            
            var comp = transporters[0].parent.TryGetComp<CompLaunchableSRTS>();
            if ( comp != null)
            {
                __result = __result.Union(comp.parent.TryGetComp<CompTransporter>().GetDirectlyHeldThings());
                
            }
        }
    }
}
