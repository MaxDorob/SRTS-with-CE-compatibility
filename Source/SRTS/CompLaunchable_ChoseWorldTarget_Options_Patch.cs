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
using UnityEngine.Tilemaps;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace SRTS
{
    [HarmonyLib.HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.ChoseWorldTarget), argumentTypes: [typeof(GlobalTargetInfo), typeof(PlanetTile), typeof(IEnumerable<IThingHolder>), typeof(int), typeof(Action<PlanetTile, TransportersArrivalAction>), typeof(CompLaunchable), typeof(float?)])]
    internal static class CompLaunchable_ChoseWorldTarget_Options_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ILGenerator)
        {
            var targetMethod = AccessTools.Method(typeof(Enumerable), nameof(Enumerable.ToList), generics: [typeof(FloatMenuOption)]);
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.Calls(targetMethod))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //tile
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return CodeInstruction.Call(typeof(CompLaunchable_ChoseWorldTarget_Options_Patch), nameof(CompLaunchable_ChoseWorldTarget_Options_Patch.ModifyList));
                }
            }
        }

        private static void ModifyList(List<FloatMenuOption> list, CompLaunchable instance, GlobalTargetInfo target, Action<PlanetTile, TransportPodsArrivalAction> launchAction)
        {
            var compBomber = instance.parent.GetComp<CompBombFlyer>();
            if (compBomber != null)
            {
                list.AddRange(compBomber.FloatMenuOptionsAt(target.Tile, launchAction));
            }
        }
    }
}
