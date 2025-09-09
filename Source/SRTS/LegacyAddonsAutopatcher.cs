using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    [StaticConstructorOnStartup]
    internal class LegacyAddonsAutopatcher
    {
        public static bool autoPatchProcessed = false;
        static LegacyAddonsAutopatcher()
        {
            if (SRTSMod.mod.settings.autopatcher)
            {
                Log.Warning("Autopatching");
                autoPatchProcessed = true;
                foreach (var srts in DefDatabase<ThingDef>.AllDefs.Where(x => x.HasComp<CompLaunchableSRTS>()))
                {
                    StringBuilder performedActions = new StringBuilder();
                    var transporterComp = srts.GetCompProperties<CompProperties_Transporter>();

                    if (transporterComp == null)
                    {
                        performedActions.AppendLine($"PATCH FAILED! There's no {nameof(CompTransporter)}");
                        PostPatched(srts, performedActions, true);
                        continue;
                    }
                    if (!transporterComp.max1PerGroup)
                    {
                        performedActions.AppendLine($"{nameof(transporterComp.max1PerGroup)} is false! Changing to true...");
                        transporterComp.max1PerGroup = true;
                    }
                    if (!transporterComp.canChangeAssignedThingsAfterStarting)
                    {
                        performedActions.AppendLine($"{nameof(transporterComp.canChangeAssignedThingsAfterStarting)} is false! Changing to true...");
                        transporterComp.canChangeAssignedThingsAfterStarting = true;
                    }

                    if (!typeof(Building_PassengerShuttle).IsAssignableFrom(srts.thingClass))
                    {
                        performedActions.AppendLine($"{nameof(srts.thingClass)} ({srts.thingClass.Name}) is not {nameof(Building_PassengerShuttle)}, changing to {nameof(Building_PassengerShuttle)}...");
                        srts.thingClass = typeof(Building_PassengerShuttle);
                    }


                    var srtsComp = srts.GetCompProperties<CompProperties_LaunchableSRTS>();
                    var shipDef = srtsComp.shipDef;
                    if (shipDef == null)
                    {
                        performedActions.AppendLine("shipDef is null, trying to assign some...");
                        if (srts.HasComp<CompBombFlyer>())
                        {
                            srtsComp.shipDef = TryFindOrCreateDef<BomberShipDef>(srts.defName, performedActions);
                        }
                        else
                        {
                            srtsComp.shipDef = TryFindOrCreateDef<BomberShipDef>(srts.defName, performedActions);
                        }
                        srtsComp.shipDef.shipThing ??= srts;
                        shipDef = srtsComp.shipDef;
                    }
                    srtsComp.shipDef.worldObject ??= DefDatabase<WorldObjectDef>.GetNamed("TravelingSRTS");

                    if (!srtsComp.shipDef.playerShuttle)
                    {
                        performedActions.AppendLine("shipDef.playerShuttle is false, changing to true...");
                        srtsComp.shipDef.playerShuttle = true;
                    }


                    var launchableComp = srts.GetCompProperties<CompProperties_Launchable>();
                    if (launchableComp == null)
                    {
                        performedActions.AppendLine($"There's no {nameof(CompLaunchable)}, adding new one...");
                        launchableComp = new CompProperties_Launchable();
                        launchableComp.fuelPerTile = srtsComp.fuelPerTile ?? 2.25f;
                        srts.comps.Add(launchableComp);
                    }
                    launchableComp.worldObjectDef ??= DefDatabase<WorldObjectDef>.GetNamed("TravelingSRTS");



                    var leavingSkyfaller = srtsComp.shipDef.leavingSkyfaller ?? launchableComp.skyfallerLeaving ?? DefDatabase<ThingDef>.GetNamed($"{srts.defName}_Leaving");
                    if (leavingSkyfaller == null)
                    {
                        performedActions.AppendLine($"PATCH FAILED! Can't find leavingSkyfaller def");
                        PostPatched(srts, performedActions, true);
                        continue;
                    }



                    if (launchableComp.skyfallerLeaving == null)
                    {
                        performedActions.AppendLine($"{nameof(CompProperties_Launchable)}.{nameof(launchableComp.skyfallerLeaving)} was null. Assigning {leavingSkyfaller.defName}");
                        launchableComp.skyfallerLeaving = leavingSkyfaller;
                    }



                    if (srtsComp.shipDef.leavingSkyfaller == null)
                    {
                        performedActions.AppendLine($"{nameof(CompProperties_LaunchableSRTS)}.{nameof(srtsComp.shipDef)}.{nameof(srtsComp.shipDef.leavingSkyfaller)} was null. Assigning {leavingSkyfaller.defName}");
                        srtsComp.shipDef.leavingSkyfaller = leavingSkyfaller;
                    }



                    if (srtsComp.shipDef.arrivingSkyfaller == null)
                    {
                        var arrivingSkyfaller = DefDatabase<ThingDef>.GetNamed($"{srts.defName}_Incoming");
                        if (arrivingSkyfaller == null)
                        {
                            performedActions.AppendLine($"PATCH FAILED! Can't find arrivingSkyfaller def");
                            PostPatched(srts, performedActions, true);
                            continue;
                        }
                        performedActions.AppendLine($"{nameof(CompProperties_LaunchableSRTS)}.{nameof(srtsComp.shipDef)}.{nameof(srtsComp.shipDef.arrivingSkyfaller)} was null. Assigning {arrivingSkyfaller.defName}");
                        srtsComp.shipDef.arrivingSkyfaller = arrivingSkyfaller;
                    }

                    if (srtsComp.shipDef is BomberShipDef bomberShipDef)
                    {
                        if (bomberShipDef.bomberSkyfaller == null)
                        {
                            var bombingSkyfaller = DefDatabase<ThingDef>.GetNamed($"{srts.defName}_BomberRun");
                            if (bombingSkyfaller == null)
                            {
                                performedActions.AppendLine($"PATCH FAILED! Can't find bomberSkyfaller def");
                                PostPatched(srts, performedActions, true);
                                continue;
                            }
                            performedActions.AppendLine($"{nameof(CompProperties_LaunchableSRTS)}.{nameof(srtsComp.shipDef)}.{nameof(bomberShipDef.bomberSkyfaller)} was null. Assigning {bombingSkyfaller.defName}");
                            bomberShipDef.bomberSkyfaller = bombingSkyfaller;
                        }
                        if (!bomberShipDef.bomberSkyfaller.skyfaller.reversed)
                        {
                            performedActions.AppendLine($"{nameof(SkyfallerProperties)}.{nameof(SkyfallerProperties.reversed)} is false, changing to true...");
                            bomberShipDef.bomberSkyfaller.skyfaller.reversed = true;
                        }
                        bomberShipDef.bomberSkyfaller.skyfaller.ticksToImpactRange = new IntRange(0, 0);
                    }


                    if (shipDef.leavingSkyfaller?.skyfaller.rotationCurve == null)
                    {
                        performedActions.AppendLine($"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.rotationCurve} is null. Setting default...");
                        shipDef.leavingSkyfaller.skyfaller.rotationCurve = new SimpleCurve()
                        {
                            {new CurvePoint(0,0)},
                        };
                    }
                    if (shipDef.leavingSkyfaller?.skyfaller.speedCurve == null)
                    {
                        performedActions.AppendLine($"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.speedCurve} is null. Setting default...");
                        shipDef.leavingSkyfaller.skyfaller.speedCurve = new SimpleCurve()
                        {
                            {new CurvePoint(0,1)},
                        };
                    }
                    if (shipDef.leavingSkyfaller?.skyfaller.zPositionCurve == null)
                    {
                        performedActions.AppendLine($"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.zPositionCurve} is null. Setting default...");
                        shipDef.leavingSkyfaller.skyfaller.zPositionCurve = new SimpleCurve()
                        {
                            {new CurvePoint(0,0)},
                        };
                    }


                    if (shipDef.arrivingSkyfaller?.skyfaller.rotationCurve == null)
                    {
                        performedActions.AppendLine($"{nameof(shipDef.arrivingSkyfaller)}'s {shipDef.arrivingSkyfaller.skyfaller.rotationCurve} is null. Setting default...");
                        shipDef.arrivingSkyfaller.skyfaller.rotationCurve = new SimpleCurve()
                        {
                            {new CurvePoint(0,0)},
                        };
                    }
                    if (shipDef.arrivingSkyfaller?.skyfaller.speedCurve == null)
                    {
                        performedActions.AppendLine($"{nameof(shipDef.arrivingSkyfaller)}'s {shipDef.arrivingSkyfaller.skyfaller.speedCurve} is null. Setting default...");
                        shipDef.arrivingSkyfaller.skyfaller.speedCurve = new SimpleCurve()
                        {
                            {new CurvePoint(0,1)},
                        };
                    }
                    if (shipDef.arrivingSkyfaller?.skyfaller.zPositionCurve == null)
                    {
                        performedActions.AppendLine($"{nameof(shipDef.arrivingSkyfaller)}'s {shipDef.arrivingSkyfaller.skyfaller.zPositionCurve} is null. Setting default...");
                        shipDef.arrivingSkyfaller.skyfaller.zPositionCurve = new SimpleCurve()
                        {
                            {new CurvePoint(0,0)},
                        };
                    }


                    PostPatched(srts, performedActions, false);
                }
            }
        }
        private static void PostPatched(ThingDef srts, StringBuilder performedActions, bool fail)
        {
            if (performedActions.Length > 0)
            {
                var text = $"Performed patches with {srts.defName}:\n{performedActions}";
                if (fail)
                {
                    Log.Error(text);
                }
                else
                {
                    Log.Warning(text);
                }
                var srtsComp = srts.GetCompProperties<CompProperties_LaunchableSRTS>();
            }
        }
        private static T TryFindOrCreateDef<T>(string defName, StringBuilder performedActions) where T : Def, new()
        {
            T def = DefDatabase<T>.GetNamed(defName, false);
            if (def == null)
            {
                performedActions.AppendLine($"Failed to find {typeof(T).Name} with defName \"{defName}\". Creating new one");
                def = new T();
                def.defName = defName;
                if (!typeof(T).BaseType.IsAbstract)
                {
                    typeof(DefDatabase<>).MakeGenericType(typeof(T).BaseType).GetMethod("Add", [typeof(T).BaseType]).Invoke(null, [def]);
                }
                DefDatabase<T>.Add(def);
            }
            return def;
        }
    }
}
