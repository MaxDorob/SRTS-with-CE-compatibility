using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SRTS
{
    public class Designator_Landing : SRTSDesignator
    {
        public override bool Valid => CanDesignateCell(cell);
        public IntVec3 cell;
        public Rot4 currentRotation, savedRotation;

        public Designator_Landing(Map map, Thing transporter) : base(map, transporter)
        {
            currentRotation = savedRotation = transporter.Rotation;
        }
        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            RoyalTitlePermitWorker_CallShuttle.DrawShuttleGhost(cell, map, transporter.def, savedRotation);
            RoyalTitlePermitWorker_CallShuttle.DrawShuttleGhost(UI.MouseCell(), map, transporter.def, currentRotation);
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return base.CanDesignateCell(loc) && RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(loc, map, transporter.def, currentRotation);
        }
        public override void DesignateSingleCell(IntVec3 c)
        {
            cell = c;
            savedRotation = currentRotation;
        }
        public override void SelectedProcessInput(Event ev)
        {
            HandleRotationShortcuts();
            base.SelectedProcessInput(ev);
        }
        private void HandleRotationShortcuts()
        {
            RotationDirection rotationDirection = RotationDirection.None;
            //if (Event.current.button == 2)
            //{
            //    if (Event.current.type == EventType.MouseDown)
            //    {
            //        Event.current.Use();
            //        middleMouseDownTime = Time.realtimeSinceStartup;
            //    }
            //    if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - middleMouseDownTime < 0.15f)
            //    {
            //        rotationDirection = RotationDirection.Clockwise;
            //    }
            //}
            if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Clockwise;
            }
            if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
            {
                rotationDirection = RotationDirection.Counterclockwise;
            }
            if (rotationDirection != RotationDirection.None)
            {
                currentRotation.Rotate(rotationDirection);
            }
        }
    }
}
