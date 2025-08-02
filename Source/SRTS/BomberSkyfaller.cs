using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using SPExtended;
using Verse.Sound;
using CombatExtended;
using static UnityEngine.UI.Image;

namespace SRTS
{
    [StaticConstructorOnStartup]
    public class BomberSkyfaller : PassengerShuttleLeaving
    {
        public BomberSkyfaller() : base()
        {
            this.innerContainer = new ThingOwner<Thing>(this);
            this.bombCells = new List<IntVec3>();
        }

        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            factionInt = newFaction;
        }
        public override Vector3 DrawPos
        {
            get
            {
                return Vector3.Lerp(enterPos.ToVector3Shifted(), Position.ToVector3Shifted(), ((float)ticksToImpact) / ticksToDiscard).WithY(this.def.Altitude);
            }
        }
        public IntVec3 enterPos;
        public Thing SRTS => ThingOwnerUtility.GetAllThingsRecursively(this).Single(x => x.HasComp<CompLaunchableSRTS>());
        public float Speed => SRTSMod.GetStatFor<float>(SRTS.def.defName, StatName.bombingSpeed) * 10;
        public int Radius => SRTSMod.GetStatFor<int>(this.SRTS.def.defName, StatName.radiusDrop);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<IntVec3>(ref bombCells, "bombCells", LookMode.Value);
            Scribe_Defs.Look(ref sound, "sound");
        }



        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (sound != null)
                sound.PlayOneShotOnCamera(this.Map);
            if (!respawningAfterLoad)
            {
                ticksToDiscard = Mathf.CeilToInt((Position - enterPos).LengthHorizontal / Speed) * GenTicks.TicksPerRealSecond;
            }
        }


        protected override void GetDrawPositionAndRotation(ref Vector3 drawLoc, out float extraRotation)
        {
            base.GetDrawPositionAndRotation(ref drawLoc, out extraRotation);
            extraRotation += Vector3.Angle((Position - enterPos).ToVector3Shifted(), Vector3.forward);
        }

        protected override void Tick()
        {
            base.Tick();
            try
            {
                var bombCell = bombCells.FirstOrFallback(x => x.InHorDistOf(DrawPos.ToIntVec3(), 3f), IntVec3.Invalid);
                Log.Message(string.Join("\n", bombCells.Select(x => $"{x} in distance {x.DistanceTo(DrawPos.ToIntVec3())} to {DrawPos}")));
                if (bombCell != IntVec3.Invalid)
                {
                    this.DropBombs(bombCell);
                    bombCells.Remove(bombCell);
                }
            }
            catch (Exception ex)
            {

                Log.Error($"Failed to drop bombs: {ex.Message}" +
                    $"\nStackTrace:\n{ex.StackTrace}" +
                    $"\n------------------------------\n");
            }

        }
        private int BombsPerCell => bombs.Count / bombCells.Count;
        private void DropBombs(IntVec3 bombCell)
        {
            var bombsPerCell = BombsPerCell;
            for (int i = 0; i < bombsPerCell; i++)
            {
                DropSingleBomb(bombs[0], bombCell);
                if (bombs[0].stackCount <= 0)
                {
                    bombs.RemoveAt(0);
                }
            }
        }

        private void DropSingleBomb(Thing bombStack, IntVec3 bombPos)
        {
            int timerTickExplode = 20 + Rand.Range(0, 5); //Change later to allow release timer
            var singleBomb = bombStack.stackCount <= 1 ? bombStack : bombStack.SplitOff(1);
            if (SRTSHelper.CEModLoaded)
            {
                CEHelper.CEDropBomb(bombPos, singleBomb, this, GetCurrentTargetingRadius());
            }
            else
            {
                FallingBomb bombThing = new FallingBomb(singleBomb, singleBomb.TryGetComp<CompExplosive>(), this.Map, this.def.skyfaller.shadow);
                bombThing.HitPoints = int.MaxValue;
                bombThing.ticksRemaining = timerTickExplode;

                IntVec3 c = (from x in GenRadial.RadialCellsAround(bombPos, GetCurrentTargetingRadius(), true)
                             where x.InBounds(this.Map)
                             select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(this.Position) / GetCurrentTargetingRadius(), 1f) + 0.05f);
                bombThing.angle = this.angle + (SPTrig.LeftRightOfLine(this.DrawPos.ToIntVec3(), this.Position, c) * -10);
                bombThing.speed = (float)SPExtra.Distance(this.DrawPos.ToIntVec3(), c) / bombThing.ticksRemaining;
                Thing t = GenSpawn.Spawn(bombThing, c, this.Map);
                GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(t, singleBomb.TryGetComp<CompExplosive>().Props.explosiveDamageType, null);
            }
        }

        private int GetCurrentTargetingRadius()
        {
            switch (bombType)
            {
                case BombingType.carpet:
                    return Radius;
                case BombingType.precise:
                    return (int)(Radius * 0.6f);
                case BombingType.missile:
                    throw new NotImplementedException("BombingType");
                default:
                    throw new NotImplementedException("BombingType");
            }
        }


        public SoundDef sound;


        public List<IntVec3> bombCells = new List<IntVec3>();

        public List<Thing> bombs = new List<Thing>();

        public int precisionBombingNumBombs;


        public BombingType bombType;
    }
    public class SRTSBombing : BomberSkyfaller //Backward compatibility
    {
    }
}
