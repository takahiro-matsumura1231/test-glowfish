using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace Template.Core
{
    public class ScreenManager : MonoBehaviourSingleton<ScreenManager>
    {
        [Header("Screens")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private GameObject gameRoot;
        [SerializeField] private GameObject winRoot;
        [SerializeField] private GameObject loseRoot;

        [Header("Popup")]
        [SerializeField] private GameObject settingsPopup;

        [Header("Components")]
        [SerializeField] private GameObject ReStartButton;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text gameScoreText;
        [SerializeField] private TMP_Text winScoreText;
        [Header("Win UI")]
        [SerializeField] private Image winFishImage;
        [Header("Game Overlay (Time Up)")]
        [SerializeField] private RectTransform overlayRoot;   // under gameRoot
        [SerializeField] private Image overlayCircleImage;    // anchored top-left
        [SerializeField] private TMP_Text timeUpText;         // centered
        [Header("Overlay Durations")]
        [SerializeField] private float circleExpandDuration = 1.0f;
        [SerializeField] private float timeUpFadeDuration = 1.0f;
        [SerializeField] private float timeUpHoldDuration = 1.0f; // requested 1 second
        [SerializeField] private float resultsFadeDuration = 1.0f;
        [Header("Win Results Group (fade after transition)")]
        [SerializeField] private CanvasGroup winResultsGroup;

        private Sequence overlaySequence;
        private Sequence resultsSequence;

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleStateChanged;
            EventBus.OnTimeChanged += HandleTimeChanged;
            EventBus.OnScoreChanged += HandleScoreChanged;
            EventBus.OnGameTimeExpired += HandleGameTimeExpired;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleStateChanged;
            EventBus.OnTimeChanged -= HandleTimeChanged;
            EventBus.OnScoreChanged -= HandleScoreChanged;
            EventBus.OnGameTimeExpired -= HandleGameTimeExpired;
            if (overlaySequence != null && overlaySequence.IsActive()) overlaySequence.Kill(true);
            if (resultsSequence != null && resultsSequence.IsActive()) resultsSequence.Kill(true);
        }

        private void Start()
        {
            var manager = GameManager.Instance;
            if (manager != null)
            {
                HandleStateChanged(manager.CurrentState);
                HandleScoreChanged(manager.CurrentScore);
                // If already in Win state (debug), ensure image shows
                if (manager.CurrentState == GameState.Win)
                {
                    ApplyWinFishImage(manager);
                }
            }
        }

        private void HandleStateChanged(GameState state)
        {
            SetActiveSafe(menuRoot, state == GameState.Menu);
            SetActiveSafe(gameRoot, state == GameState.Game);
            SetActiveSafe(winRoot, state == GameState.Win);
            SetActiveSafe(loseRoot, state == GameState.Lose);
            // settingsPopup is independent; do not change here

            UpdateComponents(state);
            // Ensure win screen shows latest score
            if (state == GameState.Win)
            {
                var manager = GameManager.Instance;
                if (manager != null) HandleScoreChanged(manager.CurrentScore);
                if (manager != null) ApplyWinFishImage(manager);
                PlayResultsFade();
            }
            else if (state == GameState.Game)
            {
                // Reset overlay visuals when re-entering Game
                ResetOverlay();
            }
        }
        
        private void UpdateComponents(GameState state)
        {
            if (ReStartButton != null)
            {
                bool shouldBeActive = (state != GameState.Menu);
                SetActiveSafe(ReStartButton, shouldBeActive);
            }
        }

        private void ApplyWinFishImage(GameManager manager)
        {
            if (winFishImage == null || manager == null) return;
            winFishImage.sprite = manager.FinalFishSprite;
            if (manager.FinalFishSprite != null)
            {
                winFishImage.preserveAspect = true;
                // Optionally mirror the in-game size if provided
                if (manager.FinalFishSize != Vector2.zero)
                {
                    var rect = winFishImage.rectTransform;
                    if (rect != null) rect.sizeDelta = manager.FinalFishSize;
                }
                winFishImage.gameObject.SetActive(true);
            }
            else
            {
                winFishImage.gameObject.SetActive(false);
            }
        }

        private static void SetActiveSafe(GameObject obj, bool active)
        {
            if (obj != null && obj.activeSelf != active)
            {
                obj.SetActive(active);
            }
        }

        // Settings popup controls
        public void OpenSettings()
        {
            SetActiveSafe(settingsPopup, true);
        }

        public void CloseSettings()
        {
            SetActiveSafe(settingsPopup, false);
        }

        public void ToggleSettings()
        {
            if (settingsPopup == null) return;
            settingsPopup.SetActive(!settingsPopup.activeSelf);
        }

        private void HandleTimeChanged(float remainingSeconds)
        {
            if (timerText == null) return;
            int total = Mathf.CeilToInt(remainingSeconds);
            int minutes = total / 60;
            int seconds = total % 60;
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void HandleScoreChanged(int score)
        {
            if (gameScoreText != null)
            {
                gameScoreText.text = score.ToString();
            }
            if (winScoreText != null)
            {
                winScoreText.text = score.ToString();
            }
        }

        public void HandleGameTimeExpired()
        {
            PrepareOverlay();
            PlayTimeUpSequence();
        }

        private void PrepareOverlay()
        {
            // Ensure overlay is visible
            if (overlayRoot != null)
            {
                overlayRoot.gameObject.SetActive(true);
            }

            // Circle collapsed and invisible
            if (overlayCircleImage != null)
            {
                var circleRect = overlayCircleImage.rectTransform;
                if (circleRect != null)
                {
                    // Anchor and pivot at top-left so growth moves toward bottom-right
                    circleRect.anchorMin = new Vector2(0f, 1f);
                    circleRect.anchorMax = new Vector2(0f, 1f);
                    circleRect.pivot = new Vector2(0f, 1f);
                    circleRect.anchoredPosition = Vector2.zero;
                    circleRect.sizeDelta = Vector2.zero;
                }
                var cg = overlayCircleImage.GetComponent<CanvasGroup>();
                if (cg == null) cg = overlayCircleImage.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                overlayCircleImage.gameObject.SetActive(true);
            }

            // TIMEUP text hidden
            if (timeUpText != null)
            {
                var cg = timeUpText.GetComponent<CanvasGroup>();
                if (cg == null) cg = timeUpText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                var tr = timeUpText.rectTransform;
                if (tr != null) tr.localScale = Vector3.one * 0.9f;
                timeUpText.gameObject.SetActive(true);
            }
        }

        private void PlayTimeUpSequence()
        {
            if (overlaySequence != null && overlaySequence.IsActive())
            {
                overlaySequence.Kill(true);
            }

            RectTransform circleRect = (overlayCircleImage != null) ? overlayCircleImage.rectTransform : null;
            RectTransform parentRect = (overlayRoot != null) ? overlayRoot : (overlayCircleImage != null ? overlayCircleImage.transform.parent as RectTransform : null);
            Vector2 parentSize = (parentRect != null) ? parentRect.rect.size : new Vector2(Screen.width, Screen.height);
            // Circle grows from the top-left corner, so diameter must be at least twice the diagonal to fully cover.
            float diag = Mathf.Sqrt(parentSize.x * parentSize.x + parentSize.y * parentSize.y);
            float targetDiameter = diag * 1.7f; // safety margin

            var circleCg = (overlayCircleImage != null)
                ? (overlayCircleImage.GetComponent<CanvasGroup>() ?? overlayCircleImage.gameObject.AddComponent<CanvasGroup>())
                : null;
            var timeUpCg = (timeUpText != null)
                ? (timeUpText.GetComponent<CanvasGroup>() ?? timeUpText.gameObject.AddComponent<CanvasGroup>())
                : null;

            if (circleRect != null) circleRect.sizeDelta = Vector2.zero;
            if (circleCg != null) circleCg.alpha = 0f;
            if (timeUpCg != null) timeUpCg.alpha = 0f;
            if (timeUpText != null && timeUpText.rectTransform != null) timeUpText.rectTransform.localScale = Vector3.one * 0.9f;

            overlaySequence = DOTween.Sequence();
            // 1) Circle expand + fade in
            if (circleRect != null)
            {
                overlaySequence.Append(circleRect.DOSizeDelta(new Vector2(targetDiameter, targetDiameter), circleExpandDuration).SetEase(Ease.OutQuad));
                if (circleCg != null)
                {
                    overlaySequence.Join(circleCg.DOFade(1f, circleExpandDuration));
                }
            }
            // 2) TIMEUP fade + scale
            if (timeUpCg != null && timeUpText != null)
            {
                overlaySequence.Append(timeUpCg.DOFade(1f, timeUpFadeDuration).SetEase(Ease.OutQuad));
                overlaySequence.Join(timeUpText.rectTransform.DOScale(1f, timeUpFadeDuration).SetEase(Ease.OutQuad));
            }
            // 3) Hold
            overlaySequence.AppendInterval(timeUpHoldDuration);
            // 4) TIMEUP fade out
            if (timeUpCg != null)
            {
                overlaySequence.Append(timeUpCg.DOFade(0f, 1.0f));
            }
            // On complete → move to Win
            overlaySequence.OnComplete(() =>
            {
                // Hide overlay elements before switching state to avoid leftover visuals on next play
                ResetOverlay();
                var gm = GameManager.Instance;
                if (gm != null) gm.WinGame();
            });
        }

        private void PlayResultsFade()
        {
            if (winResultsGroup == null) return;
            if (resultsSequence != null && resultsSequence.IsActive())
            {
                resultsSequence.Kill(true);
            }
            winResultsGroup.gameObject.SetActive(true);
            winResultsGroup.interactable = false;
            winResultsGroup.blocksRaycasts = false;
            winResultsGroup.alpha = 0f;
            resultsSequence = DOTween.Sequence();
            resultsSequence.Append(winResultsGroup.DOFade(1f, resultsFadeDuration));
            resultsSequence.OnComplete(() =>
            {
                winResultsGroup.alpha = 1f;
                winResultsGroup.interactable = true;
                winResultsGroup.blocksRaycasts = true;
            });
        }

        private void ResetOverlay()
        {
            if (overlayCircleImage != null)
            {
                var circleRect = overlayCircleImage.rectTransform;
                if (circleRect != null) circleRect.sizeDelta = Vector2.zero;
                var cg = overlayCircleImage.GetComponent<CanvasGroup>();
                if (cg == null) cg = overlayCircleImage.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }
            if (timeUpText != null)
            {
                var cg = timeUpText.GetComponent<CanvasGroup>();
                if (cg == null) cg = timeUpText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                if (timeUpText.rectTransform != null) timeUpText.rectTransform.localScale = Vector3.one * 0.9f;
            }
            if (overlayRoot != null) overlayRoot.gameObject.SetActive(false);
        }
    }
}



