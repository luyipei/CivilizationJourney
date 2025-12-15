using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CivilizationJourney.Dialogue.Editor
{
    /// <summary>
    /// 对话编辑器窗口
    /// 提供可视化的对话编辑界面，方便美术和策划使用
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        private DialogueData currentDialogue;
        private Vector2 leftPanelScroll;
        private Vector2 rightPanelScroll;
        private int selectedSceneIndex = -1;
        private int selectedLineIndex = -1;

        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle selectedStyle;
        private GUIStyle portraitPreviewStyle;

        private bool stylesInitialized = false;

        // 预览相关
        private bool isPreviewMode = false;
        private int previewLineIndex = 0;

        [MenuItem("文明之旅/对话编辑器 %#D", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<DialogueEditorWindow>("对话编辑器");
            window.minSize = new Vector2(900, 600);
            window.Show();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            boxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            selectedStyle = new GUIStyle("box")
            {
                padding = new RectOffset(5, 5, 5, 5)
            };
            selectedStyle.normal.background = MakeColorTexture(new Color(0.3f, 0.5f, 0.8f, 0.5f));

            portraitPreviewStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter
            };

            stylesInitialized = true;
        }

        private Texture2D MakeColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawToolbar();
            EditorGUILayout.EndHorizontal();

            if (currentDialogue == null)
            {
                DrawNoDialogueSelected();
                return;
            }

            EditorGUILayout.BeginHorizontal();

            // 左侧面板 - 场景和对话列表
            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            DrawLeftPanel();
            EditorGUILayout.EndVertical();

            // 分隔线
            EditorGUILayout.BeginVertical(GUILayout.Width(2));
            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));
            EditorGUILayout.EndVertical();

            // 右侧面板 - 对话详情编辑
            EditorGUILayout.BeginVertical();
            DrawRightPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // 处理拖放
            HandleDragAndDrop();
        }

        private void DrawToolbar()
        {
            if (GUILayout.Button("新建对话", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                CreateNewDialogue();
            }

            if (GUILayout.Button("打开对话", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                OpenDialogue();
            }

            if (currentDialogue != null)
            {
                if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    SaveDialogue();
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField($"当前: {currentDialogue.dialogueTitle}", GUILayout.Width(200));

                if (GUILayout.Button(isPreviewMode ? "退出预览" : "预览", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    isPreviewMode = !isPreviewMode;
                    if (isPreviewMode)
                    {
                        previewLineIndex = 0;
                    }
                }
            }
            else
            {
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawNoDialogueSelected()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(boxStyle, GUILayout.Width(400), GUILayout.Height(200));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("欢迎使用对话编辑器", headerStyle);
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("请新建或打开一个对话文件", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("新建对话", GUILayout.Width(120), GUILayout.Height(30)))
            {
                CreateNewDialogue();
            }
            GUILayout.Space(20);
            if (GUILayout.Button("打开对话", GUILayout.Width(120), GUILayout.Height(30)))
            {
                OpenDialogue();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            // 拖放提示
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("或将对话文件拖放到此窗口", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter });
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(50);
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.LabelField("场景列表", headerStyle);
            EditorGUILayout.Space(5);

            // 添加场景按钮
            if (GUILayout.Button("+ 添加场景", GUILayout.Height(25)))
            {
                AddNewScene();
            }

            EditorGUILayout.Space(10);

            leftPanelScroll = EditorGUILayout.BeginScrollView(leftPanelScroll);

            for (int i = 0; i < currentDialogue.scenes.Count; i++)
            {
                DrawSceneItem(i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSceneItem(int sceneIndex)
        {
            var scene = currentDialogue.scenes[sceneIndex];
            bool isSelected = selectedSceneIndex == sceneIndex;

            EditorGUILayout.BeginVertical(isSelected ? selectedStyle : boxStyle);

            // 场景标题行
            EditorGUILayout.BeginHorizontal();

            // 折叠/展开
            scene.sceneName = EditorGUILayout.TextField(scene.sceneName, EditorStyles.boldLabel);

            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("删除场景", $"确定要删除场景 \"{scene.sceneName}\" 吗？", "确定", "取消"))
                {
                    currentDialogue.scenes.RemoveAt(sceneIndex);
                    if (selectedSceneIndex == sceneIndex)
                    {
                        selectedSceneIndex = -1;
                        selectedLineIndex = -1;
                    }
                    EditorUtility.SetDirty(currentDialogue);
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            // 点击选择场景
            if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                selectedSceneIndex = sceneIndex;
                selectedLineIndex = -1;
                Repaint();
            }

            // 对话列表
            if (isSelected || scene.dialogueLines.Count > 0)
            {
                EditorGUILayout.Space(5);

                for (int j = 0; j < scene.dialogueLines.Count; j++)
                {
                    DrawDialogueLineItem(sceneIndex, j);
                }

                if (isSelected)
                {
                    EditorGUILayout.Space(5);
                    if (GUILayout.Button("+ 添加对话"))
                    {
                        AddNewDialogueLine(sceneIndex);
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawDialogueLineItem(int sceneIndex, int lineIndex)
        {
            var line = currentDialogue.scenes[sceneIndex].dialogueLines[lineIndex];
            bool isSelected = selectedSceneIndex == sceneIndex && selectedLineIndex == lineIndex;

            Color bgColor = isSelected ? new Color(0.4f, 0.6f, 0.9f, 0.3f) : Color.clear;
            var rect = EditorGUILayout.BeginHorizontal();

            if (isSelected)
            {
                EditorGUI.DrawRect(rect, bgColor);
            }

            // 立绘位置图标
            string posIcon = line.portraitPosition == PortraitPosition.Left ? "◀" : 
                            (line.portraitPosition == PortraitPosition.Right ? "▶" : "◆");
            EditorGUILayout.LabelField(posIcon, GUILayout.Width(15));

            // 角色名和对话预览
            string preview = string.IsNullOrEmpty(line.characterName) ? "(未命名)" : line.characterName;
            string textPreview = string.IsNullOrEmpty(line.dialogueText) ? "" : 
                (line.dialogueText.Length > 15 ? line.dialogueText.Substring(0, 15) + "..." : line.dialogueText);
            
            if (GUILayout.Button($"{preview}: {textPreview}", EditorStyles.label))
            {
                selectedSceneIndex = sceneIndex;
                selectedLineIndex = lineIndex;
            }

            // 删除按钮
            if (GUILayout.Button("×", GUILayout.Width(18)))
            {
                currentDialogue.scenes[sceneIndex].dialogueLines.RemoveAt(lineIndex);
                if (selectedLineIndex == lineIndex)
                {
                    selectedLineIndex = -1;
                }
                EditorUtility.SetDirty(currentDialogue);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRightPanel()
        {
            if (isPreviewMode)
            {
                DrawPreviewPanel();
                return;
            }

            if (selectedSceneIndex < 0 || selectedLineIndex < 0)
            {
                DrawDialogueInfo();
                return;
            }

            if (selectedSceneIndex >= currentDialogue.scenes.Count ||
                selectedLineIndex >= currentDialogue.scenes[selectedSceneIndex].dialogueLines.Count)
            {
                selectedLineIndex = -1;
                return;
            }

            var line = currentDialogue.scenes[selectedSceneIndex].dialogueLines[selectedLineIndex];

            EditorGUILayout.LabelField("对话编辑", headerStyle);
            EditorGUILayout.Space(10);

            rightPanelScroll = EditorGUILayout.BeginScrollView(rightPanelScroll);

            EditorGUI.BeginChangeCheck();

            // 角色信息
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("角色信息", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            line.characterName = EditorGUILayout.TextField("角色名", line.characterName);
            line.portrait = (Sprite)EditorGUILayout.ObjectField("立绘", line.portrait, typeof(Sprite), false);
            line.portraitPosition = (PortraitPosition)EditorGUILayout.EnumPopup("立绘位置", line.portraitPosition);
            line.portraitAnimation = (PortraitAnimation)EditorGUILayout.EnumPopup("立绘动画", line.portraitAnimation);
            line.hideOtherPortrait = EditorGUILayout.Toggle("隐藏对方立绘", line.hideOtherPortrait);

            // 立绘预览
            if (line.portrait != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("立绘预览:");
                var previewRect = GUILayoutUtility.GetRect(150, 200);
                previewRect.x = previewRect.x + (previewRect.width - 150) / 2;
                previewRect.width = 150;
                GUI.DrawTexture(previewRect, line.portrait.texture, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 对话内容
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("对话内容", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("对话文本:");
            line.dialogueText = EditorGUILayout.TextArea(line.dialogueText, GUILayout.Height(80));
            line.typingSpeed = EditorGUILayout.Slider("打字速度", line.typingSpeed, 0.01f, 0.2f);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 音效设置
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("音效设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            line.voiceClip = (AudioClip)EditorGUILayout.ObjectField("语音", line.voiceClip, typeof(AudioClip), false);
            line.typingSound = (AudioClip)EditorGUILayout.ObjectField("打字音效", line.typingSound, typeof(AudioClip), false);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 高级设置
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("高级设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            line.autoNext = EditorGUILayout.Toggle("自动播放下一句", line.autoNext);
            if (line.autoNext)
            {
                line.autoNextDelay = EditorGUILayout.Slider("自动播放延迟", line.autoNextDelay, 0f, 5f);
            }

            line.backgroundImage = (Sprite)EditorGUILayout.ObjectField("背景图片", line.backgroundImage, typeof(Sprite), false);
            line.backgroundMusic = (AudioClip)EditorGUILayout.ObjectField("背景音乐", line.backgroundMusic, typeof(AudioClip), false);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);

            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            
            if (selectedLineIndex > 0)
            {
                if (GUILayout.Button("↑ 上移", GUILayout.Height(30)))
                {
                    MoveDialogueLine(selectedSceneIndex, selectedLineIndex, -1);
                }
            }
            
            if (selectedLineIndex < currentDialogue.scenes[selectedSceneIndex].dialogueLines.Count - 1)
            {
                if (GUILayout.Button("↓ 下移", GUILayout.Height(30)))
                {
                    MoveDialogueLine(selectedSceneIndex, selectedLineIndex, 1);
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("复制", GUILayout.Height(30), GUILayout.Width(80)))
            {
                DuplicateDialogueLine(selectedSceneIndex, selectedLineIndex);
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentDialogue);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawDialogueInfo()
        {
            EditorGUILayout.LabelField("对话信息", headerStyle);
            EditorGUILayout.Space(10);

            rightPanelScroll = EditorGUILayout.BeginScrollView(rightPanelScroll);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            currentDialogue.dialogueTitle = EditorGUILayout.TextField("标题", currentDialogue.dialogueTitle);
            EditorGUILayout.LabelField("描述:");
            currentDialogue.dialogueDescription = EditorGUILayout.TextArea(currentDialogue.dialogueDescription, GUILayout.Height(60));

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("全局设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            currentDialogue.allowSkip = EditorGUILayout.Toggle("允许跳过", currentDialogue.allowSkip);
            currentDialogue.allowFastForward = EditorGUILayout.Toggle("允许快进", currentDialogue.allowFastForward);
            currentDialogue.defaultTypingSpeed = EditorGUILayout.Slider("默认打字速度", currentDialogue.defaultTypingSpeed, 0.01f, 0.2f);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField($"场景数量: {currentDialogue.scenes.Count}");
            EditorGUILayout.LabelField($"对话总数: {currentDialogue.TotalLineCount}");

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentDialogue);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("提示: 在左侧选择一条对话进行编辑", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.LabelField("对话预览", headerStyle);
            EditorGUILayout.Space(10);

            if (currentDialogue.scenes.Count == 0)
            {
                EditorGUILayout.LabelField("没有可预览的对话", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // 获取当前预览的对话
            int totalLines = 0;
            DialogueLine currentLine = null;
            int currentSceneIdx = 0;
            int currentLineIdx = 0;

            for (int i = 0; i < currentDialogue.scenes.Count; i++)
            {
                var scene = currentDialogue.scenes[i];
                if (previewLineIndex < totalLines + scene.dialogueLines.Count)
                {
                    currentSceneIdx = i;
                    currentLineIdx = previewLineIndex - totalLines;
                    currentLine = scene.dialogueLines[currentLineIdx];
                    break;
                }
                totalLines += scene.dialogueLines.Count;
            }

            if (currentLine == null)
            {
                previewLineIndex = 0;
                return;
            }

            // 预览区域
            EditorGUILayout.BeginVertical(boxStyle, GUILayout.Height(400));

            // 背景区域
            var bgRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(250));
            EditorGUI.DrawRect(bgRect, new Color(0.2f, 0.2f, 0.3f));

            // 立绘
            if (currentLine.portrait != null)
            {
                var portraitRect = new Rect(
                    currentLine.portraitPosition == PortraitPosition.Left ? bgRect.x + 20 : bgRect.xMax - 170,
                    bgRect.y + 10,
                    150,
                    230
                );
                GUI.DrawTexture(portraitRect, currentLine.portrait.texture, ScaleMode.ScaleToFit);
            }

            // 对话框
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(currentLine.characterName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(currentLine.dialogueText, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            // 控制按钮
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"场景: {currentDialogue.scenes[currentSceneIdx].sceneName} | 对话: {previewLineIndex + 1}/{currentDialogue.TotalLineCount}");

            if (GUILayout.Button("◀ 上一句", GUILayout.Width(80)) && previewLineIndex > 0)
            {
                previewLineIndex--;
            }

            if (GUILayout.Button("下一句 ▶", GUILayout.Width(80)) && previewLineIndex < currentDialogue.TotalLineCount - 1)
            {
                previewLineIndex++;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewDialogue()
        {
            string path = EditorUtility.SaveFilePanelInProject("新建对话", "NewDialogue", "asset", "选择保存位置");
            if (string.IsNullOrEmpty(path)) return;

            var newDialogue = CreateInstance<DialogueData>();
            newDialogue.dialogueTitle = System.IO.Path.GetFileNameWithoutExtension(path);
            newDialogue.scenes.Add(new DialogueScene { sceneName = "场景1" });

            AssetDatabase.CreateAsset(newDialogue, path);
            AssetDatabase.SaveAssets();

            currentDialogue = newDialogue;
            selectedSceneIndex = 0;
            selectedLineIndex = -1;

            EditorGUIUtility.PingObject(newDialogue);
        }

        private void OpenDialogue()
        {
            string path = EditorUtility.OpenFilePanel("打开对话", "Assets", "asset");
            if (string.IsNullOrEmpty(path)) return;

            // 转换为相对路径
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (dialogue != null)
            {
                currentDialogue = dialogue;
                selectedSceneIndex = currentDialogue.scenes.Count > 0 ? 0 : -1;
                selectedLineIndex = -1;
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "无法加载对话文件", "确定");
            }
        }

        private void SaveDialogue()
        {
            if (currentDialogue != null)
            {
                EditorUtility.SetDirty(currentDialogue);
                AssetDatabase.SaveAssets();
                Debug.Log($"对话 \"{currentDialogue.dialogueTitle}\" 已保存");
            }
        }

        private void AddNewScene()
        {
            var newScene = new DialogueScene
            {
                sceneName = $"场景{currentDialogue.scenes.Count + 1}"
            };
            currentDialogue.scenes.Add(newScene);
            selectedSceneIndex = currentDialogue.scenes.Count - 1;
            selectedLineIndex = -1;
            EditorUtility.SetDirty(currentDialogue);
        }

        private void AddNewDialogueLine(int sceneIndex)
        {
            var newLine = new DialogueLine
            {
                characterName = "角色名",
                dialogueText = "对话内容...",
                typingSpeed = currentDialogue.defaultTypingSpeed
            };
            currentDialogue.scenes[sceneIndex].dialogueLines.Add(newLine);
            selectedLineIndex = currentDialogue.scenes[sceneIndex].dialogueLines.Count - 1;
            EditorUtility.SetDirty(currentDialogue);
        }

        private void MoveDialogueLine(int sceneIndex, int lineIndex, int direction)
        {
            var lines = currentDialogue.scenes[sceneIndex].dialogueLines;
            int newIndex = lineIndex + direction;

            if (newIndex < 0 || newIndex >= lines.Count) return;

            var temp = lines[lineIndex];
            lines[lineIndex] = lines[newIndex];
            lines[newIndex] = temp;

            selectedLineIndex = newIndex;
            EditorUtility.SetDirty(currentDialogue);
        }

        private void DuplicateDialogueLine(int sceneIndex, int lineIndex)
        {
            var original = currentDialogue.scenes[sceneIndex].dialogueLines[lineIndex];
            var copy = new DialogueLine
            {
                characterName = original.characterName,
                portrait = original.portrait,
                portraitPosition = original.portraitPosition,
                portraitAnimation = original.portraitAnimation,
                dialogueText = original.dialogueText,
                typingSpeed = original.typingSpeed,
                voiceClip = original.voiceClip,
                typingSound = original.typingSound,
                autoNext = original.autoNext,
                autoNextDelay = original.autoNextDelay,
                hideOtherPortrait = original.hideOtherPortrait,
                backgroundImage = original.backgroundImage,
                backgroundMusic = original.backgroundMusic
            };

            currentDialogue.scenes[sceneIndex].dialogueLines.Insert(lineIndex + 1, copy);
            selectedLineIndex = lineIndex + 1;
            EditorUtility.SetDirty(currentDialogue);
        }

        private void HandleDragAndDrop()
        {
            var evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is DialogueData dialogue)
                        {
                            currentDialogue = dialogue;
                            selectedSceneIndex = currentDialogue.scenes.Count > 0 ? 0 : -1;
                            selectedLineIndex = -1;
                            break;
                        }
                    }
                }

                evt.Use();
            }
        }

        private void OnSelectionChange()
        {
            // 如果在Project窗口选择了对话文件，自动加载
            if (Selection.activeObject is DialogueData dialogue)
            {
                currentDialogue = dialogue;
                selectedSceneIndex = currentDialogue.scenes.Count > 0 ? 0 : -1;
                selectedLineIndex = -1;
                Repaint();
            }
        }
    }
}
