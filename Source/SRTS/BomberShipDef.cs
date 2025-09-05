using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    public class BomberShipDef : TransportShipDef
    {
        public ThingDef bomberSkyfaller;
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
            {
                yield return error;
            }
            if (bomberSkyfaller == null)
            {
                yield return $"{nameof(bomberSkyfaller)} is null";
            }
        }
    }
}
