using UnityEngine;

namespace Player
{

    [CreateAssetMenu(fileName = "Player",
                     menuName = "Scriptable Objects/Player")]
    public class PlayerData : ScriptableObject
    {

        public float moveSpeed;

        public float attackRange;

        public int inventoryMaxWeight = 300;

        public bool overburdened = false;

        public int strength = 10;
        public int constitution = 10;
        public int dexterity = 10;

        public float AttackDamage
            => strength * 0.7f + constitution * 0.2f + dexterity * 0.2f;

    }

}
