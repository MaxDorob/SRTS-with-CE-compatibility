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
    public class TransporterArrivalOption_PrepareBombRun : TransportPodsArrivalAction
    {
        private MapParent mapParent;

        public TransporterArrivalOption_PrepareBombRun() : base()
        {
        }
        public TransporterArrivalOption_PrepareBombRun(MapParent map)
        {
            this.mapParent = map;
        }

        public override bool GeneratesMap => true;

        public override void Arrived(List<ActiveDropPodInfo> transporters, RimWorld.Planet.PlanetTile tile)
        {
            Find.World.GetComponent<WorldComponent_BomberController>().StartSelectingFor(tile, transporters);
        }
        public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, MapParent site)
        {
            if (site == null /*|| !site.Spawned*/)
            {
                return false;
            }
            if (!TransportersArrivalActionUtility.AnyNonDownedColonist(pods))
            {
                return false;
            }
            if (site.EnterCooldownBlocksEntering())
            {
                return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(site.EnterCooldownTicksLeft().ToStringTicksToPeriod(true, false, true, true, false)));
            }
            if (!pods.SelectMany(p => ThingOwnerUtility.GetAllThingsRecursively(p)).Any(x => SRTSMod.mod.settings.allowedBombs.Contains(x.def.defName)))
            {
                return FloatMenuAcceptanceReport.WithFailMessage("SRTSNoBombsLoaded".Translate());
            }
            return true;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, MapParent site)
        {
            var bombsLoaded = pods.SelectMany(p => ThingOwnerUtility.GetAllThingsRecursively(p)).Any(x => SRTSMod.mod.settings.allowedBombs.Contains(x.def.defName));
            var label = "SRTSSendBombRun".Translate();
            if (!bombsLoaded)
            {
                label += $" ({"SRTSNoBombsLoaded".Translate()})";
            }
            foreach (var item in TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, site), () => new TransporterArrivalOption_PrepareBombRun(site), label, launchAction, site.Tile))
            {
                item.Disabled = !bombsLoaded;
                yield return item;
            }
        }
    }
}
