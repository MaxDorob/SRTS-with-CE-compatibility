using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace SRTS
{
    [StaticConstructorOnStartup]
    public class FallingBomb : Projectile
    {
        public FallingBomb()
        {
        }
        public Thing innerThing;
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            var map = Map;
            base.Impact(hitThing, blockedByShield);
            if (!blockedByShield)
            {
                GenSpawn.Spawn(innerThing, CellFinder.FindNoWipeSpawnLocNear(destination.ToIntVec3(), map, innerThing.def, Rot4.North, 4), map);
                var explosiveComp = innerThing.TryGetComp<CompExplosive>();
                if (explosiveComp != null)
                {
                    explosiveComp.Detonate(map);
                }

            }
        }
        public override Graphic Graphic => innerThing.Graphic;
        public override Material DrawMat => Graphic.MatSingle;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref innerThing, nameof(innerThing));
        }
    }
}
