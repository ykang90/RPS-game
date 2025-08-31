using UnityEngine;

namespace CARVES.Utls
{
    static class AnimatorExtension
    {
        /// <summary>
        /// 播放动作的标准方法, 通过设置triggerInt和triggerOn触发动画
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="action"></param>
        public static void PlayAnim(this Animator animator, int action)
        {
            animator.SetInteger("triggerInt", action);
            animator.SetTrigger("triggerOn");
        }
    }
}