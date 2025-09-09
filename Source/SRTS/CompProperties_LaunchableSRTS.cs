﻿using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace SRTS
{
    public class CompProperties_LaunchableSRTS : CompProperties_Shuttle
    {
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach(var error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            if (SRTSMod.mod.settings.autopatcher && !LegacyAddonsAutopatcher.autoPatchProcessed)
            {
                yield break;
            }
            if (parentDef.GetCompProperties<CompProperties_Launchable>() == null)
            {
                yield return "There's no CompLaunchable (you can try to enable autopatcher to fix this issue)";
            }
            CompProperties_Transporter transporterProps = parentDef.GetCompProperties<CompProperties_Transporter>();
            if (transporterProps == null)
            {
                yield return "There's no " + nameof(CompProperties_Transporter);
            }
            else if (!transporterProps.max1PerGroup)
            {
                yield return $"{nameof(CompProperties_Transporter)}.{nameof(CompProperties_Transporter.max1PerGroup)} is false (you can try to enable autopatcher to fix this issue)";
            }
            if (shipDef == null)
            {
                yield return "shipDef is null (you can try to enable autopatcher to fix this issue)";
            }
            else
            {
                if (shipDef.leavingSkyfaller?.skyfaller.rotationCurve == null)
                {
                    yield return $"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.rotationCurve} is null (you can try to enable autopatcher to fix this issue)";
                }
                if (shipDef.leavingSkyfaller?.skyfaller.speedCurve == null)
                {
                    yield return $"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.speedCurve} is null (you can try to enable autopatcher to fix this issue)";
                }
                if (shipDef.leavingSkyfaller?.skyfaller.zPositionCurve == null)
                {
                    yield return $"{nameof(shipDef.leavingSkyfaller)}'s {shipDef.leavingSkyfaller.skyfaller.zPositionCurve} is null (you can try to enable autopatcher to fix this issue)";
                }
            }
            if ((parentDef.GetCompProperties<CompProperties_Power>()?.PowerConsumption ?? 0.0f) < -0.00001f)
            {
                if (parentDef.GetCompProperties<CompProperties_Refuelable>().consumeFuelOnlyWhenUsed)
                {
                    yield return "consumeFuelOnlyWhenUsed is true and it generates power (infinite power source)";
                }
            }
            if (!SRTSMod.mod.settings.autopatcher)
            {
                if (fuelPerTile != null)
                {
                    yield return $"{nameof(fuelPerTile)} must be specified in {nameof(CompProperties_Launchable)}";
                }
                if (shuttleBayLanding != null)
                {
                    yield return $"{nameof(shuttleBayLanding)} is obsolete";
                }
            }

        }
        public CompProperties_LaunchableSRTS()
        {
            this.compClass = typeof (CompLaunchableSRTS);
        }

        public float travelSpeed = 25f;
        public int minPassengers = 1;
        public int maxPassengers = 2;

        [Obsolete]
        public float? fuelPerTile;
        [Obsolete]
        public bool? shuttleBayLanding;

        public bool spaceFaring;
    }
}
