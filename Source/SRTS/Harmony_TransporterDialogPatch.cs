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
    [HarmonyPatch(typeof(TransporterUtility), nameof(TransporterUtility.AllSendableItems))]
    public static class Harmony_TransporterDialogPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var ins in instructions)
            {
                yield return ins;
            }
        }
        public static void Prefix(List<CompTransporter> transporters, Map map, ref bool autoLoot)
        {
            Log.Message(nameof(Harmony_TransporterDialogPatch) + " called. auto loot = " + autoLoot);
            //if(transporters.Any())
        }
    }
}
