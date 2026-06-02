using HarmonyLib;
using RimWorld;
using Verse;

namespace LifeStageAgeScaling.Patches
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "LovinAgeFactor")]
    public static class LovinAgeFactor_Patch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Pawn ___pawn, Pawn __0, ref float __result)
        {
            if (___pawn == null || __0 == null)
                return;

            __result = AgeScaleUtility.RomanceAgeCompatibility(___pawn, __0);

            Log.Message($"[AgeScale] LovinAgeFactor patched {___pawn.LabelShortCap} -> {__0.LabelShortCap} = {__result:F3}");
        }
    }
}