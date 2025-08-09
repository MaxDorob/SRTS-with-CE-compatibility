using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace SRTS
{
    public class DrawStyle_Carpet : DrawStyle_SRTS
    {
        public override BombingType BombingType => BombingType.carpet;

        public override int BombCountPerPoint => SRTSMod.GetStatFor<int>(Transporter.def.defName, StatName.numberBombs);

        public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
        {
            if (Transporter == null)
            {
                return;
            }
            var targetingLength = (origin - target).LengthHorizontal;
            var numRings = Mathf.Clamp(((int)(targetingLength / SRTSMod.GetStatFor<float>(this.Transporter.def.defName, StatName.distanceBetweenDrops))),0, SRTSMod.GetStatFor<int>(this.Transporter.def.defName, StatName.numberBombs));
            for (int i = 0; i < numRings; i++)
            {
                buffer.Add(TargeterToCell(origin, target, i));
            }
        }
        private IntVec3 TargeterToCell(IntVec3 start, IntVec3 end, int bombNumber)
        {
            var targetingLength = (start - end).LengthHorizontal;
            var numRings = Mathf.Clamp(((int)(targetingLength / SRTSMod.GetStatFor<float>(this.Transporter.def.defName, StatName.distanceBetweenDrops))), 0, SRTSMod.GetStatFor<int>(this.Transporter.def.defName, StatName.numberBombs)) - 1; // zero is a point as well
            return Vector3.Lerp(start.ToVector3Shifted(), end.ToVector3Shifted(), bombNumber / (float)numRings).ToIntVec3();
        }
    }
}
