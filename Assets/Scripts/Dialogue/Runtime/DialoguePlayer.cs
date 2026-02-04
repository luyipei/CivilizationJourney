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
        private Coroutine autoPlayCoroutine;

        // 当前背景和BGM（用于继承）
        private Sprite currentBackground;
        private AudioClip currentBGM;

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

            // 初始化背景和BGM
            currentBackground = dialogue.defaultBackground;
            currentBGM = dialogue.defaultBGM;

            if (pauseGameOnDialogue)
            {
                Time.timeScale = 0;
            }

            if (dialogueUI != null)
            {
                dialogueUI.Show();
                dialogueUI.SetSkipButtonActive(currentDialogue.allowSkip);
                
                // 设置初始背景
                if (currentBackground != null)
                {
                    dialogueUI.SetBackground(currentBackground);
                }
            }

            // 播放初始BGM
            if (currentBGM != null)
            {
                PlayBGM(currentBGM);
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

            var scene = GetCurrentScene();

            onLineStart?.Invoke(line);

            if (dialogueUI != null)
            {
                // 处理背景：对话行 > 场景 > 全局 > 继承上一个
                Sprite bgToUse = null;
                if (line.backgroundImage != null)
                {
                    bgToUse = line.backgroundImage;
                }
                else if (scene != null && scene.sceneBackground != null && currentLineIndex == 0)
                {
                    // 场景开始时使用场景背景
                    bgToUse = scene.sceneBackground;
                }

                if (bgToUse != null)
                {
                    currentBackground = bgToUse;
                    dialogueUI.SetBackground(bgToUse);
                }

                // 设置角色名
                dialogueUI.SetCharacterName(line.characterName);

                // 设置立绘
                if (line.hidePortrait)
                {
                    dialogueUI.HidePortrait();
                }
                else
                {
                    dialogueUI.SetPortrait(
                        line.portrait,
                        line.portraitPosition,
                        line.portraitSize,
                        line.portraitAnimation,
                        line.portraitOffsetX,
                        line.portraitOffsetY,
                        line.portraitScale
                    );
                }

                // 添加到历史记录
                dialogueUI.AddToHistory(line.characterName, line.dialogueText);

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

            // 处理BGM：对话行 > 场景 > 继承
            AudioClip bgmToUse = null;
            if (line.backgroundMusic != null)
            {
                bgmToUse = line.backgroundMusic;
            }
            else if (scene != null && scene.sceneBGM != null && currentLineIndex == 0)
            {
                bgmToUse = scene.sceneBGM;
            }

            if (bgmToUse != null && bgmToUse != currentBGM)
            {
                currentBGM = bgmToUse;
                PlayBGM(bgmToUse);
            }
        }

        /// <summary>
        /// 打字机效果协程
        /// </summary>
        private IEnumerator TypeText(DialogueLine line)
        {
            isTyping = true;
            dialogueUI?.SetContinueIndicator(false);
            
            string fullText = line.dialogueText;
            string currentText = "";

            float speed = line.typingSpeed > 0 ? line.typingSpeed : currentDialogue.defaultTypingSpeed;

            for (int i = 0; i < fullText.Length; i++)
            {
                currentText += fullText[i];
                dialogueUI?.SetDialogueText(currentText);

                // 播放打字音效
                if (line.typingSound != null && i % 2 == 0)
                {
                    PlayTypingSound(line.typingSound);
                }

                yield return new WaitForSecondsRealtime(speed);
            }

            isTyping = false;
            dialogueUI?.SetContinueIndicator(true);
            onLineEnd?.Invoke(line);

            // 自动播放模式或对话行设置了自动播放
            if (line.autoNext || (dialogueUI != null && dialogueUI.IsAutoMode))
            {
                float delay = line.autoNext ? line.autoNextDelay : 1.5f;
                autoPlayCoroutine = StartCoroutine(AutoPlayNext(delay));
            }
        }

        private IEnumerator AutoPlayNext(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (isPlaying && !isTyping)
            {
                NextLine();
            }
        }

        /// <summary>
        /// 下一行对话
        /// </summary>
        public void NextLine()
        {
            if (!isPlaying) return;

            // 停止自动播放协程
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
                autoPlayCoroutine = null;
            }

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
                dialogueUI.SetContinueIndicator(true);
            }

            isTyping = false;
            onLineEnd?.Invoke(line);

            // 自动播放模式
            if (dialogueUI != null && dialogueUI.IsAutoMode)
            {
                autoPlayCoroutine = StartCoroutine(AutoPlayNext(1.5f));
            }
        }

        /// <summary>
        /// 跳过当前场景，进入下一个场景
        /// </summary>
        public void SkipScene()
        {
            if (!isPlaying) return;
            if (!currentDialogue.allowSkip) return;

            // 停止当前协程
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
                autoPlayCoroutine = null;
            }
            isTyping = false;

            // 跳到下一个场景
            currentSceneIndex++;
            currentLineIndex = 0;

            // 如果没有下一个场景了，结束对话
            if (currentSceneIndex >= currentDialogue.scenes.Count)
            {
                EndDialogue();
                return;
            }

            onSceneChange?.Invoke(currentSceneIndex);
            PlayCurrentLine();
        }

        /// <summary>
        /// 跳过整个对话（直接结束）
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
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
                autoPlayCoroutine = null;
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
        private AudioSource bgmSource;
        private AudioSource voiceSource;
        private AudioSource sfxSource;

        private void PlayVoice(AudioClip clip)
        {
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        private void PlayTypingSound(AudioClip clip)
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = 0.5f;
            }
            sfxSource.PlayOneShot(clip);
        }

        private void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
                bgmSource.volume = 0.7f;
            }
            
            if (bgmSource.clip != clip)
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }

        private void Update()
        {
            if (!isPlaying) return;

            // 点击或按空格继续
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                NextLine();
            }

            // 按Escape跳过当前场景
            if (Input.GetKeyDown(KeyCode.Escape) && currentDialogue.allowSkip)
            {
                SkipScene();
            }
        }
    }
}
