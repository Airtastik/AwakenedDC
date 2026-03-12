public static class ElementalChart
{
    // [attacker, defender] = damage multiplier
    private static readonly float[,] chart =
    {
        //             Normal  Fire    Water   Nature  Absurd
        /* Normal */  { 1.0f,  1.0f,   1.0f,   1.0f,   0.5f  }, // Normal does half damage to Absurd, but is unaffected by other types
        /* Fire   */  { 1.0f,  1.0f,   0.5f,   2.0f,   1.0f  }, // Fire does half damage to Water, double damage to Nature, and is unaffected by Normal and Absurd
        /* Water  */  { 1.0f,  2.0f,   1.0f,   0.5f,   1.0f  }, // Water does double damage to Fire, half damage to Nature, and is unaffected by Normal and Absurd
        /* Nature */  { 1.0f,  0.5f,   2.0f,   1.0f,   1.0f  }, // Nature does half damage to Fire, double damage to Water, and is unaffected by Normal and Absurd
        /* Absurd */  { 0.5f,  1.0f,   1.0f,   1.0f,   1.0f  }, // Absurd does half damage to Normal, but is unaffected by other types
    };

    public static float GetMultiplier(ElementalType attacker, ElementalType defender)
    {
        return chart[(int)attacker, (int)defender];
    }
}
