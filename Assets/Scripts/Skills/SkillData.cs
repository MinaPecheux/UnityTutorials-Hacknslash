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
        public Sprite icon;

        public void Cast()
        {
            SkillEffects.EFFECTS[code]();
        }
    }

}
