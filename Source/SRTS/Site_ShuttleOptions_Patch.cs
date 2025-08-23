using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    /// <summary>
    ///  Used to prevent wrong <see cref="TransportersArrivalAction_VisitSpace"/> usage because for some reason there's no OdysseyActive check and 'if (this.def.mapGenerator == MapGeneratorDefOf.Space)' returns true even in case if Odyssey is not active
    /// </summary>
    [HarmonyLib.HarmonyPatch()]
    internal static class Site_ShuttleOptions_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var method in typeof(Site).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(t => t.Name.Contains("GetShuttleFloatMenuOptions")).SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).Where(m => m.Name == "MoveNext"))
            {
                yield return method;
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (ModsConfig.OdysseyActive)
            {
                return instructions;
            }
            var list = instructions.ToList();
            var targetField = AccessTools.Field(typeof(MapGeneratorDefOf), nameof(MapGeneratorDefOf.Space));
            while (list.Any(x => x.LoadsField(targetField)))
            {
                var index = list.FirstIndexOf(x => x.LoadsField(targetField));
                list[index + 1].opcode = OpCodes.Br_S;
                list.RemoveAt(index);
                list.RemoveAt(index - 1);
                list.RemoveAt(index - 2);
                list.RemoveAt(index - 3);
            }
            return list;
        }
    }
}
