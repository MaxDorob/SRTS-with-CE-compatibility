using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SRTS
{
    public abstract class SRTSDesignator : Designator
    {
        public override float Order
        {
            get
            {
                return -100f;
            }
        }

        public override bool AlwaysDoGuiControls
        {
            get
            {
                return true;
            }
        }

        public float PaneTopY
        {
            get
            {
                return (float)(UI.screenHeight - 35);
            }
        }

        public abstract bool Valid { get; }

        public SRTSDesignator(Map map, Thing transporterInfo)
        {
            this.transporter = transporterInfo;
            this.map = map;
        }


        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (!loc.IsValid)
            {
                return "InvalidCell".Translate();
            }
            if (!loc.InBounds(map))
            {
                return "OutOfBounds".Translate();
            }
            return true;
        }



        public override void Selected()
        {
            if ((Find.CurrentMap != this.map) || Find.World.renderer.wantedMode == WorldRenderMode.Planet)
            {
                CameraJumper.TryJump(this.map.Center, this.map, CameraJumper.MovementMode.Pan);
            }
        }

        public Thing transporter;

        public Map map;

        private Rot4 deselectedRotation;

        private static float middleMouseDownTime;

    }
}
