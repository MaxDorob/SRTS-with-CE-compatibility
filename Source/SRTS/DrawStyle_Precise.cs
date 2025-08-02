using SPExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var targetingLength = (start - end).LengthHorizontal;
            IntVec3 targetedCell = new IntVec3(end.x, start.y, end.z);
            double angle = start.AngleToPoint(end);
            float xDiff = start.x + Math.Sign(targetedCell.x - start.ToVector3Shifted().x) * (float)(targetingLength / 2 * Math.Cos(angle.DegreesToRadians()));
            float zDiff = start.z + Math.Sign(targetedCell.z - start.ToVector3Shifted().z) * (float)(targetingLength / 2 * Math.Sin(angle.DegreesToRadians()));
            buffer.Add(new IntVec3((int)xDiff, 0, (int)zDiff));
        }
    }
}
