using UnityEngine;
using Cysharp.Threading.Tasks;

public static class AnimatorExtensions
{
    /// <summary>
    /// 播放动画并等待动画结束
    /// </summary>
    public static async UniTask PlayAndWaitAsync(this Animator animator, string stateName, int layer = 0)
    {
        // 如果当前已经在播放这个动画，则直接返回
        var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        if (stateInfo.IsName(stateName))
            return;

        // 播放动画
        animator.Play(stateName, layer, 0f);

        // 等一帧，确保 Play 生效
        await UniTask.DelayFrame(1);

        // 循环等待动画播放完毕
        while (animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName) &&
               animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f)
        {
            await UniTask.DelayFrame(1);
        }
    }
}
