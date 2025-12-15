using System;
using System.Collections.Generic;
using UnityEngine;

namespace CivilizationJourney.Dialogue
{
    /// <summary>
    /// 角色表情/状态
    /// </summary>
    [Serializable]
    public class CharacterExpression
    {
        [Tooltip("表情名称")]
        public string expressionName;
        
        [Tooltip("表情立绘")]
        public Sprite expressionSprite;
    }

    /// <summary>
    /// 角色数据 ScriptableObject
    /// 用于存储角色的基本信息和立绘
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "文明之旅/角色数据", order = 2)]
    public class CharacterData : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("角色名称")]
        public string characterName;
        
        [Tooltip("角色描述")]
        [TextArea(2, 5)]
        public string characterDescription;
        
        [Tooltip("角色名字颜色")]
        public Color nameColor = Color.white;

        [Header("立绘")]
        [Tooltip("默认立绘")]
        public Sprite defaultPortrait;
        
        [Tooltip("表情立绘列表")]
        public List<CharacterExpression> expressions = new List<CharacterExpression>();

        [Header("音效")]
        [Tooltip("默认说话音效")]
        public AudioClip defaultVoice;
        
        [Tooltip("默认打字音效")]
        public AudioClip defaultTypingSound;

        /// <summary>
        /// 根据表情名称获取立绘
        /// </summary>
        public Sprite GetExpression(string expressionName)
        {
            if (string.IsNullOrEmpty(expressionName))
            {
                return defaultPortrait;
            }

            foreach (var expression in expressions)
            {
                if (expression.expressionName == expressionName)
                {
                    return expression.expressionSprite;
                }
            }
            return defaultPortrait;
        }
    }
}
