using UnityEngine;

public enum EnemyBehaviour
{
    Aggressive,     // Prioritizes high damage moves
    Defensive,      // Prioritizes buffs and survival
    Disruptor,      // Prioritizes debuffs and status effects
    Random          // Picks moves at random
}

public class EnemyUnit : Unit
{
    [Header("Enemy Info")]
    public int tier;                    // Enemy power tier e.g. 1 = weak, 5 = boss
    public EnemyBehaviour behaviour;

    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;

    protected override void Awake()
    {
        base.Awake();
    }

    public Move ChooseMove(Unit target)
    {
        if (moveList == null || moveList.Length == 0) return null;

        return behaviour switch
        {
            EnemyBehaviour.Aggressive => ChooseAggressive(),
            EnemyBehaviour.Defensive  => ChooseDefensive(),
            EnemyBehaviour.Disruptor  => ChooseDisruptor(),
            EnemyBehaviour.Random     => moveList[Random.Range(0, moveList.Length)],
            _                         => moveList[Random.Range(0, moveList.Length)]
        };
    }

    private Move ChooseAggressive()
    {
        Move best = moveList[0];
        foreach (Move m in moveList)
            if (m.moveType == MoveType.Attack && m.baseDamage > best.baseDamage)
                best = m;
        return best;
    }

    private Move ChooseDefensive()
    {
        foreach (Move m in moveList)
            if (m.moveType == MoveType.Buff || m.moveType == MoveType.Heal)
                return m;
        return moveList[Random.Range(0, moveList.Length)];
    }

    private Move ChooseDisruptor()
    {
        foreach (Move m in moveList)
            if (m.moveType == MoveType.Debuff && m.effectChance > 0)
                return m;
        return moveList[Random.Range(0, moveList.Length)];
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        // Hook: trigger enemy death animation, drop loot, etc.
    }
}
