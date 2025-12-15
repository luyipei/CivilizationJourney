using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CivilizationJourney.Dialogue
{
    /// <summary>
    /// 对话播放器
    /// 负责播放对话数据，控制对话流程
    /// </summary>
    public class DialoguePlayer : MonoBehaviour
    {
        [Header("当前对话")]
        [SerializeField] private DialogueData currentDialogue;

        [Header("UI引用")]
        [SerializeField] private DialogueUI dialogueUI;

        [Header("播放设置")]
        [SerializeField] private bool autoStart = false;
        [SerializeField] private bool pauseGameOnDialogue = false;

        // 状态
        private int currentSceneIndex = 0;
        private int currentLineIndex = 0;
        private bool isPlaying = false;
        private bool isTyping = false;
        private Coroutine typingCoroutine;

        // 事件
        public UnityEvent onDialogueStart;
        public UnityEvent onDialogueEnd;
        public UnityEvent<DialogueLine> onLineStart;
        public UnityEvent<DialogueLine> onLineEnd;
        public UnityEvent<int> onSceneChange;

        // 属性
        public bool IsPlaying => isPlaying;
        public bool IsTyping => isTyping;
        public DialogueData CurrentDialogue => currentDialogue;
        public int CurrentSceneIndex => currentSceneIndex;
        public int CurrentLineIndex => currentLineIndex;

        /// <summary>
        /// 设置对话UI引用（供DialogueLoader调用）
        /// </summary>
        public void SetDialogueUI(DialogueUI ui)
        {
            dialogueUI = ui;
        }

        /// <summary>
        /// 设置对话数据（供DialogueLoader调用）
        /// </summary>
        public void SetDialogueData(DialogueData data)
        {
            currentDialogue = data;
        }

        private void Start()
        {
            if (autoStart && currentDialogue != null)
            {
                StartDialogue();
            }
        }

        /// <summary>
        /// 开始播放对话
        /// </summary>
        public void StartDialogue()
        {
            if (currentDialogue == null)
            {
                Debug.LogError("DialoguePlayer: 没有设置对话数据!");
                return;
            }

            StartDialogue(currentDialogue);
        }

        /// <summary>
        /// 开始播放指定对话
        /// </summary>
        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogError("DialoguePlayer: 对话数据为空!");
                return;
            }

            currentDialogue = dialogue;
            currentSceneIndex = 0;
            currentLineIndex = 0;
            isPlaying = true;

            if (pauseGameOnDialogue)
            {
                Time.timeScale = 0;
            }

            if (dialogueUI != null)
            {
                dialogueUI.Show();
                dialogueUI.SetSkipButtonActive(currentDialogue.allowSkip);
            }

            onDialogueStart?.Invoke();
            PlayCurrentLine();
        }

        /// <summary>
        /// 播放当前行
        /// </summary>
        private void PlayCurrentLine()
        {
            var line = GetCurrentLine();
            if (line == null)
            {
                EndDialogue();
                return;
            }

            onLineStart?.Invoke(line);

            if (dialogueUI != null)
            {
                dialogueUI.SetCharacterName(line.characterName);
                dialogueUI.SetPortrait(line.portrait, line.portraitPosition, line.portraitAnimation);
                
                if (line.hideOtherPortrait)
                {
                    dialogueUI.HideOtherPortrait(line.portraitPosition);
                }

                if (line.backgroundImage != null)
                {
                    dialogueUI.SetBackground(line.backgroundImage);
                }

                // 开始打字效果
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                typingCoroutine = StartCoroutine(TypeText(line));
            }

            // 播放语音
            if (line.voiceClip != null)
            {
                PlayVoice(line.voiceClip);
            }

            // 播放背景音乐
            if (line.backgroundMusic != null)
            {
                PlayBGM(line.backgroundMusic);
            }
        }

        /// <summary>
        /// 打字机效果协程
        /// </summary>
        private IEnumerator TypeText(DialogueLine line)
        {
            isTyping = true;
            string fullText = line.dialogueText;
            string currentText = "";

            for (int i = 0; i < fullText.Length; i++)
            {
                currentText += fullText[i];
                dialogueUI?.SetDialogueText(currentText);

                // 播放打字音效
                if (line.typingSound != null && i % 2 == 0)
                {
                    PlayTypingSound(line.typingSound);
                }

                yield return new WaitForSecondsRealtime(line.typingSpeed);
            }

            isTyping = false;
            onLineEnd?.Invoke(line);

            // 自动播放下一句
            if (line.autoNext)
            {
                yield return new WaitForSecondsRealtime(line.autoNextDelay);
                NextLine();
            }
        }

        /// <summary>
        /// 下一行对话
        /// </summary>
        public void NextLine()
        {
            if (!isPlaying) return;

            // 如果正在打字，先完成打字
            if (isTyping)
            {
                CompleteTyping();
                return;
            }

            var scene = GetCurrentScene();
            if (scene == null)
            {
                EndDialogue();
                return;
            }

            currentLineIndex++;

            // 检查是否需要切换场景
            if (currentLineIndex >= scene.dialogueLines.Count)
            {
                currentSceneIndex++;
                currentLineIndex = 0;

                if (currentSceneIndex >= currentDialogue.scenes.Count)
                {
                    EndDialogue();
                    return;
                }

                onSceneChange?.Invoke(currentSceneIndex);
            }

            PlayCurrentLine();
        }

        /// <summary>
        /// 完成当前打字效果
        /// </summary>
        public void CompleteTyping()
        {
            if (!isTyping) return;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            var line = GetCurrentLine();
            if (line != null && dialogueUI != null)
            {
                dialogueUI.SetDialogueText(line.dialogueText);
            }

            isTyping = false;
            onLineEnd?.Invoke(line);
        }

        /// <summary>
        /// 跳过对话
        /// </summary>
        public void SkipDialogue()
        {
            if (!isPlaying) return;
            if (!currentDialogue.allowSkip) return;

            EndDialogue();
        }

        /// <summary>
        /// 结束对话
        /// </summary>
        public void EndDialogue()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            isPlaying = false;
            isTyping = false;

            if (pauseGameOnDialogue)
            {
                Time.timeScale = 1;
            }

            if (dialogueUI != null)
            {
                dialogueUI.Hide();
            }

            onDialogueEnd?.Invoke();
        }

        /// <summary>
        /// 跳转到指定场景
        /// </summary>
        public void GoToScene(int sceneIndex)
        {
            if (sceneIndex < 0 || sceneIndex >= currentDialogue.scenes.Count) return;

            currentSceneIndex = sceneIndex;
            currentLineIndex = 0;
            onSceneChange?.Invoke(currentSceneIndex);
            PlayCurrentLine();
        }

        /// <summary>
        /// 跳转到指定行
        /// </summary>
        public void GoToLine(int sceneIndex, int lineIndex)
        {
            if (sceneIndex < 0 || sceneIndex >= currentDialogue.scenes.Count) return;
            if (lineIndex < 0 || lineIndex >= currentDialogue.scenes[sceneIndex].dialogueLines.Count) return;

            currentSceneIndex = sceneIndex;
            currentLineIndex = lineIndex;
            PlayCurrentLine();
        }

        /// <summary>
        /// 获取当前场景
        /// </summary>
        private DialogueScene GetCurrentScene()
        {
            return currentDialogue?.GetScene(currentSceneIndex);
        }

        /// <summary>
        /// 获取当前对话行
        /// </summary>
        private DialogueLine GetCurrentLine()
        {
            return currentDialogue?.GetDialogueLine(currentSceneIndex, currentLineIndex);
        }

        // 音频播放方法（可根据项目需求扩展）
        private void PlayVoice(AudioClip clip)
        {
            // TODO: 实现语音播放
            // AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }

        private void PlayTypingSound(AudioClip clip)
        {
            // TODO: 实现打字音效播放
        }

        private void PlayBGM(AudioClip clip)
        {
            // TODO: 实现背景音乐播放
        }

        private void Update()
        {
            if (!isPlaying) return;

            // 点击或按空格继续
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                NextLine();
            }

            // 按Escape跳过
            if (Input.GetKeyDown(KeyCode.Escape) && currentDialogue.allowSkip)
            {
                SkipDialogue();
            }
        }
    }
}
