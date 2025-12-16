using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CivilizationJourney.Dialogue
{
    /// <summary>
    /// 对话加载器
    /// 拖到空GameObject上，设置对话数据，运行即可自动生成UI并播放对话
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
        /// 动态创建对话UI
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
                canvas.sortingOrder = 100; // 确保在最上层

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGO.AddComponent<GraphicRaycaster>();

                // 创建EventSystem
                if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // 创建对话UI根节点
            GameObject dialogueRoot = new GameObject("DialogueUI");
            dialogueRoot.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = dialogueRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 添加CanvasGroup用于淡入淡出
            CanvasGroup canvasGroup = dialogueRoot.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            // 添加DialogueUI组件
            dialogueUI = dialogueRoot.AddComponent<DialogueUI>();

            // 创建背景遮罩
            GameObject background = CreateUIElement(dialogueRoot.transform, "Background");
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);
            SetFullStretch(background.GetComponent<RectTransform>());

            // 创建左立绘
            GameObject leftPortrait = CreateUIElement(dialogueRoot.transform, "LeftPortrait");
            Image leftImg = leftPortrait.AddComponent<Image>();
            leftImg.color = Color.white;
            leftImg.preserveAspect = true;
            RectTransform leftRect = leftPortrait.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0.15f);
            leftRect.anchorMax = new Vector2(0.35f, 0.95f);
            leftRect.offsetMin = new Vector2(30, 0);
            leftRect.offsetMax = new Vector2(0, -50);
            leftPortrait.SetActive(false);

            // 创建右立绘
            GameObject rightPortrait = CreateUIElement(dialogueRoot.transform, "RightPortrait");
            Image rightImg = rightPortrait.AddComponent<Image>();
            rightImg.color = Color.white;
            rightImg.preserveAspect = true;
            RectTransform rightRect = rightPortrait.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.65f, 0.15f);
            rightRect.anchorMax = new Vector2(1, 0.95f);
            rightRect.offsetMin = new Vector2(0, 0);
            rightRect.offsetMax = new Vector2(-30, -50);
            rightPortrait.SetActive(false);

            // 创建中间立绘
            GameObject centerPortrait = CreateUIElement(dialogueRoot.transform, "CenterPortrait");
            Image centerImg = centerPortrait.AddComponent<Image>();
            centerImg.color = Color.white;
            centerImg.preserveAspect = true;
            RectTransform centerRect = centerPortrait.GetComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.25f, 0.15f);
            centerRect.anchorMax = new Vector2(0.75f, 0.95f);
            centerRect.offsetMin = Vector2.zero;
            centerRect.offsetMax = new Vector2(0, -50);
            centerPortrait.SetActive(false);

            // 创建对话框背景
            GameObject dialogueBox = CreateUIElement(dialogueRoot.transform, "DialogueBox");
            Image boxImage = dialogueBox.AddComponent<Image>();
            boxImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            RectTransform boxRect = dialogueBox.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.05f, 0.02f);
            boxRect.anchorMax = new Vector2(0.95f, 0.25f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;

            // 创建角色名背景
            GameObject nameBox = CreateUIElement(dialogueBox.transform, "NameBox");
            Image nameBoxImage = nameBox.AddComponent<Image>();
            nameBoxImage.color = new Color(0.2f, 0.3f, 0.5f, 1f);
            RectTransform nameBoxRect = nameBox.GetComponent<RectTransform>();
            nameBoxRect.anchorMin = new Vector2(0, 1);
            nameBoxRect.anchorMax = new Vector2(0, 1);
            nameBoxRect.pivot = new Vector2(0, 0);
            nameBoxRect.anchoredPosition = new Vector2(20, 10);
            nameBoxRect.sizeDelta = new Vector2(250, 45);

            // 创建角色名文本
            GameObject nameTextGO = CreateUIElement(nameBox.transform, "NameText");
            TextMeshProUGUI nameText = nameTextGO.AddComponent<TextMeshProUGUI>();
            nameText.text = "角色名";
            nameText.fontSize = 28;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            SetFullStretch(nameTextGO.GetComponent<RectTransform>());
            nameTextGO.GetComponent<RectTransform>().offsetMin = new Vector2(10, 5);
            nameTextGO.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -5);

            // 创建对话文本
            GameObject dialogueTextGO = CreateUIElement(dialogueBox.transform, "DialogueText");
            TextMeshProUGUI dialogueText = dialogueTextGO.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "";
            dialogueText.fontSize = 26;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            dialogueText.lineSpacing = 10;
            RectTransform dialogueTextRect = dialogueTextGO.GetComponent<RectTransform>();
            dialogueTextRect.anchorMin = Vector2.zero;
            dialogueTextRect.anchorMax = Vector2.one;
            dialogueTextRect.offsetMin = new Vector2(30, 20);
            dialogueTextRect.offsetMax = new Vector2(-30, -15);

            // 创建点击继续提示
            GameObject continueIndicator = CreateUIElement(dialogueBox.transform, "ContinueIndicator");
            TextMeshProUGUI continueText = continueIndicator.AddComponent<TextMeshProUGUI>();
            continueText.text = "▼ 点击继续";
            continueText.fontSize = 18;
            continueText.color = new Color(1, 1, 1, 0.7f);
            continueText.alignment = TextAlignmentOptions.Right;
            RectTransform continueRect = continueIndicator.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.7f, 0);
            continueRect.anchorMax = new Vector2(1, 0);
            continueRect.pivot = new Vector2(1, 0);
            continueRect.anchoredPosition = new Vector2(-20, 10);
            continueRect.sizeDelta = new Vector2(150, 30);

            // 创建跳过按钮
            GameObject skipButton = CreateUIElement(dialogueRoot.transform, "SkipButton");
            Image skipBtnImage = skipButton.AddComponent<Image>();
            skipBtnImage.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);
            Button skipBtn = skipButton.AddComponent<Button>();
            ColorBlock colors = skipBtn.colors;
            colors.highlightedColor = new Color(0.5f, 0.5f, 0.6f, 0.9f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            skipBtn.colors = colors;
            RectTransform skipRect = skipButton.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1, 1);
            skipRect.anchorMax = new Vector2(1, 1);
            skipRect.pivot = new Vector2(1, 1);
            skipRect.anchoredPosition = new Vector2(-20, -20);
            skipRect.sizeDelta = new Vector2(100, 40);

            GameObject skipTextGO = CreateUIElement(skipButton.transform, "Text");
            TextMeshProUGUI skipText = skipTextGO.AddComponent<TextMeshProUGUI>();
            skipText.text = "跳过";
            skipText.fontSize = 22;
            skipText.color = Color.white;
            skipText.alignment = TextAlignmentOptions.Center;
            SetFullStretch(skipTextGO.GetComponent<RectTransform>());

            // 通过反射设置DialogueUI的字段
            SetPrivateField(dialogueUI, "canvasGroup", canvasGroup);
            SetPrivateField(dialogueUI, "dialoguePanel", dialogueRoot);
            SetPrivateField(dialogueUI, "backgroundImage", bgImage);
            SetPrivateField(dialogueUI, "leftPortrait", leftImg);
            SetPrivateField(dialogueUI, "rightPortrait", rightImg);
            SetPrivateField(dialogueUI, "centerPortrait", centerImg);
            SetPrivateField(dialogueUI, "dialogueBox", dialogueBox);
            SetPrivateField(dialogueUI, "characterNameText", nameText);
            SetPrivateField(dialogueUI, "dialogueText", dialogueText);
            SetPrivateField(dialogueUI, "continueIndicator", continueIndicator);
            SetPrivateField(dialogueUI, "skipButton", skipBtn);

            if (showDebugInfo)
            {
                Debug.Log("DialogueLoader: 对话UI创建完成");
            }
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
