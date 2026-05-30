using HarmonyLib;
using Verse;

namespace LifeStageAgeScaling
{
    [StaticConstructorOnStartup]
    public static class AgeScaleModInit
    {
        static AgeScaleModInit()
        {
            new Harmony("xcqweas.AgeScaling").PatchAll();
        }
    }
}