using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SRTS
{
    [HarmonyLib.HarmonyPatch()]
    internal class CompLaunchable_CustomCooldown_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CompLaunchable), nameof(CompLaunchable.CanLaunch));
            yield return AccessTools.Method(typeof(CompLaunchable), nameof(CompLaunchable.CompTick));
            yield return AccessTools.Method(typeof(CompLaunchable), nameof(CompLaunchable.CompInspectStringExtra));
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetField = AccessTools.Field(typeof(CompProperties_Launchable), nameof(CompProperties_Launchable.cooldownTicks));
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(targetField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(CompLaunchable_CustomCooldown_Patch), nameof(CompLaunchable_CustomCooldown_Patch.ModifyCooldownTicksIfNeeded));
                }
            }
        }
        public static int ModifyCooldownTicksIfNeeded(int originalCooldownTicks, CompLaunchable instance)
        {
            if (instance.parent.HasComp<CompLaunchableSRTS>())
            {
                return Mathf.FloorToInt(SRTSMod.GetStatFor<float>(instance.parent.def.defName, StatName.cooldownHours) * GenDate.TicksPerHour);
            }
            return originalCooldownTicks;
        }
    }
}
