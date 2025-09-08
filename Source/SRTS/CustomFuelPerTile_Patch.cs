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
    [HarmonyLib.HarmonyPatch()]
    internal static class CustomFuelPerTile_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CompLaunchable), nameof(CompLaunchable.FuelNeededToLaunchAtDist));
            yield return AccessTools.Method(typeof(CompLaunchable), nameof(CompLaunchable.MaxLaunchDistanceAtFuelLevel), [typeof(float), typeof(PlanetLayer)]);
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetField = AccessTools.Field(typeof(CompProperties_Launchable), nameof(CompProperties_Launchable.fuelPerTile));
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(targetField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(CustomFuelPerTile_Patch), nameof(CustomFuelPerTile_Patch.ModifyFuelLevelIfNeeded));
                }
            }
        }
        public static float ModifyFuelLevelIfNeeded(float original, CompLaunchable compLaunchable)
        {
            if (compLaunchable.parent.HasComp<CompLaunchableSRTS>())
            {
                return SRTSMod.GetStatFor<float>(compLaunchable.parent.def.defName, StatName.fuelPerTile);
            }
            return original;
        }
    }
}
