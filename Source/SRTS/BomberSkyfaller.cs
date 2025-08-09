using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
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
                var bombCell = bombCells.FirstOrFallback(x => x.InHorDistOf(DrawPos.ToIntVec3(), 22f), IntVec3.Invalid);
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
                bombs.RemoveAt(0);
            }
        }

        private void DropSingleBomb(Thing bombStack, IntVec3 bombPos)
        {
            var singleBomb = bombStack.stackCount <= 1 ? bombStack : bombStack.SplitOff(1);
            if (SRTSHelper.CEModLoaded)
            {
                CEHelper.CEDropBomb(bombPos, singleBomb, this, GetCurrentTargetingRadius());
            }
            else
            {
                Projectile bombThing = GenerateProjectileFor(singleBomb);
                if (bombThing == null)
                {
                    Log.Warning($"Failed to spawn {singleBomb}");
                    return;
                }

                IntVec3 destination = (from x in GenRadial.RadialCellsAround(bombPos, GetCurrentTargetingRadius(), true)
                                       where x.InBounds(this.Map)
                                       select x).RandomElementByWeight((IntVec3 x) => 1f / Mathf.Sqrt(Mathf.Max((x - bombPos).LengthHorizontal, 0.5f)));

                GenSpawn.Spawn(bombThing, DrawPos.ToIntVec3(), this.Map);
                bombThing.Launch(this, DrawPos, destination, destination, ProjectileHitFlags.All);
                GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(bombThing, singleBomb.TryGetComp<CompExplosive>().Props.explosiveDamageType, null);
            }
        }
        private Projectile GenerateProjectileFor(Thing shell)
        {
            var projectileDef = shell.def.projectileWhenLoaded;
            if (projectileDef != null)
            {
                shell.Destroy();
                return ThingMaker.MakeThing(projectileDef) as Projectile;
            }

            var thing = ThingMaker.MakeThing(DefsOf.SRTSFallingBomb) as FallingBomb;
            thing.innerThing = shell;
            return thing;
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
