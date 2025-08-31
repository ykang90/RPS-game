using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CARVES.Utls
{
    /// <summary>
    /// 将 Unity 日志实时输出到屏幕上的轻量级控制台。<br/>
    /// 1. 将本脚本挂在一个GameObject 上；<br/>
    /// 2. 调整 fontSize / 宽高 / 锚点，即可在运行时查看日志；<br/>
    /// 3. 支持环形缓冲/自动换色/自动滚动。<br/>
    /// </summary>
    public class SceneConsole : MonoBehaviour
    {
        [Header("Display")] [SerializeField, LabelText("最大行数")]
        int maxLines = 200; // 环形缓冲大小

        [SerializeField, LabelText("合并重复日志")] bool collapse = true; // 合并重复日志
        [SerializeField, LabelText("自动滚动")] bool autoScroll = true;
        [SerializeField] ScrollRect _scroll; // 可以配合ScrollRect
        [SerializeField] TMP_Text _text;

        readonly Queue<string> lines = new();
        string lastRaw;

        void OnEnable() => Application.logMessageReceived += Handle;
        void OnDisable() => Application.logMessageReceived -= Handle;

        void Handle(string logString, string stackTrace, LogType type)
        {
            if (collapse && logString == lastRaw) return;
            lastRaw = logString;

            var color = type switch
            {
                LogType.Error or LogType.Exception => "#FF5555",
                LogType.Warning => "#FFCC00",
                _ => "#FFFFFF"
            };
            lines.Enqueue($"<color={color}>{logString}</color>");
            while (lines.Count > maxLines) lines.Dequeue();

            var sb = new System.Text.StringBuilder();
            foreach (var l in lines) sb.AppendLine(l);
            _text.text = sb.ToString();

            if (autoScroll && _scroll) StartCoroutine(ScrollToBottom());
        }

        IEnumerator ScrollToBottom()
        {
            yield return null; // 等布局刷新
            _scroll.verticalNormalizedPosition = 0f; // 0 = 最底部
        }
    }
}