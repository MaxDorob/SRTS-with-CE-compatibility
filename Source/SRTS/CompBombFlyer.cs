using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace SRTS
{
    public enum BombingType { carpet, precise, missile }
    public class CompBombFlyer : ThingComp
    {
        public Building SRTS_Launcher => this.parent as Building;
        public CompLaunchableSRTS CompLauncher => SRTS_Launcher.GetComp<CompLaunchableSRTS>();
        public CompProperties_BombsAway Props => (CompProperties_BombsAway)this.props;

        internal IEnumerable<FloatMenuOption> FloatMenuOptionsAt(PlanetTile tile, Action<PlanetTile, TransportPodsArrivalAction> launchAction)
        {
            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < worldObjects.Count; i++)
            {
                var wObj = worldObjects[i];
                if (wObj.Tile == tile && wObj is MapParent mapParent)
                {
                    foreach (var option in TransporterArrivalOption_PrepareBombRun.GetFloatMenuOptions(launchAction, this.parent.GetComp<CompTransporter>().TransportersInGroup(this.parent.Map), mapParent))
                    {
                        yield return option;
                    }


                }
            }
        }
    }
}
