using UnityEngine;

namespace Enemy
{

    [CreateAssetMenu(
        fileName = "Enemy",
        menuName = "Scriptable Objects/Enemy")]
    public class EnemyData : ScriptableObject
    {
        public float healthpoints;
    }

}