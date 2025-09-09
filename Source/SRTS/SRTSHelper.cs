using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using CombatExtended;

namespace SRTS
{
    public static class SRTSHelper
    {
        public static Thing GetSingleSRTS(IEnumerable<IThingHolder> args)
        {
            var source = args.SelectMany(x => ThingOwnerUtility.GetAllThingsRecursively(x)).Where(x => x.HasComp<CompLaunchableSRTS>()).ToList();
            if (!source.Any())
            {
                Log.Error($"There's no SRTS");
                return null;
            }
            if (source.Count == 1)
            {
                return source[0];
            }
            return source.OrderByDescending(x => x.TryGetComp<CompTransporter>()?.Props.massCapacity ?? 1f).FirstOrDefault();
        }
        public static void PopulateDictionary()
        {
            srtsDefProjects = new Dictionary<ThingDef, ResearchProjectDef>();
            List<ThingDef> defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x?.researchPrerequisites?.Count > 0 && x.researchPrerequisites?[0].tab.ToString() == "SRTSE").ToList();
            foreach (ThingDef def in defs)
            {
                srtsDefProjects.Add(def, def.researchPrerequisites[0]);
            }
        }
        
        public static void PopulateAllowedBombs()
        {
            if (CEModLoaded)
            {                
                CEHelper.PopulateAllowedBombsCE();
                return;
            }

            List<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.GetCompProperties<CompProperties_Explosive>() != null && x.projectileWhenLoaded != null);
            if (SRTSMod.mod.settings.allowedBombs is null)
                SRTSMod.mod.settings.allowedBombs = new List<string>();
            if (SRTSMod.mod.settings.disallowedBombs is null)
                SRTSMod.mod.settings.disallowedBombs = new List<string>();
            foreach (ThingDef td in things)
            {
                if (!SRTSMod.mod.settings.allowedBombs.Contains(td.defName) && !SRTSMod.mod.settings.disallowedBombs.Contains(td.defName))
                {
                    SRTSMod.mod.settings.allowedBombs.Add(td.defName);
                }
            }
        }

        public static Dictionary<ThingDef, ResearchProjectDef> srtsDefProjects = new Dictionary<ThingDef, ResearchProjectDef>();
        public static bool CEModLoaded = false;

    }
}
