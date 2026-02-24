public static class ElementalChart
{
    // [attacker, defender] = damage multiplier
    private static readonly float[,] chart =
    {
        //             Normal  Fire    Water   Nature  Absurd
        /* Normal */  { 1.0f,  1.0f,   1.0f,   1.0f,   1.0f  },
        /* Fire   */  { 1.0f,  0.5f,   0.5f,   2.0f,   1.0f  },
        /* Water  */  { 1.0f,  2.0f,   0.5f,   0.5f,   1.0f  },
        /* Nature */  { 1.0f,  0.5f,   2.0f,   0.5f,   1.0f  },
        /* Absurd */  { 1.5f,  1.5f,   1.5f,   1.5f,   1.5f  }, // Absurd is unpredictable
    };

    public static float GetMultiplier(ElementalType attacker, ElementalType defender)
    {
        return chart[(int)attacker, (int)defender];
    }
}
