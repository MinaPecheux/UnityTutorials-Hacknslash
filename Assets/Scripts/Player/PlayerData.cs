using UnityEngine;

namespace Player
{

    [CreateAssetMenu(fileName = "Player",
                     menuName = "Scriptable Objects/Player")]
    public class PlayerData : ScriptableObject
    {

        public float moveSpeed;

        public float attackRange;
        public float attackDamage;

        public int inventoryMaxWeight = 300;

    }

}
