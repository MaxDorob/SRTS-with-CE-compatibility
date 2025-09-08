using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    [HarmonyLib.HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.ChoseWorldTarget), [typeof(GlobalTargetInfo), typeof(PlanetTile), typeof(IEnumerable<IThingHolder>), typeof(int), typeof(Action<PlanetTile, TransportersArrivalAction>), typeof(CompLaunchable), typeof(float?)])]
    internal static class SpaceLaunch_Patch
    {
        public static bool Prefix(CompLaunchable launchable, GlobalTargetInfo target, ref bool __result)
        {
            if (ModsConfig.OdysseyActive)
            {
                var srtsComp = launchable.parent.TryGetComp<CompLaunchableSRTS>();
                if (srtsComp != null && target.HasWorldObject && target.Tile.LayerDef == PlanetLayerDefOf.Orbit && !SRTSMod.GetStatFor<bool>(srtsComp.parent.def.defName, StatName.spaceFaring))
                {
                    Messages.Message("SRTSNotAbleToLandInSpace".Translate(), MessageTypeDefOf.RejectInput, false);
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
