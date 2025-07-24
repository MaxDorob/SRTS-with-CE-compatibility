using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SRTS
{
    public class CompProperties_LaunchableSRTS : CompProperties_Shuttle
    {
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach(var error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if (parentDef.GetCompProperties<CompProperties_Launchable>() == null)
            {
                yield return "There's no CompLaunchable";
            }
            if (shipDef == null)
            {
                yield return "shipDef is null";
            }
            else
            {
                if (shipDef.leavingSkyfaller?.skyfaller.rotationCurve == null)
                {
                    yield return $"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.rotationCurve} is null";
                }
                if (shipDef.leavingSkyfaller?.skyfaller.speedCurve == null)
                {
                    yield return $"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.speedCurve} is null";
                }
                if (shipDef.leavingSkyfaller?.skyfaller.zPositionCurve == null)
                {
                    yield return $"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.zPositionCurve} is null";
                }
            }

        }
        public CompProperties_LaunchableSRTS()
        {
            this.compClass = typeof (CompLaunchableSRTS);
        }

        public float travelSpeed = 25f;
        public int minPassengers = 1;
        public int maxPassengers = 2;

        public float fuelPerTile = 2.25f;

        /* SOS2 Compatibility */
        public bool spaceFaring;
        public bool shuttleBayLanding;
        /* ------------------ */
    }
}
