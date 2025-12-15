using System;
using System.Collections.Generic;
using UnityEngine;

namespace CivilizationJourney.Dialogue
{
    /// <summary>
    /// 立绘位置枚举
    /// </summary>
    public enum PortraitPosition
    {
        Left,   // 左侧立绘
        Right,  // 右侧立绘
        Center  // 中间立绘
    }

    /// <summary>
    /// 立绘动画效果
    /// </summary>
    public enum PortraitAnimation
    {
        None,       // 无动画
        FadeIn,     // 淡入
        FadeOut,    // 淡出
        Shake,      // 抖动
        Bounce      // 弹跳
    }

    /// <summary>
    /// 单条对话数据
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [Header("角色信息")]
        [Tooltip("说话角色的名字")]
        public string characterName;
        
        [Tooltip("角色立绘")]
        public Sprite portrait;
        
        [Tooltip("立绘位置")]
        public PortraitPosition portraitPosition = PortraitPosition.Left;
        
        [Tooltip("立绘动画")]
        public PortraitAnimation portraitAnimation = PortraitAnimation.None;

        [Header("对话内容")]
        [TextArea(3, 10)]
        [Tooltip("对话文本内容")]
        public string dialogueText;
        
        [Tooltip("打字机效果速度（每个字符间隔秒数）")]
        [Range(0.01f, 0.2f)]
        public float typingSpeed = 0.05f;

        [Header("音效")]
        [Tooltip("对话音效")]
        public AudioClip voiceClip;
        
        [Tooltip("打字音效")]
        public AudioClip typingSound;

        [Header("高级设置")]
        [Tooltip("是否自动播放下一句")]
        public bool autoNext = false;
        
        [Tooltip("自动播放延迟时间")]
        [Range(0f, 5f)]
        public float autoNextDelay = 2f;
        
        [Tooltip("是否隐藏对方立绘")]
        public bool hideOtherPortrait = false;

        [Tooltip("背景图片（可选，留空则保持当前背景）")]
        public Sprite backgroundImage;

        [Tooltip("背景音乐（可选，留空则保持当前BGM）")]
        public AudioClip backgroundMusic;
    }

    /// <summary>
    /// 对话章节/场景
    /// </summary>
    [Serializable]
    public class DialogueScene
    {
        [Tooltip("场景名称")]
        public string sceneName;
        
        [Tooltip("场景描述")]
        [TextArea(2, 5)]
        public string sceneDescription;
        
        [Tooltip("该场景的对话列表")]
        public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    }

    /// <summary>
    /// 对话数据 ScriptableObject
    /// 用于存储一整段对话/剧情
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "文明之旅/对话数据", order = 1)]
    public class DialogueData : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("对话/剧情标题")]
        public string dialogueTitle;
        
        [Tooltip("对话/剧情描述")]
        [TextArea(3, 5)]
        public string dialogueDescription;

        [Header("场景列表")]
        [Tooltip("对话场景列表")]
        public List<DialogueScene> scenes = new List<DialogueScene>();

        [Header("全局设置")]
        [Tooltip("是否允许跳过")]
        public bool allowSkip = true;
        
        [Tooltip("是否允许快进")]
        public bool allowFastForward = true;
        
        [Tooltip("默认打字速度")]
        [Range(0.01f, 0.2f)]
        public float defaultTypingSpeed = 0.05f;

        /// <summary>
        /// 获取所有对话行数
        /// </summary>
        public int TotalLineCount
        {
            get
            {
                int count = 0;
                foreach (var scene in scenes)
                {
                    count += scene.dialogueLines.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// 获取指定场景的对话
        /// </summary>
        public DialogueScene GetScene(int index)
        {
            if (index >= 0 && index < scenes.Count)
            {
                return scenes[index];
            }
            return null;
        }

        /// <summary>
        /// 获取指定场景的指定对话行
        /// </summary>
        public DialogueLine GetDialogueLine(int sceneIndex, int lineIndex)
        {
            var scene = GetScene(sceneIndex);
            if (scene != null && lineIndex >= 0 && lineIndex < scene.dialogueLines.Count)
            {
                return scene.dialogueLines[lineIndex];
            }
            return null;
        }
    }
}
