using UnityEngine;

namespace Enemy
{

    public class EnemyManager : MonoBehaviour
    {
        private float _hp = 6;

        public void TakeHit(float amount)
        {
            _hp -= amount;
            if (_hp <= 0)
                Destroy(gameObject);
        }

    }

}