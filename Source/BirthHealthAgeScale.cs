using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LifeStageAgeScaling.Patches
{
    [HarmonyPatch]
    public static class PregnancyBirthQuality_Patch
    {
        private static System.Reflection.MethodBase _target;
        private static bool _reportedMissingTarget;

        static System.Reflection.MethodBase TargetMethod()
        {
            if (_target != null)
                return _target;

            _target = AccessTools.Method(typeof(PregnancyUtility), "GetBirthQualityFor");
            return _target;
        }

        static bool Prepare()
        {
            bool ok = TargetMethod() != null;
            if (!ok && !_reportedMissingTarget)
            {
                _reportedMissingTarget = true;
                Log.Warning("[AgeScaling] Could not patch birth quality: PregnancyUtility.GetBirthQualityFor not found. Birth health scaling is disabled.");
            }

            return ok;
        }

        static void Postfix(Pawn mother, ref float __result)
        {
            if (mother == null)
                return;

            __result *= AgeScaleUtility.BirthQualityFactor(mother);
            __result = Mathf.Clamp01(__result);
        }
    }
}