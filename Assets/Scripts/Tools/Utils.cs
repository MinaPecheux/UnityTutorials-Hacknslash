using System.Collections;
using UnityEngine;

namespace Tools
{

    public static class Utils
    {

        public static IEnumerator WaitingForCurrentAnimation(
            Animator animator,
            System.Action callback,
            float earlyExit = 0f,
            string waitForAnimName = null,
            float extraWait = 0f,
            bool stopAfterAnim = false)
        {
            if (stopAfterAnim)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(
                    animator.GetAnimatorTransitionInfo(0).duration);
                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() =>
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            }
            else if (waitForAnimName == null)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(
                    animator.GetAnimatorTransitionInfo(0).duration);
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(
                    animator.GetCurrentAnimatorStateInfo(0).length - earlyExit);
            }
            else
            {
                yield return new WaitUntil(() =>
                    animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == waitForAnimName);
                yield return new WaitForSeconds(
                    animator.GetCurrentAnimatorStateInfo(0).length);
            }
            if (extraWait > 0)
                yield return new WaitForSeconds(extraWait);
            callback();
        }

    }

}
