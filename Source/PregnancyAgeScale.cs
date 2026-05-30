using HarmonyLib;
using RimWorld;
using Verse;

namespace LifeStageAgeScaling.Patches
{
    [HarmonyPatch]
    public static class PregnancyUtility_Patch
    {
        private static System.Reflection.MethodBase _target;
        private static bool _reportedMissingTarget;

        static System.Reflection.MethodBase TargetMethod()
        {
            if (_target != null)
                return _target;

            _target = AccessTools.Method(typeof(PregnancyUtility), "PregnancyChanceForPawn")
                      ?? AccessTools.Method(typeof(PregnancyUtility), "PregnancyChanceForWoman")
                      ?? AccessTools.Method(typeof(PregnancyUtility), "PregnancyChanceForPartners");

            return _target;
        }

        static bool Prepare()
        {
            bool ok = TargetMethod() != null;
            if (!ok && !_reportedMissingTarget)
            {
                _reportedMissingTarget = true;
                Log.Warning("[AgeScaling] Could not patch pregnancy chance: no known PregnancyUtility chance method found. Fertility scaling is disabled.");
            }

            return ok;
        }

        static bool Prefix(Pawn pawn, ref float __result)
        {
            __result = AgeScaleUtility.FertilityFactor(pawn);
            return false; // skip vanilla
        }
    }
}