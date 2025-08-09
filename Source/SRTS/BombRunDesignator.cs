using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Verse;

namespace SRTS
{
    public class BombRunDesignator : SRTSDesignator
    {
        public BombRunDesignator(Map map, Thing transporterInfo, IEnumerable<Thing> availableBombs) : base(map, transporterInfo)
        {
        }

        public IntVec3 start = IntVec3.Invalid, end = IntVec3.Invalid;
        private const float RadiusPreciseMultiplier = 0.6f;

        public List<IntVec3> BombCells
        {
            get
            {
                var list = new List<IntVec3>();
                Find.DesignatorManager.SelectedStyle.DrawStyleWorker.Update(start, end, list);
                return list;
            }
        }
        public BombingType BombingType => (Find.DesignatorManager.SelectedStyle.DrawStyleWorker as DrawStyle_SRTS).BombingType;

        public override bool DrawHighlight => false; // Don't use default highlight
        public override bool Valid => CanDesignateCell(start) && CanDesignateCell(end);

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            foreach (var cell in dragCells)
            {
                GenDraw.DrawRadiusRing(cell, SRTSMod.GetStatFor<int>(transporter.def.defName, StatName.radiusDrop));
                GenDraw.DrawTargetHighlight(cell);
            }
        }
        public override void DesignateSingleCell(IntVec3 c)
        { 
        }
        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            this.start = Find.DesignatorManager.Dragger.lastStart;
            this.end = Find.DesignatorManager.Dragger.lastEnd;
            var list = new List<IntVec3>();
            Find.DesignatorManager.SelectedStyle.DrawStyleWorker.Update(start, end, list);
            GenDraw.DrawTargetHighlight(start);
            GenDraw.DrawTargetHighlight(end);
            GenDraw.DrawLineBetween(Utils.GetEdgeCell(map, (end - start).ToVector3Shifted(), start).ToVector3Shifted(), end.ToVector3Shifted());
            RenderHighlight(list);
        }
        public override void DoExtraGuiControls(float leftX, float bottomY)
        {
            Log.Message(nameof(DoExtraGuiControls));
            base.DoExtraGuiControls(leftX, bottomY);
        }
        public override DrawStyleCategoryDef DrawStyleCategory => DefsOf.SRTSBombing;
    }
}
