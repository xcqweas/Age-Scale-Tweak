using UnityEngine;
using Verse;

namespace LifeStageAgeScaling
{
    public static class AgeScaleUtility
    {
        public const float HumanLifeExpectancy = 80f;

        public static float BiologicalAge(Pawn pawn)
        {
            return pawn.ageTracker.AgeBiologicalYearsFloat;
        }

        public static float LifeExpectancy(Pawn pawn)
        {
            return Mathf.Max(pawn?.RaceProps?.lifeExpectancy ?? HumanLifeExpectancy, 1f);
        }

        public static float FirstReproductiveAge(Pawn pawn)
        {
            if (pawn?.RaceProps?.lifeStageAges != null)
            {
                foreach (LifeStageAge stage in pawn.RaceProps.lifeStageAges)
                {
                    if (stage.def.reproductive)
                        return stage.minAge;
                }
            }

            return 13f;
        }

        public static float BiologicalMaturityAge(Pawn pawn)
        {
            float reproductive = FirstReproductiveAge(pawn);
            float life = LifeExpectancy(pawn);

            // Prevent long-lived races from becoming "full adults" at 13
            // just because the race mod reused human reproductive ages.
            return Mathf.Max(reproductive, life * 0.10f);
        }

        public static float AdultProgress(Pawn pawn)
        {
            float age = BiologicalAge(pawn);
            float maturity = BiologicalMaturityAge(pawn);
            float life = LifeExpectancy(pawn);

            return Mathf.Clamp01((age - maturity) / Mathf.Max(1f, life - maturity));
        }

        public static float SmoothStep(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        public static float LongevityPrimeMultiplier(float lifeExpectancy)
        {
            float ratio = Mathf.Max(1f, lifeExpectancy / HumanLifeExpectancy);
            float bonus = Mathf.Log(ratio, 2f) * 0.2f;

            return Mathf.Clamp(1f + bonus, 1f, 1.8f);
        }

        public static float FertilityFactor(Pawn pawn)
        {
            if (pawn == null)
                return 0f;

            return pawn.gender == Gender.Male
                ? FertilityFactorMale(pawn)
                : FertilityFactorFemale(pawn);
        }

        public static float FertilityFactorFemale(Pawn pawn)
        {
            float age = BiologicalAge(pawn);
            float reproductive = FirstReproductiveAge(pawn);
            float maturity = BiologicalMaturityAge(pawn);
            float life = LifeExpectancy(pawn);

            if (age < reproductive)
            {
                Log.Message($"[AgeScale] FertilityFactorFemale pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p=n/a result=0.000");
                return 0f;
            }

            if (age < maturity)
            {
                float t = SmoothStep((age - reproductive) / Mathf.Max(1f, maturity - reproductive));
                float result = Mathf.Lerp(0.15f, 0.7f, t);
                Log.Message($"[AgeScale] FertilityFactorFemale pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p=growing result={result:F3}");
                return result;
            }

            float p = AdultProgress(pawn);
            float primeMult = LongevityPrimeMultiplier(life);

            float peakStart = 0.03f;
            float peakEnd = Mathf.Min(0.16f * primeMult, 0.32f);
            float declineStart = Mathf.Min(0.22f * primeMult, 0.40f);
            float infertile = Mathf.Min(0.85f, 0.55f * primeMult);

            if (p < peakStart)
            {
                float t = SmoothStep(p / peakStart);
                float result = Mathf.Lerp(0.7f, 1f, t);
                Log.Message($"[AgeScale] FertilityFactorFemale pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result={result:F3}");
                return result;
            }

            if (p < peakEnd)
            {
                Log.Message($"[AgeScale] FertilityFactorFemale pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result=1.000");
                return 1f;
            }

            if (p < declineStart)
            {
                float t = SmoothStep((p - peakEnd) / Mathf.Max(0.001f, declineStart - peakEnd));
                float result = Mathf.Lerp(1f, 0.9f, t);
                Log.Message($"[AgeScale] FertilityFactorFemale pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result={result:F3}");
                return result;
            }

            if (p < infertile)
            {
                float t = SmoothStep((p - declineStart) / Mathf.Max(0.001f, infertile - declineStart));
                float result = Mathf.Lerp(0.9f, 0f, t);
                Log.Message($"[AgeScale] FertilityFactorFemale pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result={result:F3}");
                return result;
            }

            Log.Message($"[AgeScale] FertilityFactorFemale pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result=0.000");
            return 0f;
        }

        public static float FertilityFactorMale(Pawn pawn)
        {
            float age = BiologicalAge(pawn);
            float reproductive = FirstReproductiveAge(pawn);
            float maturity = BiologicalMaturityAge(pawn);
            float life = LifeExpectancy(pawn);

            if (age < reproductive)
                return 0f;

            if (age < maturity)
            {
                float t = SmoothStep((age - reproductive) / Mathf.Max(1f, maturity - reproductive));
                return Mathf.Lerp(0.2f, 0.8f, t);
            }

            float p = AdultProgress(pawn);
            float primeMult = Mathf.Lerp(1f, LongevityPrimeMultiplier(life), 0.5f);

            float peakStart = 0.03f;
            float peakEnd = Mathf.Min(0.35f * primeMult, 0.55f);
            float declineStart = Mathf.Min(0.45f * primeMult, 0.65f);
            float infertile = 0.95f;

            if (p < peakStart)
            {
                float t = SmoothStep(p / peakStart);
                return Mathf.Lerp(0.8f, 1f, t);
            }

            if (p < peakEnd)
                return 1f;

            if (p < declineStart)
            {
                float t = SmoothStep((p - peakEnd) / Mathf.Max(0.001f, declineStart - peakEnd));
                return Mathf.Lerp(1f, 0.9f, t);
            }

            if (p < infertile)
            {
                float t = SmoothStep((p - declineStart) / Mathf.Max(0.001f, infertile - declineStart));
                return Mathf.Lerp(0.9f, 0.15f, t);
            }

            return 0.15f;
        }

        public static float RomanceAgeCompatibility(Pawn a, Pawn b)
        {
            if (a == null || b == null)
                return 0f;

            float scoreA = RomanceAgeScore(a);
            float scoreB = RomanceAgeScore(b);
            float diff = Mathf.Abs(scoreA - scoreB);

            float lifeA = LifeExpectancy(a);
            float lifeB = LifeExpectancy(b);
            float longA = LongLifeFactor(lifeA);
            float longB = LongLifeFactor(lifeB);
            float eitherLong = Mathf.Max(longA, longB);
            float bothLong = Mathf.Min(longA, longB);

            // If either side is long-lived, the same score gap should feel less severe.
            // If both are long-lived, loosen it even more.
            float leniency = 1f + 0.35f * eitherLong + 0.45f * bothLong;
            float effectiveDiff = diff / leniency;

            float compatibility;
            if (effectiveDiff <= 0.14f)
                compatibility = 1f;
            else if (effectiveDiff <= 0.34f)
                compatibility = Mathf.Lerp(1f, 0.82f, SmoothStep((effectiveDiff - 0.14f) / 0.20f));
            else if (effectiveDiff <= 0.62f)
                compatibility = Mathf.Lerp(0.82f, 0.42f, SmoothStep((effectiveDiff - 0.34f) / 0.28f));
            else
                compatibility = 0.22f;

            if (IsGrowingPhase(a) != IsGrowingPhase(b))
                compatibility *= Mathf.Lerp(0.55f, 0.80f, eitherLong);

            if (IsDecliningPhase(a) != IsDecliningPhase(b) && (IsDecliningPhase(a) || IsDecliningPhase(b)))
                compatibility *= Mathf.Lerp(0.70f, 0.90f, bothLong);

            // Keep a small floor so adult long-lived pairs are not overly suppressed.
            if (!IsGrowingPhase(a) && !IsGrowingPhase(b) && eitherLong > 0f)
            {
                float floor = Mathf.Lerp(0.12f, 0.30f, bothLong);
                compatibility = Mathf.Max(compatibility, floor);
            }

            return compatibility;
        }

        private static float RomanceAgeScore(Pawn pawn)
        {
            float age = BiologicalAge(pawn);
            float reproductive = FirstReproductiveAge(pawn);
            float maturity = BiologicalMaturityAge(pawn);
            float life = LifeExpectancy(pawn);

            if (age < reproductive)
            {
                Log.Message($"[AgeScale] RomanceAgeScore pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p=n/a result=0.000");
                return 0f;
            }

            if (age < maturity)
            {
                float t = SmoothStep((age - reproductive) / Mathf.Max(1f, maturity - reproductive));
                float result = Mathf.Lerp(0.35f, 0.68f, t);
                Log.Message($"[AgeScale] RomanceAgeScore pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p=growing result={result:F3}");
                return result;
            }

            float p = AdultProgress(pawn);
            float primeMult = LongevityPrimeMultiplier(life);
            float longLife = LongLifeFactor(life);

            float peakStart = 0.02f;
            float peakEnd = Mathf.Min(0.22f * Mathf.Pow(primeMult, 1.15f), 0.50f);
            float declineStart = Mathf.Min(0.45f * Mathf.Pow(primeMult, 1.2f), 0.78f);
            float endValue = Mathf.Lerp(0.48f, 0.62f, longLife);

            if (p < peakStart)
            {
                float t = SmoothStep(p / peakStart);
                float result = Mathf.Lerp(0.68f, 0.82f, t);
                Log.Message($"[AgeScale] RomanceAgeScore pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result={result:F3}");
                return result;
            }

            if (p < peakEnd)
            {
                float t = SmoothStep((p - peakStart) / Mathf.Max(0.001f, peakEnd - peakStart));
                float result = Mathf.Lerp(0.82f, 1f, t);
                Log.Message($"[AgeScale] RomanceAgeScore pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result={result:F3}");
                return result;
            }

            if (p < declineStart)
            {
                float t = SmoothStep((p - peakEnd) / Mathf.Max(0.001f, declineStart - peakEnd));
                float result = Mathf.Lerp(1f, 0.86f, t);
                Log.Message($"[AgeScale] RomanceAgeScore pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result={result:F3}");
                return result;
            }

            float tailT = SmoothStep((p - declineStart) / Mathf.Max(0.001f, 1f - declineStart));
            float tailResult = Mathf.Lerp(0.86f, endValue, tailT);
            Log.Message($"[AgeScale] RomanceAgeScore pawn={pawn?.LabelShortCap ?? "null"} age={age:F2} life={life:F2} p={p:F3} result={tailResult:F3}");
            return tailResult;

            /* float reproductive = FirstReproductiveAge(pawn);
            float maturity = BiologicalMaturityAge(pawn);
            float life = LifeExpectancy(pawn);
            float age = BiologicalAge(pawn);

            if (age < reproductive)
                return 0f;

            if (age < maturity)
            {
                float t = SmoothStep((age - reproductive) / Mathf.Max(1f, maturity - reproductive));
                return Mathf.Lerp(0.12f, 0.48f, t);
            }

            float p = AdultProgress(pawn);
            float primeMult = LongevityPrimeMultiplier(life);

            float peakStart = 0.03f;
            float peakEnd = Mathf.Min(0.16f * Mathf.Pow(primeMult, 1.36f), 0.36f);
            float declineStart = Mathf.Min(0.22f * Mathf.Pow(primeMult, 1.36f), 0.44f);

            if (p < peakStart)
                return Mathf.Lerp(0.48f, 0.60f, SmoothStep(p / peakStart));

            if (p < peakEnd)
                return 0.70f;

            if (p < declineStart)
                return Mathf.Lerp(0.70f, 0.82f, SmoothStep((p - peakEnd) / Mathf.Max(0.001f, declineStart - peakEnd)));

            return Mathf.Lerp(0.82f, 1f, SmoothStep((p - declineStart) / Mathf.Max(0.001f, 1f - declineStart)));
        */
        } 

        private static float LongLifeFactor(float lifeExpectancy)
        {
            // 0 at human expectancy (80), trending to 1 as races get much longer-lived.
            return Mathf.Clamp01((lifeExpectancy - HumanLifeExpectancy) / 320f);
        }

        private static bool IsGrowingPhase(Pawn pawn)
        {
            float age = BiologicalAge(pawn);
            float reproductive = FirstReproductiveAge(pawn);
            float maturity = BiologicalMaturityAge(pawn);

            return age >= reproductive && age < maturity;
        }

        private static bool IsDecliningPhase(Pawn pawn)
        {
            float age = BiologicalAge(pawn);
            float maturity = BiologicalMaturityAge(pawn);
            float life = LifeExpectancy(pawn);
            float p = AdultProgress(pawn);
            float primeMult = LongevityPrimeMultiplier(life);
            float declineStart = Mathf.Min(0.22f * primeMult, 0.40f);

            return age >= maturity && p >= declineStart;
        }

        public static float BirthQualityFactor(Pawn mother)
        {
            if (mother == null)
                return 1f;

            float age = BiologicalAge(mother);
            float reproductive = FirstReproductiveAge(mother);
            float maturity = BiologicalMaturityAge(mother);
            float life = LifeExpectancy(mother);

            if (age < reproductive)
                return 0.8f;

            if (age < maturity)
            {
                float t = SmoothStep((age - reproductive) / Mathf.Max(1f, maturity - reproductive));
                return Mathf.Lerp(0.85f, 0.96f, t);
            }

            float p = AdultProgress(mother);
            float primeMult = LongevityPrimeMultiplier(life);

            float primeStart = 0.03f;
            float primeEnd = Mathf.Min(0.14f * primeMult, 0.30f);
            float declineStart = Mathf.Min(0.24f * primeMult, 0.42f);
            float declineEnd = 0.55f;

            if (p < primeStart)
            {
                float t = SmoothStep(p / primeStart);
                return Mathf.Lerp(0.96f, 1f, t);
            }

            if (p < primeEnd)
                return 1f;

            if (p < declineStart)
            {
                float t = SmoothStep((p - primeEnd) / Mathf.Max(0.001f, declineStart - primeEnd));
                return Mathf.Lerp(1f, 0.94f, t);
            }

            if (p < declineEnd)
            {
                float t = SmoothStep((p - declineStart) / Mathf.Max(0.001f, declineEnd - declineStart));
                return Mathf.Lerp(0.94f, 0.7f, t);
            }

            return 0.7f;
        }
    }
}