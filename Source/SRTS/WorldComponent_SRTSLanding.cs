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
    public class WorldComponent_SRTSLanding : WorldComponent_SRTSIncomingController
    {
        public WorldComponent_SRTSLanding(World world) : base(world)
        {
        }
        public new Designator_Landing Designator => base.Designator as Designator_Landing;
        protected override void DrawButtons(Rect rect)
        {
            Rect rect2 = new Rect(rect.xMin + 10f, rect.yMin + 10f, 200f, 50f);
            if (this.map != null)
            {
                if (Widgets.ButtonText(rect2, "Abort".Translate(), true, true, true, null))
                {
                    this.map = null;
                    this.waitingTransporter = null;
                    Find.DesignatorManager.Deselect();
                }
                rect2.x += 210f;
            }
            Designator designator = this.Designator;
            if (Widgets.ButtonText(rect2, "SRTSSelectLandingSpot".Translate(), true, true, true, null))
            {
                Designator.map = this.map;
                Find.DesignatorManager.Select(designator);
            }

            rect2.x += 210f;
            if (Widgets.ButtonText(rect2, "SRTSLandHere".Translate(), true, true, true, null))
            {
                if (Designator.Valid)
                {
                    OnConfirm();
                }
                else
                {
                    Messages.Message("SRTSCannotLandHere".Translate(), MessageTypeDefOf.RejectInput, true);
                }
            }
            if (Find.DesignatorManager.SelectedDesignator == designator)
            {
                Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, (float)(UI.screenHeight - 35));
            }
        }

        private void OnConfirm()
        {
            waitingTransporter.arrivalAction = new TransportersArrivalAction_LandInSpecificCell(map.Parent, Designator.cell, Designator.savedRotation, true);
            waitingTransporter.Arrived();
            this.map = null;
            this.waitingTransporter = null;
            Find.DesignatorManager.Deselect();
        }

        protected override Designator InitDesignator()
        {
            var designator = new Designator_Landing(map, SRTS);
            return designator;
        }
    }
}
