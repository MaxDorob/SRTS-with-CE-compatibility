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
        public override bool Valid => CanDesignateCell(start) && CanDesignateCell(end) && start != end;

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            if (dragCells.Count > 0)
            {
                dragCells = dragCells.Distinct().ToList();
                IEnumerable<Thing> bombs = Find.World.GetComponent<WorldComponent_BomberController>().SelectedBombs.ToList();
                for (int i = 0; i < dragCells.Count; i++)
                {
                    IntVec3 cell = dragCells[i];
                    var cellsLeft = dragCells.Count - i;
                    GenDraw.DrawRadiusRing(cell, SRTSMod.GetStatFor<int>(transporter.def.defName, StatName.radiusDrop));
                    GenDraw.DrawTargetHighlight(cell);
                    int bombsPerCell = Mathf.Max(Mathf.CeilToInt((float)bombs.Count() / cellsLeft), 1);
                    var bombsForCell = bombs.Take(bombsPerCell).ToList();
                    Vector3 offset = new Vector3(0.7f, AltitudeLayer.MapDataOverlay.AltitudeFor(), -0.2f);

                    foreach (var bomb in bombsForCell)
                    {
                        Graphics.DrawMesh(MeshPool.plane10, cell.ToVector3() + offset, Quaternion.identity, bomb.Graphic.MatSingle, 0);

                        offset.x += 0.3f;
                        offset.y += 0.3f;
                        if (offset.x > 0.68f + 0.3f * 3)
                        {
                            offset.z += -0.5f;
                            offset.x = 0.42f;
                        }
                    }

                    bombs = bombs.Skip(bombsPerCell);
                }
            }
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
        }
        public override void DrawMouseAttachments()
        {
            base.DrawMouseAttachments();
            var bombs = Find.World.GetComponent<WorldComponent_BomberController>().Bombs.ToList();
            if (bombs.Count == 0)
            {
                return;
            }
            var index = (int)GenMath.PositiveMod(Time.realtimeSinceStartup / 2f, bombs.Count);
            GenUI.DrawMouseAttachment(bombs[index].def.uiIcon);
        }
        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            this.start = Find.DesignatorManager.Dragger.lastStart;
            this.end = Find.DesignatorManager.Dragger.lastEnd;
            var list = new List<IntVec3>();
            Find.DesignatorManager.SelectedStyle.DrawStyleWorker.Update(start, end, list);
            GenDraw.DrawTargetHighlight(start);
            if (start != end)
            {
                GenDraw.DrawTargetHighlight(end);
                GenDraw.DrawLineBetween(Utils.GetEdgeCell(map, (end - start).ToVector3(), start).ToVector3Shifted(), end.ToVector3Shifted());
            }
            RenderHighlight(list);
        }
        public override DrawStyleCategoryDef DrawStyleCategory => DefsOf.SRTSBombing;
    }
}
