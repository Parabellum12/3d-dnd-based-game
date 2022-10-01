using UnityEngine;

public static class ProbabilityUtilities
{
    public static bool RandomlyPicked(float randomValue, float treshold, float chance)
    {
        return randomValue >= treshold && randomValue <= chance + treshold;
    }
    public static bool RunPercentageRandom(float chance)
    {
        var rdm = Random.Range(0, 100f);
        return rdm <= chance;
    }
}
