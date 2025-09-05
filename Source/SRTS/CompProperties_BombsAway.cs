using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SRTS
{
    public class CompProperties_BombsAway : CompProperties
    {
        public CompProperties_BombsAway()
        {
            this.compClass = typeof(CompBombFlyer);
        }

        public int numberBombs = 3;
        public int precisionBombingNumBombs = 1;
        public int radiusOfDrop = 10;
        public float speed = 0.8f;
        public float distanceBetweenDrops = 5f;
        public SoundDef soundFlyBy;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if (parentDef.GetCompProperties<CompProperties_LaunchableSRTS>()?.shipDef as BomberShipDef == null)
            {
                yield return "shipDef is not " + nameof(BomberShipDef);
            }
        }
    }
}
