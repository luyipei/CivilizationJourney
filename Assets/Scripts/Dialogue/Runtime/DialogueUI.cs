using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CivilizationJourney.Dialogue
{
    /// <summary>
    /// 对话UI控制器
    /// 参考视觉小说风格：全屏背景 + 大立绘 + 底部对话框
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("主容器")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject dialoguePanel;

        [Header("背景")]
        [SerializeField] private Image backgroundImage;

        [Header("立绘")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private RectTransform portraitRect;

        [Header("对话框")]
        [SerializeField] private Image dialogueBoxImage;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private Image nameUnderline;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject continueIndicator;

        [Header("功能按钮")]
        [SerializeField] private Button autoButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private Button skipButton;

        [Header("动画设置")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float portraitFadeDuration = 0.25f;
        [SerializeField] private float portraitSlideDuration = 0.3f;

        [Header("立绘位置预设")]
        [SerializeField] private Vector2 leftPosition = new Vector2(-400, 0);
        [SerializeField] private Vector2 rightPosition = new Vector2(400, 0);
        [SerializeField] private Vector2 centerPosition = new Vector2(0, 0);

        [Header("立绘尺寸预设")]
        [SerializeField] private Vector2 normalSize = new Vector2(600, 900);
        [SerializeField] private Vector2 largeSize = new Vector2(900, 1200);
        [SerializeField] private Vector2 smallSize = new Vector2(400, 600);

        private Coroutine fadeCoroutine;
        private Coroutine portraitAnimCoroutine;
        
        // 当前状态
        private Sprite currentBackground;
        private bool isAutoMode = false;

        // 历史记录
        private List<DialogueHistoryEntry> dialogueHistory = new List<DialogueHistoryEntry>();

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            // 初始隐藏立绘
            if (portraitImage != null)
            {
                portraitImage.gameObject.SetActive(false);
            }

            // 绑定按钮事件
            if (autoButton != null)
            {
                autoButton.onClick.AddListener(OnAutoClicked);
            }
            if (historyButton != null)
            {
                historyButton.onClick.AddListener(OnHistoryClicked);
            }
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }
        }

        /// <summary>
        /// 显示对话UI
        /// </summary>
        public void Show()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeIn());
        }

        /// <summary>
        /// 隐藏对话UI
        /// </summary>
        public void Hide()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            float elapsed = 0;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        /// <summary>
        /// 设置背景图片
        /// </summary>
        public void SetBackground(Sprite background)
        {
            if (background == null) return;
            
            currentBackground = background;
            
            if (backgroundImage != null)
            {
                backgroundImage.sprite = background;
                backgroundImage.color = Color.white;
            }
        }

        /// <summary>
        /// 获取当前背景
        /// </summary>
        public Sprite GetCurrentBackground()
        {
            return currentBackground;
        }

        /// <summary>
        /// 设置角色名
        /// </summary>
        public void SetCharacterName(string name)
        {
            if (characterNameText != null)
            {
                characterNameText.text = name;
            }
        }

        /// <summary>
        /// 设置对话文本
        /// </summary>
        public void SetDialogueText(string text)
        {
            if (dialogueText != null)
            {
                dialogueText.text = text;
            }
        }

        /// <summary>
        /// 设置立绘
        /// </summary>
        public void SetPortrait(Sprite portrait, PortraitPosition position, PortraitSize size, 
            PortraitAnimation animation, float offsetX, float offsetY, float scale)
        {
            if (portraitImage == null || portraitRect == null) return;

            if (portrait == null)
            {
                // 隐藏立绘
                if (portraitAnimCoroutine != null)
                {
                    StopCoroutine(portraitAnimCoroutine);
                }
                portraitAnimCoroutine = StartCoroutine(FadeOutPortrait());
                return;
            }

            // 设置立绘图片
            portraitImage.sprite = portrait;
            portraitImage.preserveAspect = true;
            portraitImage.gameObject.SetActive(true);

            // 计算位置
            Vector2 targetPos = GetPositionByEnum(position);
            targetPos.x += offsetX;
            targetPos.y += offsetY;

            // 计算尺寸
            Vector2 targetSize = GetSizeByEnum(size);
            targetSize *= scale;

            // 播放动画
            if (portraitAnimCoroutine != null)
            {
                StopCoroutine(portraitAnimCoroutine);
            }

            switch (animation)
            {
                case PortraitAnimation.FadeIn:
                    portraitAnimCoroutine = StartCoroutine(AnimateFadeIn(targetPos, targetSize));
                    break;
                case PortraitAnimation.SlideIn:
                    portraitAnimCoroutine = StartCoroutine(AnimateSlideIn(targetPos, targetSize, position));
                    break;
                case PortraitAnimation.Shake:
                    SetPortraitImmediate(targetPos, targetSize);
                    portraitAnimCoroutine = StartCoroutine(AnimateShake());
                    break;
                case PortraitAnimation.Bounce:
                    SetPortraitImmediate(targetPos, targetSize);
                    portraitAnimCoroutine = StartCoroutine(AnimateBounce());
                    break;
                default:
                    SetPortraitImmediate(targetPos, targetSize);
                    break;
            }
        }

        /// <summary>
        /// 隐藏立绘
        /// </summary>
        public void HidePortrait()
        {
            if (portraitAnimCoroutine != null)
            {
                StopCoroutine(portraitAnimCoroutine);
            }
            portraitAnimCoroutine = StartCoroutine(FadeOutPortrait());
        }

        private void SetPortraitImmediate(Vector2 position, Vector2 size)
        {
            portraitRect.anchoredPosition = position;
            portraitRect.sizeDelta = size;
            portraitImage.color = Color.white;
        }

        private Vector2 GetPositionByEnum(PortraitPosition position)
        {
            switch (position)
            {
                case PortraitPosition.Left:
                    return leftPosition;
                case PortraitPosition.Right:
                    return rightPosition;
                case PortraitPosition.Center:
                    return centerPosition;
                case PortraitPosition.LeftFar:
                    return new Vector2(leftPosition.x - 200, leftPosition.y);
                case PortraitPosition.RightFar:
                    return new Vector2(rightPosition.x + 200, rightPosition.y);
                default:
                    return rightPosition;
            }
        }

        private Vector2 GetSizeByEnum(PortraitSize size)
        {
            switch (size)
            {
                case PortraitSize.Normal:
                    return normalSize;
                case PortraitSize.Large:
                    return largeSize;
                case PortraitSize.Small:
                    return smallSize;
                default:
                    return largeSize;
            }
        }

        #region Portrait Animations

        private IEnumerator AnimateFadeIn(Vector2 targetPos, Vector2 targetSize)
        {
            portraitRect.anchoredPosition = targetPos;
            portraitRect.sizeDelta = targetSize;
            
            float elapsed = 0;
            Color startColor = new Color(1, 1, 1, 0);
            Color endColor = Color.white;
            portraitImage.color = startColor;

            while (elapsed < portraitFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                portraitImage.color = Color.Lerp(startColor, endColor, elapsed / portraitFadeDuration);
                yield return null;
            }

            portraitImage.color = endColor;
        }

        private IEnumerator AnimateSlideIn(Vector2 targetPos, Vector2 targetSize, PortraitPosition position)
        {
            portraitRect.sizeDelta = targetSize;
            portraitImage.color = Color.white;

            // 从屏幕外滑入
            Vector2 startPos = targetPos;
            if (position == PortraitPosition.Left || position == PortraitPosition.LeftFar)
            {
                startPos.x -= 500;
            }
            else
            {
                startPos.x += 500;
            }

            portraitRect.anchoredPosition = startPos;

            float elapsed = 0;
            while (elapsed < portraitSlideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / portraitSlideDuration;
                t = 1 - Mathf.Pow(1 - t, 3); // Ease out cubic
                portraitRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            portraitRect.anchoredPosition = targetPos;
        }

        private IEnumerator AnimateShake()
        {
            Vector2 originalPos = portraitRect.anchoredPosition;
            float duration = 0.3f;
            float intensity = 15f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float x = originalPos.x + Random.Range(-intensity, intensity);
                float y = originalPos.y + Random.Range(-intensity, intensity);
                portraitRect.anchoredPosition = new Vector2(x, y);
                yield return null;
            }

            portraitRect.anchoredPosition = originalPos;
        }

        private IEnumerator AnimateBounce()
        {
            Vector2 originalPos = portraitRect.anchoredPosition;
            float duration = 0.4f;
            float bounceHeight = 30f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float bounce = Mathf.Sin(t * Mathf.PI) * bounceHeight;
                portraitRect.anchoredPosition = new Vector2(originalPos.x, originalPos.y + bounce);
                yield return null;
            }

            portraitRect.anchoredPosition = originalPos;
        }

        private IEnumerator FadeOutPortrait()
        {
            if (!portraitImage.gameObject.activeSelf) yield break;

            float elapsed = 0;
            Color startColor = portraitImage.color;
            Color endColor = new Color(1, 1, 1, 0);

            while (elapsed < portraitFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                portraitImage.color = Color.Lerp(startColor, endColor, elapsed / portraitFadeDuration);
                yield return null;
            }

            portraitImage.gameObject.SetActive(false);
            portraitImage.color = Color.white;
        }

        #endregion

        /// <summary>
        /// 设置跳过按钮是否可用
        /// </summary>
        public void SetSkipButtonActive(bool active)
        {
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 显示/隐藏继续提示
        /// </summary>
        public void SetContinueIndicator(bool show)
        {
            if (continueIndicator != null)
            {
                continueIndicator.SetActive(show);
            }
        }

        /// <summary>
        /// 添加历史记录
        /// </summary>
        public void AddToHistory(string characterName, string dialogueText)
        {
            dialogueHistory.Add(new DialogueHistoryEntry
            {
                characterName = characterName,
                dialogueText = dialogueText
            });

            // 限制历史记录数量
            if (dialogueHistory.Count > 100)
            {
                dialogueHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// 获取自动播放状态
        /// </summary>
        public bool IsAutoMode => isAutoMode;

        #region Button Callbacks

        private void OnAutoClicked()
        {
            isAutoMode = !isAutoMode;
            
            // 更新按钮显示
            if (autoButton != null)
            {
                var text = autoButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = isAutoMode ? "自动 ■" : "自动 ▶";
                }
            }

            Debug.Log("自动播放: " + (isAutoMode ? "开启" : "关闭"));
        }

        private void OnHistoryClicked()
        {
            // TODO: 显示历史对话窗口
            Debug.Log("历史对话 - 共 " + dialogueHistory.Count + " 条记录");
        }

        private void OnSkipClicked()
        {
            var player = FindObjectOfType<DialoguePlayer>();
            if (player != null)
            {
                player.SkipScene();
            }
        }

        #endregion
    }

    /// <summary>
    /// 对话历史记录条目
    /// </summary>
    [System.Serializable]
    public class DialogueHistoryEntry
    {
        public string characterName;
        public string dialogueText;
    }
}
