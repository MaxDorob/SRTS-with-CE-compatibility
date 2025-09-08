using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.UI.Image;

namespace SRTS
{
    public class DrawStyle_Precise : DrawStyle_SRTS
    {
        public override BombingType BombingType => BombingType.precise;

        public override int BombCountPerPoint => SRTSMod.GetStatFor<int>(Transporter.def.defName, StatName.precisionBombingNumBombs);

        public override void Update(IntVec3 start, IntVec3 end, List<IntVec3> buffer)
        {
            if (Transporter == null)
            {
                return;
            }

            buffer.Add(Vector3.Lerp(start.ToVector3Shifted(), end.ToVector3Shifted(), 0.5f).ToIntVec3());
        }
    }
}
