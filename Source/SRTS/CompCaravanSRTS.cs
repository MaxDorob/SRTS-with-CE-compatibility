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
    public class CompCaravanSRTS : WorldObjectComp
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (ModsConfig.OdysseyActive)
            {
                yield break; // We don't use custom logic if Odyssey is active
            }
            var caravan = parent as Caravan;
            var shuttle = caravan.Shuttle;
            if (shuttle != null && shuttle.HasComp<CompLaunchableSRTS>())
            {
                var launch = new Command_Action();
                launch.defaultLabel = "CommandLaunchGroup".Translate();
                launch.defaultDesc = "CommandLaunchGroupDesc".Translate();
                launch.icon = CompLaunchable.LaunchCommandTex;
                launch.action = delegate ()
                {
                    shuttle.LaunchableComp.StartChoosingDestination(delegate (PlanetTile tile, TransportersArrivalAction action)
                    {
                        CaravanShuttleUtility.LaunchShuttle(caravan, tile, action);
                    });
                };
                AcceptanceReport report = CaravanShuttleUtility.CanLaunchCaravanShuttle(caravan);
                if (!report.Accepted)
                {
                    launch.Disable(report.Reason);
                }
                yield return launch;
                //Maybe refuel?
            }
        }
    }
}
