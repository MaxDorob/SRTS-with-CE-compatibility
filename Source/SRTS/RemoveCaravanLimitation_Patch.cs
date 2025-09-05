using HarmonyLib;
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
    [HarmonyLib.HarmonyPatch]
    internal static class RemoveCaravanLimitation_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var method in typeof(Caravan).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.Name.Contains("GetGizmos")).SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).Where(m => m.Name == "MoveNext"))
            {
                yield return method;
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetMethod = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.OdysseyActive));
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(targetMethod))
                {
                    instruction.opcode = OpCodes.Ldc_I4_1;
                    instruction.operand = null;
                }

                yield return instruction;

            }
        }
    }
}
