using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CARVES.Utls
{
    /// <summary>
    /// 更新策略, 针对Coroutine来实现的优化策略，<br/>
    /// 避免所有的触发器都在每一帧都检测，而是在一定的时间间隔内检测。
    /// </summary>
    [Serializable] public class UpdateStrategy
    {
        public enum Detection
        {
            [InspectorName("秒数")]TimeInterval,
            [InspectorName("跳帧")]FrameInterval,
            [InspectorName("逐帧")]EveryFrame,
        }
        [LabelText("频率")]public Detection detection;
        [ShowIf(nameof(detection),Detection.FrameInterval),LabelText("帧间隔")]public int frameInterval = 10;
        [ShowIf(nameof(detection), Detection.TimeInterval), LabelText("秒间隔")] public float timeInterval = 0.1f;
        float temp;
        public (Detection,float) GetDetection() => (detection, detection switch
        {
            Detection.FrameInterval => frameInterval,
            Detection.TimeInterval => timeInterval,
            _ => 0
        });
        /// <summary>
        /// 每帧执行，检查更新是否满足条件, 默认不检查时间间隔, <br/>
        /// 如果需要检查时间间隔<see cref="OnTimeIntervalRoutine"/>就不要调用
        /// </summary>
        /// <returns></returns>
        public bool UpdateFrameCheck(bool checkWithTimeInterval = false)
        {
            switch (detection)
            {
                case Detection.EveryFrame:
                    return true;
                case Detection.FrameInterval:
                    temp++;
                    if (temp >= frameInterval)
                    {
                        temp = 0;
                        return true;
                    }
                    return false;
                case Detection.TimeInterval:
                    if (!checkWithTimeInterval) return false;
                    temp += Time.deltaTime;
                    if (temp >= timeInterval)
                    {
                        temp = 0;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
        public IEnumerator OnTimeIntervalRoutine(Func<bool> callback,Action onFinalizeAction)
        {
            var isFinalized = false;
            while (!isFinalized)
            {
                yield return new WaitForSeconds(timeInterval);
                isFinalized = callback.Invoke();
            }
            onFinalizeAction?.Invoke();
        }
    }
}