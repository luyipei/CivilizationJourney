using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace CivilizationJourney.Dialogue.Editor
{
    /// <summary>
    /// 对话UI快速创建工具
    /// 一键创建完整的对话UI结构
    /// </summary>
    public class DialogueUICreator : EditorWindow
    {
        [MenuItem("文明之旅/创建对话UI", false, 200)]
        public static void CreateDialogueUI()
        {
            // 检查是否已有Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            Debug.Log("123");
            if (canvas == null)
            {
                // 创建Canvas
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
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

            // 添加CanvasGroup
            CanvasGroup canvasGroup = dialogueRoot.AddComponent<CanvasGroup>();

            // 添加DialogueUI组件
            DialogueUI dialogueUI = dialogueRoot.AddComponent<DialogueUI>();

            // 创建背景
            GameObject background = CreateImage(dialogueRoot.transform, "Background", new Color(0, 0, 0, 0.8f));
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 创建左立绘
            GameObject leftPortrait = CreateImage(dialogueRoot.transform, "LeftPortrait", Color.white);
            RectTransform leftRect = leftPortrait.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0.3f, 0.85f);
            leftRect.offsetMin = new Vector2(50, 100);
            leftRect.offsetMax = new Vector2(0, 0);
            leftPortrait.SetActive(false);

            // 创建右立绘
            GameObject rightPortrait = CreateImage(dialogueRoot.transform, "RightPortrait", Color.white);
            RectTransform rightRect = rightPortrait.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.7f, 0);
            rightRect.anchorMax = new Vector2(1, 0.85f);
            rightRect.offsetMin = new Vector2(0, 100);
            rightRect.offsetMax = new Vector2(-50, 0);
            rightPortrait.SetActive(false);

            // 创建中间立绘
            GameObject centerPortrait = CreateImage(dialogueRoot.transform, "CenterPortrait", Color.white);
            RectTransform centerRect = centerPortrait.GetComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.3f, 0);
            centerRect.anchorMax = new Vector2(0.7f, 0.85f);
            centerRect.offsetMin = new Vector2(0, 100);
            centerRect.offsetMax = new Vector2(0, 0);
            centerPortrait.SetActive(false);

            // 创建对话框
            GameObject dialogueBox = CreateImage(dialogueRoot.transform, "DialogueBox", new Color(0, 0, 0, 0.9f));
            RectTransform boxRect = dialogueBox.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.05f, 0.02f);
            boxRect.anchorMax = new Vector2(0.95f, 0.28f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;

            // 创建角色名背景
            GameObject nameBox = CreateImage(dialogueBox.transform, "NameBox", new Color(0.2f, 0.2f, 0.3f, 1f));
            RectTransform nameBoxRect = nameBox.GetComponent<RectTransform>();
            nameBoxRect.anchorMin = new Vector2(0, 1);
            nameBoxRect.anchorMax = new Vector2(0.25f, 1);
            nameBoxRect.pivot = new Vector2(0, 0);
            nameBoxRect.anchoredPosition = new Vector2(20, 5);
            nameBoxRect.sizeDelta = new Vector2(200, 40);

            // 创建角色名文本
            GameObject nameText = CreateText(nameBox.transform, "CharacterName", "角色名", 24, Color.white);
            RectTransform nameTextRect = nameText.GetComponent<RectTransform>();
            nameTextRect.anchorMin = Vector2.zero;
            nameTextRect.anchorMax = Vector2.one;
            nameTextRect.offsetMin = new Vector2(15, 5);
            nameTextRect.offsetMax = new Vector2(-15, -5);

            // 创建对话文本
            GameObject dialogueTextGO = CreateText(dialogueBox.transform, "DialogueText", "这里是对话内容...", 22, Color.white);
            RectTransform dialogueTextRect = dialogueTextGO.GetComponent<RectTransform>();
            dialogueTextRect.anchorMin = new Vector2(0, 0);
            dialogueTextRect.anchorMax = new Vector2(1, 1);
            dialogueTextRect.offsetMin = new Vector2(30, 20);
            dialogueTextRect.offsetMax = new Vector2(-30, -20);
            TextMeshProUGUI tmpText = dialogueTextGO.GetComponent<TextMeshProUGUI>();
            tmpText.alignment = TextAlignmentOptions.TopLeft;

            // 创建继续提示
            GameObject continueIndicator = CreateText(dialogueBox.transform, "ContinueIndicator", "▼", 18, Color.white);
            RectTransform continueRect = continueIndicator.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(1, 0);
            continueRect.anchorMax = new Vector2(1, 0);
            continueRect.pivot = new Vector2(1, 0);
            continueRect.anchoredPosition = new Vector2(-20, 15);
            continueRect.sizeDelta = new Vector2(30, 30);

            // 创建跳过按钮
            GameObject skipButton = CreateButton(dialogueRoot.transform, "SkipButton", "跳过", new Vector2(100, 40));
            RectTransform skipRect = skipButton.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1, 1);
            skipRect.anchorMax = new Vector2(1, 1);
            skipRect.pivot = new Vector2(1, 1);
            skipRect.anchoredPosition = new Vector2(-20, -20);

            // 设置DialogueUI的引用
            SerializedObject serializedUI = new SerializedObject(dialogueUI);
            serializedUI.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedUI.FindProperty("dialoguePanel").objectReferenceValue = dialogueRoot;
            serializedUI.FindProperty("backgroundImage").objectReferenceValue = background.GetComponent<Image>();
            serializedUI.FindProperty("leftPortrait").objectReferenceValue = leftPortrait.GetComponent<Image>();
            serializedUI.FindProperty("rightPortrait").objectReferenceValue = rightPortrait.GetComponent<Image>();
            serializedUI.FindProperty("centerPortrait").objectReferenceValue = centerPortrait.GetComponent<Image>();
            serializedUI.FindProperty("dialogueBox").objectReferenceValue = dialogueBox;
            serializedUI.FindProperty("characterNameText").objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
            serializedUI.FindProperty("dialogueText").objectReferenceValue = dialogueTextGO.GetComponent<TextMeshProUGUI>();
            serializedUI.FindProperty("continueIndicator").objectReferenceValue = continueIndicator;
            serializedUI.FindProperty("skipButton").objectReferenceValue = skipButton.GetComponent<Button>();
            serializedUI.ApplyModifiedProperties();

            // 创建DialoguePlayer
            GameObject playerGO = new GameObject("DialoguePlayer");
            DialoguePlayer player = playerGO.AddComponent<DialoguePlayer>();
            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
            serializedPlayer.ApplyModifiedProperties();

            // 选中创建的对象
            Selection.activeGameObject = dialogueRoot;

            Debug.Log("对话UI创建完成！请在DialoguePlayer上设置对话数据。");
        }

        private static GameObject CreateImage(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            Image image = go.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);

            Button button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            button.colors = colors;

            GameObject textGO = CreateText(go.transform, "Text", text, 18, Color.white);
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return go;
        }

        [MenuItem("文明之旅/创建示例对话数据", false, 201)]
        public static void CreateSampleDialogue()
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/DialogueData"))
            {
                AssetDatabase.CreateFolder("Assets", "DialogueData");
            }

            // 创建示例对话
            DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
            dialogue.dialogueTitle = "示例对话";
            dialogue.dialogueDescription = "这是一个示例对话，展示对话系统的基本功能。";
            dialogue.allowSkip = true;
            dialogue.allowFastForward = true;
            dialogue.defaultTypingSpeed = 0.05f;

            // 添加场景1
            DialogueScene scene1 = new DialogueScene
            {
                sceneName = "开场",
                sceneDescription = "故事的开始"
            };

            scene1.dialogueLines.Add(new DialogueLine
            {
                characterName = "旁白",
                dialogueText = "在遥远的古代，一个伟大的文明正在崛起...",
                portraitPosition = PortraitPosition.Center,
                typingSpeed = 0.05f
            });

            scene1.dialogueLines.Add(new DialogueLine
            {
                characterName = "主角",
                dialogueText = "这里就是传说中的古城吗？真是壮观啊！",
                portraitPosition = PortraitPosition.Left,
                typingSpeed = 0.05f
            });

            scene1.dialogueLines.Add(new DialogueLine
            {
                characterName = "向导",
                dialogueText = "是的，这座城市有着数千年的历史。让我带你参观一下吧。",
                portraitPosition = PortraitPosition.Right,
                typingSpeed = 0.05f
            });

            dialogue.scenes.Add(scene1);

            // 添加场景2
            DialogueScene scene2 = new DialogueScene
            {
                sceneName = "探索",
                sceneDescription = "探索古城"
            };

            scene2.dialogueLines.Add(new DialogueLine
            {
                characterName = "主角",
                dialogueText = "这些壁画描绘的是什么？",
                portraitPosition = PortraitPosition.Left,
                typingSpeed = 0.05f
            });

            scene2.dialogueLines.Add(new DialogueLine
            {
                characterName = "向导",
                dialogueText = "这是古代文明的历史记录，记载了他们的辉煌成就。",
                portraitPosition = PortraitPosition.Right,
                typingSpeed = 0.05f
            });

            dialogue.scenes.Add(scene2);

            // 保存
            string path = "Assets/DialogueData/SampleDialogue.asset";
            AssetDatabase.CreateAsset(dialogue, path);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = dialogue;

            Debug.Log($"示例对话已创建: {path}");
        }
    }
}
