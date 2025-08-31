using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CARVES.Core
{
    public class ExceptionHandler : MonoBehaviour
    {
        static ExceptionHandler _instance;
        public static ExceptionHandler Instance => _instance;
#if UNITY_EDITOR
        public bool showInExceptionEditor;
#endif

        public bool Persist;
        public bool PauseOnError = true;
        public TMP_FontAsset Font;
        public Button TestExceptionButton;
        public event UnityAction<string> OnError;
        public event UnityAction<string> OnInfo;
        public event UnityAction<string> OnWarning;
        Text Ui;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Persist) DontDestroyOnLoad(this.gameObject);
                Application.logMessageReceived += HandleLog;
                if(TestExceptionButton)
                    TestExceptionButton.onClick.AddListener(()=>TestException());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                Application.logMessageReceived -= HandleLog;
            }
        }
        [Button]public void TestException(string errorMessage = "Test exception!") => throw new Exception(errorMessage);
        void HandleLog(string condition, string stacktrace, LogType type)
        {
#if UNITY_EDITOR
            if (!showInExceptionEditor) return;
#endif
            var msg = $"{type} - {condition}\n{stacktrace}";

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    if (OnError == null)
                        ShowErrorMessage(msg);
                    else OnError?.Invoke(msg);

                    if (PauseOnError)
                    {
                        PauseGame();
                    }

                    break;
                case LogType.Warning:
                    OnWarning?.Invoke(msg);
                    break;
                case LogType.Log:
                    OnInfo?.Invoke(msg);
                    break;
                default:
                    Debug.LogWarning($"未处理的日志类型：{type}");
                    break;
            }
        }

        void PauseGame()
        {
            Time.timeScale = 0;
            // 如果需要，可以禁用其他系统，如输入、AI 等
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            // 恢复其他系统
        }

        void ShowErrorMessage(string message)
        {
            if (Ui) Ui.text = message;
            else CreateUi(message);
        }
        void CreateUi(string message)
        {
            GameObject errorPopup = new GameObject("ErrorPopup");
            var canvas = errorPopup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var panel = errorPopup.AddComponent<Image>();
            panel.color = new Color(0, 0, 0, 0.8f);
            var textObj = new GameObject("ErrorText");
            textObj.transform.SetParent(errorPopup.transform);
            var text = textObj.AddComponent<Text>();
            text.text = message;
            text.font = Font.sourceFontFile;
            text.color = Color.red;
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            var outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;

            var rectTransform = text.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(600, 200);
            rectTransform.anchoredPosition = Vector2.zero;
        }

    }
}