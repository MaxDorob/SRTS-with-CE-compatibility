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
    public class WorldComponent_BomberController : WorldComponent_SRTSIncomingController
    {
        public WorldComponent_BomberController(World world) : base(world)
        {
        }

        public IEnumerable<Thing> Bombs => ThingOwnerUtility.GetAllThingsRecursively(WaitingTransporter).Where(x => SRTSMod.mod.settings.allowedBombs.Contains(x.def.defName));

        public IEnumerable<Thing> SelectedBombs
        {
            get
            {
                int totalNeeded = 0;
                switch ((Designator as BombRunDesignator).BombingType)
                {
                    case BombingType.carpet:
                        totalNeeded = SRTSMod.GetStatFor<int>(SRTS.def.defName, StatName.numberBombs);
                        break;
                    case BombingType.precise:
                        totalNeeded = SRTSMod.GetStatFor<int>(SRTS.def.defName, StatName.precisionBombingNumBombs);
                        break;
                }
                foreach (Thing bombStack in Bombs)
                {
                    var count = Math.Min(totalNeeded, bombStack.stackCount);
                    for (int i = 0; i < count; i++)
                    {
                        yield return bombStack;
                        totalNeeded--;
                        if (totalNeeded <= 0)
                        {
                            yield break;
                        }
                    }
                }
            }
        }

        private void SendMessageNotValid() => Messages.Message("SRTSBombRunTrajectoryIsNotValid".Translate(), MessageTypeDefOf.RejectInput, true);


        protected override Designator InitDesignator()
        {
            var designator = new BombRunDesignator(map, SRTS, Bombs);
            designator.map = map;
            return designator;
        }
        public new BombRunDesignator Designator => base.Designator as BombRunDesignator;
        protected void OnConfirm<PostBombArrivalAction>(PlanetTile destination) where PostBombArrivalAction : TransportersArrivalAction, new()
        {
            var designator = (Designator as BombRunDesignator);
            WaitingTransporter.arrivalAction = new TransporterArrivalOption_BombRun<PostBombArrivalAction>(map, designator.start, designator.end, designator.BombCells, SelectedBombs.ToList(), designator.BombingType, destination);
            WaitingTransporter.Arrived();
            this.map = null;
            this.WaitingTransporter = null;
            Find.DesignatorManager.Deselect();
        }
        protected override void DrawButtons(Rect rect)
        {
            const int buttonCounts = 5;
            const float margin = 10;
            float buttonWidth = (rect.width - margin * (buttonCounts + 1)) / buttonCounts;
            Rect rect2 = new Rect(rect.xMin + margin, rect.yMin + margin, buttonWidth, rect.height - margin * 2);
            if (this.map != null)
            {
                if (Widgets.ButtonText(rect2, "Abort".Translate(), true, true, true, null))
                {
                    WaitingTransporter.arrivalAction = new TransportersArrivalAction_FormCaravan();
                    WaitingTransporter.destinationTile = WaitingTransporter.Tile;
                    this.map = null;
                    this.WaitingTransporter = null;
                    Find.DesignatorManager.Deselect();
                }
                rect2.x += buttonWidth + margin;
            }
            Designator designator = this.Designator;
            if (Widgets.ButtonText(rect2, "SRTSChangeBombRunTrajectory".Translate(), true, true, true, null))
            {
                this.Designator.map = this.map;
                Find.DesignatorManager.Select(designator);
            }

            rect2.x += buttonWidth + margin;
            if (Widgets.ButtonText(rect2, "SRTSBombAndFormCaravan".Translate(), true, true, true, null))
            {
                if (Designator.Valid)
                {
                    OnConfirm<TransportersArrivalAction_FormCaravan>(map.Tile);
                }
                else
                {
                    SendMessageNotValid();
                }
            }
            rect2.x += buttonWidth + margin;
            AcceptanceReport goBackHomeActive = true;
            float fuelRequired = -1f;
            if (!(SRTS.TryGetComp<CompLaunchableSRTS>().TryFindHomePoint(out var homeMap, out _, out _)))
            {
                goBackHomeActive = "SRTSCannotFindHomePoint".Translate();
            }
            else
            {
                var launchable = SRTS.TryGetComp<CompLaunchable>();
                var dist = Find.WorldGrid.TraversalDistanceBetween(WaitingTransporter.Tile, homeMap.Tile);
                fuelRequired = dist * (SRTSMod.GetStatFor<float>(SRTS.def.defName, StatName.fuelPerTile) * homeMap.Tile.Layer.Def.rangeDistanceFactor);
                if (launchable.FuelLevel < fuelRequired)
                {
                    goBackHomeActive = "TransportPodNotEnoughFuel".Translate();
                }
            }
            var goBackHomeText = "SRTSBombAndGoBackToHome".Translate();
            if (!goBackHomeActive)
            {
                goBackHomeText += $"\n({goBackHomeActive.Reason})";
            }
            if (Widgets.ButtonText(rect2, goBackHomeText, true, true, goBackHomeActive.Accepted, null))
            {
                if (Designator.Valid)
                {
                    SRTS.TryGetComp<CompRefuelable>()?.ConsumeFuel(fuelRequired);
                    OnConfirm<TransporterArrivalAction_GoBackToHome>(homeMap.Tile);
                }
                else
                {
                    SendMessageNotValid();
                }

            }
            rect2.x += buttonWidth + margin;
            if (Widgets.ButtonText(rect2, "SRTSBombAndLandHere".Translate(), true, true, true, null))
            {
                if (Designator.Valid)
                {
                    OnConfirm<TransporterArrivalAction_FindLandingPlace>(map.Tile);
                }
                else
                {
                    SendMessageNotValid();
                }
            }
            if (Find.DesignatorManager.SelectedDesignator == designator)
            {
                Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, (float)(UI.screenHeight - 35));
            }
        }
    }
}
