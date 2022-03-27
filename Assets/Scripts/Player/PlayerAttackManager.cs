using UnityEngine;

namespace Player
{

    public class PlayerAttackManager : MonoBehaviour
    {
        private static readonly int _ENEMY_LAYER = 1 << 6;

        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;

        private float _attackDamage = 2f;
        private float _attackRange = 0.25f;

        public void PlayerHitLeft(int comboStep)
        {
            _PlayerHit(comboStep, _leftHand);
        }

        public void PlayerHitRight(int comboStep)
        {
            _PlayerHit(comboStep, _rightHand);
        }

        private void _PlayerHit(int comboStep, Transform hand)
        {
            Collider[] closeEnemies = Physics.OverlapSphere(
                hand.position, _attackRange, _ENEMY_LAYER);
            foreach (Collider enemy in closeEnemies)
            {
                Enemy.EnemyManager em = enemy.GetComponent<Enemy.EnemyManager>();
                if (em != null)
                    em.TakeHit(_attackDamage * (comboStep + 1));
            }
        }
    }

}
