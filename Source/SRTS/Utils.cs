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
    }
}
