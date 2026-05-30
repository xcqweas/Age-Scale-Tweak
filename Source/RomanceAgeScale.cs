using HarmonyLib;
using RimWorld;
using Verse;

namespace LifeStageAgeScaling.Patches
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
    public static class RomanceChance_Patch
    {
        static void Postfix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            if (___pawn == null || otherPawn == null)
                return;

            float factor = AgeScaleUtility.RomanceAgeCompatibility(___pawn, otherPawn);

            // Option A: replace only if vanilla age penalty is too harsh.
            __result = factor;

            // Option B: softer, preserves vanilla non-age logic if the method includes more than age.
            // __result *= factor;
        }
    }
}