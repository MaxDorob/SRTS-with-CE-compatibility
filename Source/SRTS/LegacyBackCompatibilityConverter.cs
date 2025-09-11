using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace SRTS
{
    [StaticConstructorOnStartup]
    public class LegacyBackCompatibilityConverter : BackCompatibilityConverter
    {
        [HarmonyLib.HarmonyPatch(typeof(BackCompatibility), "CheckSaveIdenticalToCurrentEnvironment")]
        internal static class CheckSaveIdenticalToCurrentEnvironment_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if (__result && (SRTSMod.mod.settings.forceLegacyConverters || ModVersion.ActualVersion != ModVersion.SRTSInSave))
                {
                    Log.WarningOnce($"SRTS version mismatch or forceLegacyConverters is active:\nSave - {ModVersion.SRTSInSave}\nActual version - {ModVersion.ActualVersion}", 826317278);
                    __result = false;
                }
            }
        }
        static LegacyBackCompatibilityConverter()
        {
            BackCompatibility.conversionChain.Add(new LegacyBackCompatibilityConverter());
        }
        public override bool AppliesToVersion(int majorVer, int minorVer)
        {
            return SRTSMod.mod.settings.forceLegacyConverters || ModVersion.ActualVersion != ModVersion.SRTSInSave;
        }

        public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
        {
            return null;
        }

        public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
        {
            Log.WarningOnce($"SRTS version mismatch or forceLegacyConverters is active:\nSave - {ModVersion.SRTSInSave}\nActual version - {ModVersion.ActualVersion}", 826317278);
            if (providedClassName == "Building")
            {
                var def = DefDatabase<ThingDef>.GetNamed(node["def"].InnerText);

                if (def != null && def.thingClass.Name != providedClassName && def.HasComp<CompLaunchableSRTS>())
                {
                    return def.thingClass;
                }
            }
            return null;
        }

        public override void PostExposeData(object obj)
        {
            if (obj is Building building)
            {
                var srtsComp = building.TryGetComp<CompLaunchableSRTS>();
                if (srtsComp != null)
                {
                    if (srtsComp.shipParent == null)
                    {
                        srtsComp.PostSpawnSetup(false);
                        srtsComp.requiredItems ??= new List<ThingDefCount>();
                        srtsComp.requiredPawns ??= new List<Pawn>();
                        srtsComp.pawnsToIgnoreIfDownedOfNotOnTheMap ??= new List<Pawn>();
                        var transporterComp = building.TryGetComp<CompTransporter>();
                        if (transporterComp != null)
                        {
                            transporterComp.leftToLoad ??= new List<TransferableOneWay>();
                        }
                    }
                }
            }
        }
    }
}
