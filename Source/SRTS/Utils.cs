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
    internal static class Utils
    {
#if RELEASE1_6
        public static void AddPod(this TravelingTransportPods pod, ActiveTransporterInfo contents, bool justLeftTheMap)
        {
            pod.AddTransporter(contents, justLeftTheMap);
        }
        public static float GetDamageAmount(this ProjectileProperties projectile, float amount)
        {
            return projectile.GetDamageAmount(amount, null);
        }
        public static float GetArmorPenetration(this ProjectileProperties projectile, float amount)
        {
            return projectile.GetArmorPenetration(null);
        }
#endif

        public static IntVec3 GetEdgeCell(Map map, Vector3 dir, IntVec3 from)
        {
            var mapBounds = new Bounds(map.Center.ToVector3(), map.Size.ToVector3().WithY(1f) - new Vector3(0.5f, 0, 0.5f)); //A dumb temp solution
            var ray = new Ray(from.ToVector3().Yto0(), dir.Yto0());
            if (!mapBounds.IntersectRay(ray, out var dist))
            {
                Log.Error("Cannot find a cell");
            }
            var cell = (from.ToVector3() + dir.normalized * dist).ToIntVec3();
            if (!cell.InBounds(map))
            {
                Log.Warning($"{cell} is not in bounds {mapBounds} ({mapBounds.min} - {mapBounds.max})");
                cell.x = Mathf.Clamp(cell.x, 0, map.Size.x - 1);
                cell.z = Mathf.Clamp(cell.z, 0, map.Size.z - 1);
            }
            return cell;
        }
    }
}
