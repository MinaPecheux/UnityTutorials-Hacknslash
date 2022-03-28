using UnityEngine;

namespace Enemy
{

    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;
        private float _hp = 6;

        private void Awake()
        {
            _hp = _data.healthpoints;
        }

        public void TakeHit(float amount)
        {
            _hp -= amount;
            if (_hp <= 0)
                Destroy(gameObject);
        }

    }

}