using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;


namespace SRTS
{
    [StaticConstructorOnStartup]
    public static class StartUp
    {
        static Harmony harmony;
        static StartUp()
        {
            harmony = new Harmony("SRTS");
            harmony.PatchAll();
        }
    }
}
