using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CivilizationJourney.Dialogue
{
    /// <summary>
    /// 对话加载器
    /// 拖到空GameObject上，设置对话数据，运行即可自动生成UI并播放对话
    /// 参考视觉小说风格：全屏背景 + 大立绘 + 底部对话框
    /// </summary>
    public class DialogueLoader : MonoBehaviour
    {
        [Header("对话数据")]
        [Tooltip("将对话编辑器导出的对话数据拖到这里")]
        public DialogueData dialogueData;

        [Header("启动设置")]
        [Tooltip("是否在游戏开始时自动播放对话")]
        public bool playOnStart = true;

        [Tooltip("开始播放前的延迟时间（秒）")]
        public float startDelay = 0.5f;

        [Header("UI设置")]
        [Tooltip("是否自动创建对话UI（如果场景中没有的话）")]
        public bool autoCreateUI = true;

        [Tooltip("自定义字体（将TMP字体资源拖到这里）")]
        public TMP_FontAsset customFont;

        [Header("调试")]
        [Tooltip("显示调试信息")]
        public bool showDebugInfo = false;

        // 运行时引用
        private DialoguePlayer dialoguePlayer;
        private DialogueUI dialogueUI;
        private Canvas canvas;

        private void Start()
        {
            if (dialogueData == null)
            {
                Debug.LogError("DialogueLoader: 请设置对话数据！");
                return;
            }

            // 查找或创建UI
            SetupDialogueSystem();

            // 自动播放
            if (playOnStart)
            {
                if (startDelay > 0)
                {
                    Invoke(nameof(PlayDialogue), startDelay);
                }
                else
                {
                    PlayDialogue();
                }
            }
        }

        /// <summary>
        /// 设置对话系统
        /// </summary>
        private void SetupDialogueSystem()
        {
            // 查找现有的DialogueUI
            dialogueUI = FindObjectOfType<DialogueUI>();
            
            // 查找现有的DialoguePlayer
            dialoguePlayer = FindObjectOfType<DialoguePlayer>();

            // 如果没有UI且允许自动创建
            if (dialogueUI == null && autoCreateUI)
            {
                CreateDialogueUI();
            }

            // 如果没有Player，创建一个
            if (dialoguePlayer == null)
            {
                dialoguePlayer = gameObject.AddComponent<DialoguePlayer>();
            }

            // 设置引用
            if (dialoguePlayer != null && dialogueUI != null)
            {
                dialoguePlayer.SetDialogueUI(dialogueUI);
                dialoguePlayer.SetDialogueData(dialogueData);
            }

            if (showDebugInfo)
            {
                Debug.Log($"DialogueLoader: 对话系统初始化完成");
                Debug.Log($"  - 对话数据: {dialogueData.dialogueTitle}");
                Debug.Log($"  - 场景数: {dialogueData.scenes.Count}");
                Debug.Log($"  - 对话总数: {dialogueData.TotalLineCount}");
            }
        }

        /// <summary>
        /// 动态创建对话UI（视觉小说风格）
        /// </summary>
        private void CreateDialogueUI()
        {
            if (showDebugInfo)
            {
                Debug.Log("DialogueLoader: 自动创建对话UI...");
            }

            // 创建Canvas
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("DialogueCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();

                if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // === 创建对话UI根节点 ===
            GameObject dialogueRoot = new GameObject("DialogueUI");
            dialogueRoot.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = dialogueRoot.AddComponent<RectTransform>();
            SetFullStretch(rootRect);

            CanvasGroup canvasGroup = dialogueRoot.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            dialogueUI = dialogueRoot.AddComponent<DialogueUI>();

            // === 1. 全屏背景 ===
            GameObject background = CreateUIElement(dialogueRoot.transform, "Background");
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // 默认深色背景
            bgImage.preserveAspect = false;
            SetFullStretch(background.GetComponent<RectTransform>());

            // === 2. 立绘容器（放在对话框后面） ===
            GameObject portraitContainer = CreateUIElement(dialogueRoot.transform, "PortraitContainer");
            SetFullStretch(portraitContainer.GetComponent<RectTransform>());

            // 单个立绘（大立绘，半身像风格）
            GameObject portrait = CreateUIElement(portraitContainer.transform, "Portrait");
            Image portraitImg = portrait.AddComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;
            RectTransform portraitRect = portrait.GetComponent<RectTransform>();
            // 立绘锚点在底部中间，向上延伸
            portraitRect.anchorMin = new Vector2(0.5f, 0);
            portraitRect.anchorMax = new Vector2(0.5f, 0);
            portraitRect.pivot = new Vector2(0.5f, 0);
            portraitRect.anchoredPosition = new Vector2(300, 0); // 默认偏右
            portraitRect.sizeDelta = new Vector2(900, 1000); // 大立绘
            portrait.SetActive(false);

            // === 3. 底部对话框（半透明渐变） ===
            GameObject dialogueBox = CreateUIElement(dialogueRoot.transform, "DialogueBox");
            Image boxImage = dialogueBox.AddComponent<Image>();
            // 使用半透明黑色，可以看到背景
            boxImage.color = new Color(0, 0, 0, 0.75f);
            RectTransform boxRect = dialogueBox.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0, 0);
            boxRect.anchorMax = new Vector2(1, 0);
            boxRect.pivot = new Vector2(0.5f, 0);
            boxRect.anchoredPosition = Vector2.zero;
            boxRect.sizeDelta = new Vector2(0, 220); // 对话框高度

            // === 4. 角色名（居中显示在对话框上方） ===
            GameObject nameTextGO = CreateUIElement(dialogueBox.transform, "CharacterName");
            TextMeshProUGUI nameText = nameTextGO.AddComponent<TextMeshProUGUI>();
            nameText.text = "";
            nameText.fontSize = 32;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = new Color(1f, 0.9f, 0.7f); // 淡金色
            nameText.alignment = TextAlignmentOptions.Center;
            if (customFont != null) nameText.font = customFont;
            RectTransform nameRect = nameTextGO.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.pivot = new Vector2(0.5f, 0);
            nameRect.anchoredPosition = new Vector2(0, 15);
            nameRect.sizeDelta = new Vector2(400, 40);

            // === 5. 角色名下划线 ===
            GameObject underline = CreateUIElement(dialogueBox.transform, "NameUnderline");
            Image underlineImg = underline.AddComponent<Image>();
            underlineImg.color = new Color(1f, 0.9f, 0.7f, 0.5f);
            RectTransform underlineRect = underline.GetComponent<RectTransform>();
            underlineRect.anchorMin = new Vector2(0.5f, 1);
            underlineRect.anchorMax = new Vector2(0.5f, 1);
            underlineRect.pivot = new Vector2(0.5f, 1);
            underlineRect.anchoredPosition = new Vector2(0, 10);
            underlineRect.sizeDelta = new Vector2(300, 2);

            // === 6. 对话文本 ===
            GameObject dialogueTextGO = CreateUIElement(dialogueBox.transform, "DialogueText");
            TextMeshProUGUI dialogueText = dialogueTextGO.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "";
            dialogueText.fontSize = 30;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.Center;
            dialogueText.lineSpacing = 15;
            if (customFont != null) dialogueText.font = customFont;
            RectTransform dialogueTextRect = dialogueTextGO.GetComponent<RectTransform>();
            dialogueTextRect.anchorMin = new Vector2(0, 0);
            dialogueTextRect.anchorMax = new Vector2(1, 1);
            dialogueTextRect.offsetMin = new Vector2(100, 30);
            dialogueTextRect.offsetMax = new Vector2(-100, -70);

            // === 7. 继续提示（右下角小三角） ===
            GameObject continueIndicator = CreateUIElement(dialogueBox.transform, "ContinueIndicator");
            TextMeshProUGUI continueText = continueIndicator.AddComponent<TextMeshProUGUI>();
            continueText.text = "\u25BC"; // ▼
            continueText.fontSize = 20;
            continueText.color = new Color(1, 1, 1, 0.6f);
            continueText.alignment = TextAlignmentOptions.Center;
            RectTransform continueRect = continueIndicator.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(1, 0);
            continueRect.anchorMax = new Vector2(1, 0);
            continueRect.pivot = new Vector2(1, 0);
            continueRect.anchoredPosition = new Vector2(-30, 15);
            continueRect.sizeDelta = new Vector2(30, 30);

            // === 8. 右上角功能按钮 ===
            // 自动按钮
            GameObject autoButton = CreateButton(dialogueRoot.transform, "AutoButton", "自动 \u25B6", 
                new Vector2(-140, -20), new Vector2(110, 35));
            
            // 历史对话按钮
            GameObject historyButton = CreateButton(dialogueRoot.transform, "HistoryButton", "历史对话",
                new Vector2(-20, -20), new Vector2(110, 35));

            // 跳过按钮（放在第二行或更右边）
            GameObject skipButton = CreateButton(dialogueRoot.transform, "SkipButton", "跳过",
                new Vector2(-20, -60), new Vector2(110, 35));

            // === 设置DialogueUI的字段 ===
            SetPrivateField(dialogueUI, "canvasGroup", canvasGroup);
            SetPrivateField(dialogueUI, "dialoguePanel", dialogueRoot);
            SetPrivateField(dialogueUI, "backgroundImage", bgImage);
            SetPrivateField(dialogueUI, "portraitImage", portraitImg);
            SetPrivateField(dialogueUI, "portraitRect", portraitRect);
            SetPrivateField(dialogueUI, "dialogueBoxImage", boxImage);
            SetPrivateField(dialogueUI, "characterNameText", nameText);
            SetPrivateField(dialogueUI, "nameUnderline", underlineImg);
            SetPrivateField(dialogueUI, "dialogueText", dialogueText);
            SetPrivateField(dialogueUI, "continueIndicator", continueIndicator);
            SetPrivateField(dialogueUI, "autoButton", autoButton.GetComponent<Button>());
            SetPrivateField(dialogueUI, "historyButton", historyButton.GetComponent<Button>());
            SetPrivateField(dialogueUI, "skipButton", skipButton.GetComponent<Button>());

            if (showDebugInfo)
            {
                Debug.Log("DialogueLoader: 对话UI创建完成（视觉小说风格）");
            }
        }

        private GameObject CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            GameObject btnGO = CreateUIElement(parent, name);
            
            // 按钮背景（半透明）
            Image btnImage = btnGO.AddComponent<Image>();
            btnImage.color = new Color(0, 0, 0, 0.4f);
            
            Button btn = btnGO.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            btn.colors = colors;
            
            RectTransform btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(1, 1);
            btnRect.anchoredPosition = position;
            btnRect.sizeDelta = size;

            // 按钮文字
            GameObject textGO = CreateUIElement(btnGO.transform, "Text");
            TextMeshProUGUI btnText = textGO.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            if (customFont != null) btnText.font = customFont;
            SetFullStretch(textGO.GetComponent<RectTransform>());

            return btnGO;
        }

        /// <summary>
        /// 播放对话
        /// </summary>
        public void PlayDialogue()
        {
            if (dialoguePlayer != null && dialogueData != null)
            {
                dialoguePlayer.StartDialogue(dialogueData);
                
                if (showDebugInfo)
                {
                    Debug.Log($"DialogueLoader: 开始播放对话 - {dialogueData.dialogueTitle}");
                }
            }
        }

        /// <summary>
        /// 播放指定的对话数据
        /// </summary>
        public void PlayDialogue(DialogueData data)
        {
            dialogueData = data;
            if (dialoguePlayer != null)
            {
                dialoguePlayer.StartDialogue(data);
            }
        }

        /// <summary>
        /// 停止对话
        /// </summary>
        public void StopDialogue()
        {
            if (dialoguePlayer != null)
            {
                dialoguePlayer.EndDialogue();
            }
        }

        #region Helper Methods

        private GameObject CreateUIElement(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        #endregion

        #region Editor Helper

#if UNITY_EDITOR
        [ContextMenu("测试播放对话")]
        private void TestPlay()
        {
            if (Application.isPlaying)
            {
                PlayDialogue();
            }
            else
            {
                Debug.Log("请先运行游戏再测试播放");
            }
        }
#endif

        #endregion
    }
}
