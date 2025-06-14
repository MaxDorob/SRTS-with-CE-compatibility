using CombatExtended;
using CombatExtended.WorldObjects;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SRTS
{
    internal static class CEHelper
    {
        static bool typesChecked=false;
        static Type AmmoDefType=null;
        static void InitTypes()
        {
            AmmoDefType = typeof(AmmoDef);
            typesChecked = true;
        }
        public static bool? IsMortarAmmo(this ThingDef thingDef)
        {
            var className = thingDef.GetType().FullName;
            if (!typesChecked)
                InitTypes();
            if (AmmoDefType == null)
                return null;
            if (!AmmoDefType.IsAssignableFrom(thingDef.GetType()))
                return null;
            return (bool)AmmoDefType.GetField("isMortarAmmo").GetValue(thingDef);
        }
        static ThingDef MyGetProjectile(ThingDef thingDef)
        {
            if (thingDef.projectile != null)
            {
                return thingDef;
            }
            if (thingDef is AmmoDef ammoDef)
            {
                ThingDef user;
                if ((user = ammoDef.Users.FirstOrFallback(null)) != null)
                {
                    CompProperties_AmmoUser props = user.GetCompProperties<CompProperties_AmmoUser>();
                    AmmoSetDef asd = props.ammoSet;
                    AmmoLink ammoLink;
                    if ((ammoLink = asd.ammoTypes.FirstOrFallback(x => x.ammo == thingDef, null)) != null)
                    {
                        return ammoLink.projectile;
                    }
                }
                else
                {
                    return ammoDef.detonateProjectile;
                }
            }
            return thingDef;
        }
        internal static void PopulateAllowedBombsCE()
        {
            Log.Message(nameof(PopulateAllowedBombsCE));
            List<ThingDef> CEthings = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsMortarAmmo()??false).ToList();//For some reason using AmmoDef will cause exception without CE
            if (SRTSMod.mod.settings.allowedBombs is null)
                SRTSMod.mod.settings.allowedBombs = new List<string>();
            if (SRTSMod.mod.settings.disallowedBombs is null)
                SRTSMod.mod.settings.disallowedBombs = new List<string>();
            foreach (ThingDef td in CEthings)
            {
                if (!SRTSMod.mod.settings.allowedBombs.Contains(td.defName) && !SRTSMod.mod.settings.disallowedBombs.Contains(td.defName))
                {
                    SRTSMod.mod.settings.allowedBombs.Add(td.defName);
                }
            }
        }

        internal static IEnumerable<ThingDef> ExplosivesDefs(string explosivesString) => DefDatabase<AmmoDef>.AllDefs.Where(x => x.isMortarAmmo && !SRTSMod.mod.settings.allowedBombs.Contains(x.defName)
                    && CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.defName, explosivesString, CompareOptions.IgnoreCase) >= 0).Cast<ThingDef>().ToList();

        internal static bool CanBombSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
        {
            return mapParent.GetComponent<HealthComp>() != null;
        }

        internal static void CEDropBomb(IntVec3 bombPos, Thing bomb,Thing shooter,float radius)
        {
            float accuracyMultiplier = radius / 2;
            var sourceLoc = new Vector2();
            sourceLoc.Set(bombPos.x, bombPos.z);
            float angleError = ((3 * Mathf.PI / 2) / 20) * accuracyMultiplier;//20% accuracy error
            float rotationError = Rand.Range(-25f, 25f)*accuracyMultiplier*4;
            CE_Utility.LaunchProjectileCE(MyGetProjectile(bomb.def), sourceLoc, new LocalTargetInfo(bombPos), shooter, (3 * Mathf.PI / 2) + Rand.Range(-angleError, angleError), 0 + rotationError > 0 ? 0 + rotationError : 360 - rotationError, 40, Rand.Range(3f, 4.5f));
        }
    }
}
