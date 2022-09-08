using System.Collections.Generic;
using UnityEngine;

namespace Skills
{
    public delegate void SkillEffect(SkillData skill);

    public static class SkillEffects
    {
        public static Dictionary<SkillCode, SkillEffect> EFFECTS =
            new Dictionary<SkillCode, SkillEffect>()
            {
                { SkillCode.PowerStrike, PowerStrike },
            };

        public static void PowerStrike(SkillData skill)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Player.PlayerController pc = player.GetComponent<Player.PlayerController>();
            pc.TriggerState("PowerStrike", () =>
            {
                Player.PlayerController.overrideDamage = -1;
            });
            
            pc.ResetAttackCombo();

            if (skill.fxPrefab != null)
                GameObject.Instantiate(skill.fxPrefab, player.transform);
            Player.PlayerController.overrideDamage = skill.damage;
        }

    }

}
