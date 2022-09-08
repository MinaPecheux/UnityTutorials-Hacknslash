using System.Threading.Tasks;
using UnityEngine;

namespace Skills
{
    public enum SkillType
    {
        Damage,
        Heal,
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
    public class SkillData : ScriptableObject
    {
        public SkillCode code;
        public string displayName;
        public SkillType type;
        public float cooldown;
        public float damage;
        public Sprite icon;
        public GameObject fxPrefab;

        private bool _inCooldown;

        private void Awake()
        {
            _inCooldown = false;
        }

        public bool Cast()
        {
            if (_inCooldown) return false;
            SkillEffects.EFFECTS[code](this);

            _inCooldown = true;
            _Resetting();
            return true;
        }

        private async void _Resetting()
        {
            await Task.Delay((int)(cooldown * 1000));
            _inCooldown = false;
        }
    }

}
