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
        protected SRTSDesignator designator;



        public bool Active => waitingTransporter != null;
        protected SRTSDesignator Designator => designator ??= InitDesignator();
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
            designator.map = map;
            Find.DesignatorManager.Select(designator);
        }
        public abstract string MoveDesignatorString { get; }
        public abstract string DesignatorIsNotValidString { get; }
        public virtual string AbortString => "Abort";
        public virtual string ConfirmString => "Confirm";
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
            Rect rect2 = new Rect(rect.xMin + 10f, rect.yMin + 10f, 200f, 50f);
            if (this.map != null)
            {
                if (Widgets.ButtonText(rect2, AbortString.Translate(), true, true, true, null))
                {
                    OnAbort();
                    this.map = null;
                    this.waitingTransporter = null;
                    Find.DesignatorManager.Deselect();
                }
                rect2.x += 210f;
            }
            Designator designator = this.Designator;
            if (Widgets.ButtonText(rect2, MoveDesignatorString.Translate(), true, true, true, null))
            {
                this.designator.map = this.map;
                Find.DesignatorManager.Select(designator);
            }

            rect2.x += 210f;
            if (Widgets.ButtonText(rect2, ConfirmString.Translate(), true, true, true, null))
            {
                if (Designator.Valid)
                {
                    OnConfirm();
                    this.map = null;
                    this.waitingTransporter = null;
                    Find.DesignatorManager.Deselect();
                }
                else
                {
                    Messages.Message(DesignatorIsNotValidString.Translate(), MessageTypeDefOf.RejectInput, true);
                }
            }
            if (Find.DesignatorManager.SelectedDesignator == designator)
            {
                Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, (float)(UI.screenHeight - 35));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, nameof(map));
            Scribe_References.Look(ref waitingTransporter, nameof(waitingTransporter));
        }

        protected abstract void OnConfirm();
        protected abstract void OnAbort();
        protected abstract SRTSDesignator InitDesignator();
    }
}
