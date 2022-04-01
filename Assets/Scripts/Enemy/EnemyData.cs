using UnityEngine;

namespace Enemy
{

    [CreateAssetMenu(
        fileName = "Enemy",
        menuName = "Scriptable Objects/Enemy")]
    public class EnemyData : ScriptableObject
    {
        public float healthpoints;
        public float moveSpeed;

        public float fovRadius;

        public float attackRadius;
        public float attackRate;
    }

}