using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SRTS
{
    public class WorldComponent_BomberController : WorldComponent_SRTSIncomingController
    {
        public WorldComponent_BomberController(World world) : base(world)
        {
        }

        public IEnumerable<Thing> Bombs => ThingOwnerUtility.GetAllThingsRecursively(waitingTransporter).Where(x => SRTSMod.mod.settings.allowedBombs.Contains(x.def.defName));

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
                    }
                }
            }
        }

        public override string MoveDesignatorString => "SRTSChangeBombRunTrajectory";

        public override string DesignatorIsNotValidString => "SRTSBombRunTrajectoryIsNotValid";

        protected override SRTSDesignator InitDesignator() => new BombRunDesignator(map, SRTS, Bombs);

        protected override void OnAbort()
        {

        }

        protected override void OnConfirm()
        {
            var designator = (Designator as BombRunDesignator);
            waitingTransporter.arrivalAction = new TransporterArrivalOption_BombRun(map, designator.start, designator.end, designator.BombCells, SelectedBombs.ToList(), designator.BombingType);

            // Assign bomb run arrivalAction there
        }
    }
}
