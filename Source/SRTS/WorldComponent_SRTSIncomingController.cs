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
            public static void Postfix(ref bool __result) => __result = __result || Find.World.components.OfType<WorldComponent_SRTSIncomingController>().Any(x => x.Active);
        }
        [HarmonyLib.HarmonyPatch(typeof(MapParent), nameof(MapParent.CheckRemoveMapNow))]
        internal static class PreventMapRemove
        {
            public static bool Prefix(MapParent __instance) => !Find.World.components.OfType<WorldComponent_SRTSIncomingController>().Any(x => x.map == __instance.Map);
        }
        public WorldComponent_SRTSIncomingController(World world) : base(world)
        {
        }

        protected Map map;

        protected TravellingTransporters waitingTransporter;
        protected Designator designator;



        public bool Active => waitingTransporter != null;
        protected Designator Designator => designator ??= InitDesignator();
        public Thing SRTS => ThingOwnerUtility.GetAllThingsRecursively(waitingTransporter).Single(x => x.HasComp<CompLaunchableSRTS>());

        public void StartSelectingFor(Map map, TravellingTransporters transporter)
        {
            if (transporter == null)
            {
                Log.Error($"{nameof(transporter)} is null");
                return;
            }
            this.map = map;
            waitingTransporter = transporter;
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
            Scribe_References.Look(ref waitingTransporter, nameof(waitingTransporter));
        }

        protected abstract Designator InitDesignator();
    }
}
