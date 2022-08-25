using System.Collections.Generic;
using UnityEngine;

namespace Player
{

    public enum PlayerStatistic
    {
        Strength,
        Constitution,
        Dexterity,
    }

    [System.Serializable]
    public class PlayerStatisticValue
    {
        public PlayerStatistic stat;
        public int value;

        public static int GetByStat(PlayerStatisticValue[] stats, PlayerStatistic stat)
        {
            foreach (PlayerStatisticValue v in stats)
            {
                if (v.stat == stat)
                    return v.value;
            }
            return 0;
        }
    }

    [CreateAssetMenu(fileName = "Player",
                     menuName = "Scriptable Objects/Player")]
    public class PlayerData : ScriptableObject
    {

        public float moveSpeed;

        public float baseAttackRange;
        [HideInInspector] public float currentAttackRange;

        public int inventoryMaxWeight = 300;

        public bool overburdened = false;

        public PlayerStatisticValue[] statistics;

        public float AttackDamage
        {
            get
            {
                float d = 0f;
                foreach (PlayerStatisticValue v in statistics)
                {
                    if (v.stat == PlayerStatistic.Strength)
                        d += v.value * 0.7f;
                    if (v.stat == PlayerStatistic.Constitution)
                        d += v.value * 0.2f;
                    if (v.stat == PlayerStatistic.Dexterity)
                        d += v.value * 0.2f;
                }
                return d;
            }
        }

        public int GetStatistic(PlayerStatistic stat)
        {
            foreach (PlayerStatisticValue v in statistics)
            {
                if (v.stat == stat)
                    return v.value;
            }
            return 0;
        }

        public void ApplyStatisticModifiers(PlayerStatisticValue[] modifiers, bool on)
        {
            foreach (PlayerStatisticValue m in modifiers)
            {
                foreach (PlayerStatisticValue v in statistics)
                {
                    if (m.stat == v.stat)
                        v.value += m.value * (on ? 1 : -1);
                }
            }
        }

    }

}
