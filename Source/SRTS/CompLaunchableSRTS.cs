using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using HarmonyLib;
using CombatExtended.Compatibility;
using System.Reflection.Emit;

namespace SRTS
{
    [StaticConstructorOnStartup]
    public class CompLaunchableSRTS : CompShuttle
    {
        static CompLaunchableSRTS()
        {
            if (!ModsConfig.OdysseyActive)
            {
                Building_PassengerShuttle.RefuelFromCargoIcon = new CachedTexture(ThingDefOf.Chemfuel.graphicData.texPath);
            }
        }
        [HarmonyPatch(typeof(CompShuttle), nameof(CompShuttle.CanLaunch), methodType: MethodType.Getter)]
        internal static class CompShuttle_CanLaunch_Patch
        {
            public static void Postfix(CompShuttle __instance, ref AcceptanceReport __result)
            {
                if (__result && __instance is CompLaunchableSRTS srts)
                {
                    __result = srts.CanLaunchExtra;
                }
            }
        }
        [HarmonyPatch(typeof(CompShuttle), nameof(IsPlayerShuttle), MethodType.Getter)]
        internal static class IsPlayerShuttle_Patch
        {
            public static bool Prefix(CompShuttle __instance, ref bool __result)
            {
                if (__instance is CompLaunchableSRTS)
                {
                    __result = true; //Only player buildable, isn't?
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(CompShuttle), nameof(PostSpawnSetup))]
        internal static class RemoveShuttleDLCLimitation
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();
                var targetMethod = AccessTools.Method(typeof(ModLister), nameof(ModLister.CheckAnyExpansion));
                while (list.Any(x => x.Calls(targetMethod)))
                {
                    var index = list.FirstIndexOf(x => x.Calls(targetMethod));
                    list[index] = new CodeInstruction(OpCodes.Ldc_I4_1);
                    list.RemoveAt(index - 1);
                }
                return list;
            }
        }
        [HarmonyPatch(typeof(CompShuttle), nameof(CompShuttle.HasPilot), MethodType.Getter)]
        internal static class HasPilot_Patch
        {
            public static bool Prefix(CompShuttle __instance, ref bool __result)
            {
                if (__instance is CompLaunchableSRTS srts)
                {
                    __result = srts.Pilots.Any();
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.TryLaunch))]
        internal static class BeforeLaunch_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ILGenerator)
            {

                foreach (var ins in instructions)
                {
                    yield return ins;
                    if (ins.IsStloc() && ins.operand is LocalBuilder local && local.LocalType == typeof(CompTransporter))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, ins.operand);
                        yield return CodeInstruction.Call(typeof(BeforeLaunch_Patch), nameof(BeforeLaunch_Patch.BeforeTransporterLaunch));
                    }
                }
            }
            public static void BeforeTransporterLaunch(CompTransporter transporter)
            {
                var compLaunchableSRTS = transporter.parent.TryGetComp<CompLaunchableSRTS>();
                if (compLaunchableSRTS != null)
                {
                    compLaunchableSRTS.lastLaunchTile = transporter.parent.Tile;
                    var flickable = transporter.parent.TryGetComp<CompFlickable>();
                    if (flickable != null)
                    {
                        compLaunchableSRTS.switchIsOn = flickable.SwitchIsOn;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(CaravanShuttleUtility), nameof(CaravanShuttleUtility.LaunchShuttle))]
        internal static class CacheLastLaunchTile_Caravan_Patch
        {
            public static void Prefix(Caravan caravan)
            {
                var compLaunchableSRTS = caravan.Shuttle?.TryGetComp<CompLaunchableSRTS>();
                if (compLaunchableSRTS != null)
                {
                    compLaunchableSRTS.lastLaunchTile = caravan.Tile;
                }
            }
        }
        [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.ShouldAutoRefuelNowIgnoringFuelPct), MethodType.Getter)]
        internal static class RefuelEvenFlickedOff_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();
                var targetField = AccessTools.Field(typeof(CompRefuelable), "flickComp");
                var index = list.FirstIndexOf(x => x.LoadsField(targetField));
                var label = (Label)list[index + 1].operand;

                List<CodeInstruction> toInsert = [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.Call(typeof(RefuelEvenFlickedOff_Patch), nameof(Ignore_compFlick)),
                    new CodeInstruction(OpCodes.Brtrue_S, label),
                    ];
                list.InsertRange(index - 1, toInsert);
                return list;
            }
            public static bool Ignore_compFlick(CompRefuelable instance)
            {
                return instance.parent.HasComp<CompLaunchableSRTS>();
            }
        }
        public CompProperties_LaunchableSRTS SRTSProps => (CompProperties_LaunchableSRTS)this.props;



        public bool TryFindHomePoint(out Map map, out IntVec3 cell, out Rot4 rotation)
        {
            if (this.homeMap != null && this.homePoint.IsValid)
            {
                cell = this.homePoint;
                rotation = homeRotation;
                map = this.homeMap;
                return true;
            }
            cell = IntVec3.Invalid;
            rotation = Rot4.Invalid;
            map = null;
            return false;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (parent.Map?.IsPlayerHome ?? false)
                {
                    homeMap = parent.Map;
                    homePoint = parent.Position;
                    homeRotation = parent.Rotation;
                }
                var flickable = parent.TryGetComp<CompFlickable>();
                if (flickable != null)
                {
                    flickable.SwitchIsOn = switchIsOn;
                    flickable.wantSwitchOn = switchIsOn;
                }
            }
        }

        public IEnumerable<Pawn> Pawns => this.Transporter.innerContainer.OfType<Pawn>().Union(parent.GetCaravan()?.PawnsListForReading ?? Enumerable.Empty<Pawn>());

        private bool IsPilot(Pawn pawn)
        {
            if (!pawn.IsFreeColonist)
            {
                return false;
            }
            if (StatDefOf.PilotingAbility == null)
            {
                return true;
            }
            if (StatDefOf.PilotingAbility.Worker.IsDisabledFor(pawn) || pawn.GetStatValue(StatDefOf.PilotingAbility, true, -1) <= 0.1f)
            {
                return false;
            }

            return true;
        }
        public IEnumerable<Pawn> Pilots => Pawns.Where(IsPilot);


        public AcceptanceReport CanLaunchExtra
        {
            get
            {
                var requiredPilots = SRTSMod.GetStatFor<int>(parent.def.defName, StatName.minPassengers);
                if (Pilots.Count() < requiredPilots)
                {
                    return "SRTSRequiredPilots".Translate(parent.def.LabelCap, requiredPilots);
                }
                var maxPawnCount = SRTSMod.GetStatFor<int>(parent.def.defName, StatName.maxPassengers);
                if (Pawns.Count() > maxPawnCount)
                {
                    return "SRTSPawnsLimitExceeded".Translate(parent.def.LabelCap, maxPawnCount, Pawns.Count());
                }
                return true;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref homeMap, nameof(homeMap));
            Scribe_Values.Look(ref homePoint, nameof(homePoint), IntVec3.Invalid);
            Scribe_Values.Look(ref homeRotation, nameof(homeRotation));
            Scribe_Values.Look(ref lastLaunchTile, nameof(lastLaunchTile), PlanetTile.Invalid);
            Scribe_Values.Look(ref switchIsOn, nameof(switchIsOn), false);
        }

        List<Thing> thingsInsideShip = new List<Thing>();
        private Map homeMap;
        private IntVec3 homePoint;
        private Rot4 homeRotation;
        public PlanetTile lastLaunchTile;
        private bool switchIsOn = false;
    }
}
