using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace LifeStageAgeScaling.Patches
{
    [HarmonyPatch]
    public static class FertilityAgeFactor_Patch
    {
        private static MethodBase _target;
        private static bool _reportedMissingTarget;

        static MethodBase TargetMethod()
        {
            if (_target != null)
                return _target;

            _target = AccessTools.Method(typeof(StatPart_FertilityByGenderAge), "AgeFactor", new[] { typeof(Pawn) });

            return _target;
        }

        static bool Prepare()
        {
            bool ok = TargetMethod() != null;
            if (!ok && !_reportedMissingTarget)
            {
                _reportedMissingTarget = true;
                Log.Warning("[AgeScaling] Could not patch StatPart_FertilityByGenderAge.AgeFactor for fertility scaling.");
            }

            return ok;
        }

        static void Postfix(Pawn pawn, ref float __result)
        {
            if (pawn == null)
                return;

            __result = AgeScaleUtility.FertilityFactor(pawn);
        }
    }
}