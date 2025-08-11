using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SRTS
{
    public abstract class WorldComponent_SRTSIncomingController : WorldComponent
    {
        [HarmonyLib.HarmonyPatch(typeof(TickManager), nameof(TickManager.ForcePaused), HarmonyLib.MethodType.Getter)]
        internal static class ForcePaused_Patch
        {
            public static void Postfix(ref bool __result) => __result = __result || ForceStopActive;
        }
        [HarmonyLib.HarmonyPatch(typeof(MapParent), nameof(MapParent.CheckRemoveMapNow))]
        internal static class PreventMapRemove
        {
            public static bool Prefix(MapParent __instance) => !ForceStopActive || !Find.World.components.OfType<WorldComponent_SRTSIncomingController>().Any(x => x.map == __instance.Map);
        }
        private static bool forceStopActive = false;
        public static bool ForceStopActive
        {
            get
            {
                if (!forceStopActive)
                {
                    return false;
                }
                VerifyForceStop();
                return forceStopActive;
            }
        }
        private static void VerifyForceStop(bool silent = false)
        {
            var mustBeStopped = Find.World.components.OfType<WorldComponent_SRTSIncomingController>().Any(x => x.Active);
            if (forceStopActive != mustBeStopped)
            {
                if (!silent)
                {
                    Log.Warning($"Wrong state of {nameof(WorldComponent_SRTSIncomingController)}.{nameof(WorldComponent_SRTSIncomingController.ForceStopActive)}. Fixing...");
                }
                forceStopActive = mustBeStopped;
            }
        }
        public WorldComponent_SRTSIncomingController(World world) : base(world)
        {
        }

        protected Map map;

        private TravellingTransporters waitingTransporter;
        protected Designator designator;


        public TravellingTransporters WaitingTransporter
        {
            get
            {
                return waitingTransporter;
            }
            set
            {
                waitingTransporter = value;
                if (value == null)
                {
                    map = null;
                    forceStopActive = false;
                }
                else
                {
                    forceStopActive = true;
                }
            }
        }

        public bool Active => WaitingTransporter != null;
        protected Designator Designator => designator ??= InitDesignator();
        public Thing SRTS => ThingOwnerUtility.GetAllThingsRecursively(WaitingTransporter).Single(x => x.HasComp<CompLaunchableSRTS>());

        public void StartSelectingFor(Map map, TravellingTransporters transporter)
        {
            if (transporter == null)
            {
                Log.Error($"{nameof(transporter)} is null");
                return;
            }

            this.map = map;
            WaitingTransporter = transporter;
            var designator = Designator;
            Find.DesignatorManager.Select(designator);
        }
        public override void WorldComponentOnGUI()
        {
            if (!this.Active || Find.ScreenshotModeHandler.Active || !WorldRendererUtility.DrawingMap)
            {
                return;
            }
            float num = 430f;
            if (this.map != null)
            {
                num = 640f;
            }
            Rect rect = new Rect((float)UI.screenWidth / 2f - num / 2f, (float)UI.screenHeight - 150f - 70f, num, 70f);
            Widgets.DrawWindowBackground(rect);
            DrawButtons(rect);
        }
        protected abstract void DrawButtons(Rect inRect);
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, nameof(map));
            Scribe_References.Look(ref waitingTransporter, nameof(WaitingTransporter));
            VerifyForceStop(true);
        }

        protected abstract Designator InitDesignator();
    }
}
