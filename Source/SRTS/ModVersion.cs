using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    public static class ModVersion
    {
        [HarmonyLib.HarmonyPatch(typeof(ScribeMetaHeaderUtility), nameof(ScribeMetaHeaderUtility.WriteMetaHeader))]
        internal static class WriteModVersion_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Leave_S)
                    {
                        yield return CodeInstruction.Call(typeof(WriteModVersion_Patch), nameof(WriteModVersion_Patch.WriteSRTSVersion));
                    }
                    yield return instruction;
                }
            }
            public static void WriteSRTSVersion()
            {
                try
                {
                    SRTSInSave = ActualVersion;
                    Scribe_Values.Look(ref SRTSInSave, nameof(SRTSInSave));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
        }
        [HarmonyLib.HarmonyPatch(typeof(ScribeMetaHeaderUtility), nameof(ScribeMetaHeaderUtility.LoadGameDataHeader))]
        internal static class LoadModVersion_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var patched = false;
                foreach (var instruction in instructions)
                {
                    if (!patched && instruction.opcode == OpCodes.Ldsflda)
                    {
                        yield return CodeInstruction.Call(typeof(LoadModVersion_Patch), nameof(LoadModVersion_Patch.LoadSRTSVersion));
                        patched = true;
                    }
                    yield return instruction;
                }
            }
            public static void LoadSRTSVersion()
            {
                try
                {
                    Scribe_Values.Look(ref SRTSInSave, nameof(SRTSInSave));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
        }
        public static string SRTSInSave = null;
        public static string ActualVersion => "1.6.2.1";

        
    }
}
