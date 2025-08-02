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
            var numRings = ((int)(targetingLength / SRTSMod.GetStatFor<float>(this.Transporter.def.defName, StatName.distanceBetweenDrops))).Clamp<int>(0, SRTSMod.GetStatFor<int>(this.Transporter.def.defName, StatName.numberBombs));
            for (int i = 0; i < numRings; i++)
            {
                buffer.Add(TargeterToCell(origin, target, i));
            }
        }
        private IntVec3 TargeterToCell(IntVec3 start, IntVec3 end, int bombNumber)
        {
            var targetingLength = (start - end).LengthHorizontal;
            var numRings = ((int)(targetingLength / SRTSMod.GetStatFor<float>(this.Transporter.def.defName, StatName.distanceBetweenDrops))).Clamp<int>(0, SRTSMod.GetStatFor<int>(this.Transporter.def.defName, StatName.numberBombs));
            double angle = start.AngleToPoint(end);
            float distanceToNextBomb = SRTSMod.mod.settings.expandBombPoints ? targetingLength / (numRings - 1) * bombNumber : SRTSMod.GetStatFor<float>(this.Transporter.def.defName, StatName.distanceBetweenDrops) * bombNumber;
            float xDiff = start.x + Math.Sign(end.x - start.ToVector3Shifted().x) *
                (float)(distanceToNextBomb * Math.Cos(angle.DegreesToRadians()));
            float zDiff = start.z + Math.Sign(end.z - start.ToVector3Shifted().z) *
                (float)(distanceToNextBomb * Math.Sin(angle.DegreesToRadians()));
            return new IntVec3((int)xDiff, 0, (int)zDiff);
        }
    }
}
