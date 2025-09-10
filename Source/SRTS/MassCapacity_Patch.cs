using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    [HarmonyLib.HarmonyPatch(typeof(CompTransporter), nameof(CompTransporter.MassCapacity), HarmonyLib.MethodType.Getter)]
    internal static class MassCapacity_Patch
    {
        public static bool Prefix(CompTransporter __instance, ref float __result)
        {
            if (__instance.parent.HasComp<CompLaunchableSRTS>())
            {
                __result = SRTSMod.GetStatFor<float>(__instance.parent.def.defName, StatName.massCapacity);
                return false;
            }
            return true;
        }
    }
}
