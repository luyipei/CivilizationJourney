using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CivilizationJourney.Dialogue
{
    /// <summary>
    /// 对话UI控制器
    /// 负责显示对话界面的各个元素
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("主容器")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject dialoguePanel;

        [Header("背景")]
        [SerializeField] private Image backgroundImage;

        [Header("立绘")]
        [SerializeField] private Image leftPortrait;
        [SerializeField] private Image rightPortrait;
        [SerializeField] private Image centerPortrait;

        [Header("对话框")]
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject continueIndicator;

        [Header("按钮")]
        [SerializeField] private Button skipButton;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button logButton;

        [Header("动画设置")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float portraitFadeDuration = 0.2f;

        [Header("立绘动画设置")]
        [SerializeField] private float shakeIntensity = 10f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float bounceHeight = 20f;
        [SerializeField] private float bounceDuration = 0.3f;

        private Coroutine fadeCoroutine;
        private Coroutine portraitAnimCoroutine;

        // 当前激活的立绘位置
        private PortraitPosition currentActivePosition = PortraitPosition.Left;

        private void Awake()
        {
            // 初始化隐藏
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            // 隐藏所有立绘
            HideAllPortraits();

            // 绑定按钮事件
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
        public void SetPortrait(Sprite portrait, PortraitPosition position, PortraitAnimation animation = PortraitAnimation.None)
        {
            Image targetImage = GetPortraitImage(position);
            if (targetImage == null) return;

            currentActivePosition = position;

            // 设置立绘图片
            if (portrait != null)
            {
                targetImage.sprite = portrait;
                targetImage.gameObject.SetActive(true);
                targetImage.color = Color.white;

                // 播放动画
                PlayPortraitAnimation(targetImage, animation);

                // 高亮当前说话的立绘，暗化其他立绘
                HighlightActivePortrait(position);
            }
            else
            {
                targetImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 高亮当前说话的立绘
        /// </summary>
        private void HighlightActivePortrait(PortraitPosition activePosition)
        {
            Color activeColor = Color.white;
            Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

            if (leftPortrait != null && leftPortrait.gameObject.activeSelf)
            {
                leftPortrait.color = activePosition == PortraitPosition.Left ? activeColor : inactiveColor;
            }

            if (rightPortrait != null && rightPortrait.gameObject.activeSelf)
            {
                rightPortrait.color = activePosition == PortraitPosition.Right ? activeColor : inactiveColor;
            }

            if (centerPortrait != null && centerPortrait.gameObject.activeSelf)
            {
                centerPortrait.color = activePosition == PortraitPosition.Center ? activeColor : inactiveColor;
            }
        }

        /// <summary>
        /// 隐藏对方立绘
        /// </summary>
        public void HideOtherPortrait(PortraitPosition currentPosition)
        {
            if (currentPosition != PortraitPosition.Left && leftPortrait != null)
            {
                StartCoroutine(FadeOutPortrait(leftPortrait));
            }

            if (currentPosition != PortraitPosition.Right && rightPortrait != null)
            {
                StartCoroutine(FadeOutPortrait(rightPortrait));
            }

            if (currentPosition != PortraitPosition.Center && centerPortrait != null)
            {
                StartCoroutine(FadeOutPortrait(centerPortrait));
            }
        }

        /// <summary>
        /// 隐藏所有立绘
        /// </summary>
        public void HideAllPortraits()
        {
            if (leftPortrait != null) leftPortrait.gameObject.SetActive(false);
            if (rightPortrait != null) rightPortrait.gameObject.SetActive(false);
            if (centerPortrait != null) centerPortrait.gameObject.SetActive(false);
        }

        private IEnumerator FadeOutPortrait(Image portrait)
        {
            if (portrait == null || !portrait.gameObject.activeSelf) yield break;

            float elapsed = 0;
            Color startColor = portrait.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

            while (elapsed < portraitFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                portrait.color = Color.Lerp(startColor, endColor, elapsed / portraitFadeDuration);
                yield return null;
            }

            portrait.gameObject.SetActive(false);
            portrait.color = Color.white;
        }

        /// <summary>
        /// 播放立绘动画
        /// </summary>
        private void PlayPortraitAnimation(Image portrait, PortraitAnimation animation)
        {
            if (portraitAnimCoroutine != null)
            {
                StopCoroutine(portraitAnimCoroutine);
            }

            switch (animation)
            {
                case PortraitAnimation.FadeIn:
                    portraitAnimCoroutine = StartCoroutine(FadeInPortrait(portrait));
                    break;
                case PortraitAnimation.Shake:
                    portraitAnimCoroutine = StartCoroutine(ShakePortrait(portrait));
                    break;
                case PortraitAnimation.Bounce:
                    portraitAnimCoroutine = StartCoroutine(BouncePortrait(portrait));
                    break;
            }
        }

        private IEnumerator FadeInPortrait(Image portrait)
        {
            float elapsed = 0;
            Color startColor = new Color(1, 1, 1, 0);
            Color endColor = Color.white;

            portrait.color = startColor;

            while (elapsed < portraitFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                portrait.color = Color.Lerp(startColor, endColor, elapsed / portraitFadeDuration);
                yield return null;
            }

            portrait.color = endColor;
        }

        private IEnumerator ShakePortrait(Image portrait)
        {
            RectTransform rect = portrait.rectTransform;
            Vector3 originalPos = rect.localPosition;
            float elapsed = 0;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float x = originalPos.x + Random.Range(-shakeIntensity, shakeIntensity);
                float y = originalPos.y + Random.Range(-shakeIntensity, shakeIntensity);
                rect.localPosition = new Vector3(x, y, originalPos.z);
                yield return null;
            }

            rect.localPosition = originalPos;
        }

        private IEnumerator BouncePortrait(Image portrait)
        {
            RectTransform rect = portrait.rectTransform;
            Vector3 originalPos = rect.localPosition;
            float elapsed = 0;

            while (elapsed < bounceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / bounceDuration;
                float bounce = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
                rect.localPosition = new Vector3(originalPos.x, originalPos.y + bounce, originalPos.z);
                yield return null;
            }

            rect.localPosition = originalPos;
        }

        /// <summary>
        /// 设置背景图片
        /// </summary>
        public void SetBackground(Sprite background)
        {
            if (backgroundImage != null && background != null)
            {
                backgroundImage.sprite = background;
            }
        }

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

        private Image GetPortraitImage(PortraitPosition position)
        {
            switch (position)
            {
                case PortraitPosition.Left:
                    return leftPortrait;
                case PortraitPosition.Right:
                    return rightPortrait;
                case PortraitPosition.Center:
                    return centerPortrait;
                default:
                    return leftPortrait;
            }
        }

        private void OnSkipClicked()
        {
            var player = FindObjectOfType<DialoguePlayer>();
            if (player != null)
            {
                player.SkipDialogue();
            }
        }
    }
}
