using System.Collections.Generic;
using UnityEngine;

namespace Skills
{
    public delegate void SkillEffect();

    public static class SkillEffects
    {
        public static Dictionary<SkillCode, SkillEffect> EFFECTS =
            new Dictionary<SkillCode, SkillEffect>()
            {
                { SkillCode.PowerStrike, PowerStrike },
            };

        public static void PowerStrike()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Player.PlayerController pc = player.GetComponent<Player.PlayerController>();
            pc.TriggerState("PowerStrike", () =>
            {
                Player.PlayerController.overrideDamage = -1f;
            });

            pc.ResetAttackCombo();

            Player.PlayerController.overrideDamage = 10000f;
        }

    }

}
