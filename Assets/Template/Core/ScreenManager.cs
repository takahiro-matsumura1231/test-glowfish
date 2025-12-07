using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Template.Gameplay.Controller;
using Template.Gameplay.Model;

namespace Template.Core
{
    public class ScreenManager : MonoBehaviourSingleton<ScreenManager>
    {
        [Header("Screens")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private GameObject gameRoot;
        [SerializeField] private GameObject winRoot;
        [SerializeField] private GameObject loseRoot;
        [SerializeField] private GameObject RankingsRoot;
		[SerializeField] private GameObject nameEntryRoot;
		[SerializeField] private TMP_InputField playerNameInput;

        [Header("Popup")]
        [SerializeField] private GameObject settingsPopup;

        [Header("Components")]
        [SerializeField] private GameObject ReStartButton;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text gameScoreText;
        [SerializeField] private TMP_Text winScoreText;
        [Header("Win UI")]
        [SerializeField] private Image winFishImage;
		[Header("Lose UI")]
		[SerializeField] private Image loseFishImage;
		[SerializeField] private Material grayscaleUIMaterial;
		[Header("Game HUD")]
		[SerializeField] private TMP_Text gamePlayerNameText;
		[Header("Level Progress Bar")]
		[SerializeField] private RectTransform levelProgressFillBarMask; // Mask ObjectのRectTransform
		[SerializeField] private TMP_Text levelProgressText;
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
        private FishController fishController;
        private FishStatus fishStatus;

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleStateChanged;
            EventBus.OnTimeChanged += HandleTimeChanged;
            EventBus.OnScoreChanged += HandleScoreChanged;
            EventBus.OnGameTimeExpired += HandleGameTimeExpired;
        }

        private void Update()
        {
            UpdateLevelProgressBar();
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
            SetActiveSafe(RankingsRoot, state == GameState.Rankings);
			SetActiveSafe(nameEntryRoot, state == GameState.NameEntry);
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
			else if (state == GameState.Lose)
			{
				var manager = GameManager.Instance;
				if (manager != null) ApplyLoseFishImage(manager);
			}
            else if (state == GameState.Game)
            {
                // Reset overlay visuals when re-entering Game
                ResetOverlay();
				// Show player name on HUD
				if (gamePlayerNameText != null)
				{
					gamePlayerNameText.text = RankingManager.Instance != null ? RankingManager.Instance.PlayerName : "Guest";
				}
                // Find FishController for level progress tracking
                FindFishController();
            }
			else if (state == GameState.Rankings)
			{
				// Ensure ranking refresh each time we open the rankings screen
				if (RankingsRoot != null)
				{
					var screen = RankingsRoot.GetComponentInChildren<RankingScreen>(true);
					if (screen != null) screen.Refresh();
				}
			}
        }
		
		// Name Entry controls
		public void OpenNameEntry()
		{
			GameManager.Instance?.GoToNameEntry();
		}
		
		public void ConfirmPlayerNameAndStart()
		{
			string name = (playerNameInput != null) ? playerNameInput.text : null;
			if (string.IsNullOrWhiteSpace(name)) name = "Guest";
			RankingManager.Instance.PlayerName = name.Trim();
			GameManager.Instance?.StartGame();
		}
        
        private void UpdateComponents(GameState state)
        {
            if (ReStartButton != null)
            {
                bool shouldBeActive = (state != GameState.Menu);
                SetActiveSafe(ReStartButton, shouldBeActive);
            }
        }

		private void ApplyLoseFishImage(GameManager manager)
		{
			if (loseFishImage == null || manager == null) return;
			loseFishImage.sprite = manager.FinalFishSprite;
			if (manager.FinalFishSprite != null)
			{
				loseFishImage.preserveAspect = true;
				// Mirror in-game size if available
				if (manager.FinalFishSize != Vector2.zero)
				{
					var rect = loseFishImage.rectTransform;
					if (rect != null) rect.sizeDelta = manager.FinalFishSize;
				}
				// Apply grayscale material (per-instance)
				if (grayscaleUIMaterial != null)
				{
					loseFishImage.material = grayscaleUIMaterial;
				}
				loseFishImage.gameObject.SetActive(true);
			}
			else
			{
				loseFishImage.gameObject.SetActive(false);
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

            // Circle collapsed (no fade)
            if (overlayCircleImage != null)
            {
                var circleRect = overlayCircleImage.rectTransform;
                if (circleRect != null)
                {
                    DOTween.Kill(circleRect);
                    // Anchor and pivot at top-left so growth moves toward bottom-right
                    circleRect.anchorMin = new Vector2(0f, 1f);
                    circleRect.anchorMax = new Vector2(0f, 1f);
                    circleRect.pivot = new Vector2(0f, 1f);
                    circleRect.anchoredPosition = Vector2.zero;
                    circleRect.sizeDelta = Vector2.zero;
                }
                // Start fully transparent, will fade in while expanding
                DOTween.Kill(overlayCircleImage);
                Color c = overlayCircleImage.color;
                c.a = 0f;
                overlayCircleImage.color = c;
                overlayCircleImage.gameObject.SetActive(true);
            }

            // TIMEUP text prepare (no fade)
            if (timeUpText != null)
            {
                var tr = timeUpText.rectTransform;
                if (tr != null)
                {
                    DOTween.Kill(tr);
                    tr.localScale = Vector3.one * 0.9f;
                }
                // hide until the TIMEUP step begins to ensure identical animation each play
                timeUpText.gameObject.SetActive(false);
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

            if (circleRect != null) circleRect.sizeDelta = Vector2.zero;
            if (timeUpText != null && timeUpText.rectTransform != null) timeUpText.rectTransform.localScale = Vector3.one * 0.9f;

            overlaySequence = DOTween.Sequence();
            // 1) Circle expand + fade in
            if (circleRect != null)
            {
                overlaySequence.Append(circleRect.DOSizeDelta(new Vector2(targetDiameter, targetDiameter), circleExpandDuration).SetEase(Ease.OutQuad));
                if (overlayCircleImage != null)
                {
                    overlaySequence.Join(overlayCircleImage.DOFade(1f, circleExpandDuration).SetEase(Ease.OutQuad));
                }
            }
            // 2) TIMEUP fade + scale
            if (timeUpText != null)
            {
                overlaySequence.AppendCallback(() =>
                {
                    if (timeUpText != null) timeUpText.gameObject.SetActive(true);
                });
                overlaySequence.Join(timeUpText.rectTransform.DOScale(1f, timeUpFadeDuration).SetEase(Ease.OutQuad));
            }
            // 3) Hold
            overlaySequence.AppendInterval(timeUpHoldDuration);
            // 4) No TIMEUP fade out (kept visible until Win transition)
            // On complete → move to Win
            overlaySequence.OnComplete(() =>
            {
                // Do not hide overlay yet; Win will crossfade in while overlay fades out.
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
                // Now that Win is fully visible, turn off the overlay canvas
                ResetOverlay();
            });
        }

        private void ResetOverlay()
        {
            if (overlayCircleImage != null)
            {
                var circleRect = overlayCircleImage.rectTransform;
                if (circleRect != null)
                {
                    DOTween.Kill(circleRect);
                    circleRect.sizeDelta = Vector2.zero;
                }
            }
            if (timeUpText != null)
            {
                if (timeUpText.rectTransform != null)
                {
                    DOTween.Kill(timeUpText.rectTransform);
                    timeUpText.rectTransform.localScale = Vector3.one * 0.9f;
                }
                timeUpText.gameObject.SetActive(false);
            }
            if (overlayRoot != null) overlayRoot.gameObject.SetActive(false);
        }

        private void HideOverlayVisuals()
        {
            if (overlayCircleImage != null)
            {
                var circleRect = overlayCircleImage.rectTransform;
                if (circleRect != null)
                {
                    DOTween.Kill(circleRect);
                    circleRect.sizeDelta = Vector2.zero;
                }
            }
            if (timeUpText != null)
            {
                if (timeUpText.rectTransform != null)
                {
                    DOTween.Kill(timeUpText.rectTransform);
                    timeUpText.rectTransform.localScale = Vector3.one * 0.9f;
                }
            }
            // Keep overlayRoot active until Win fully fades in
        }

        private void FindFishController()
        {
            if (fishController == null)
            {
                fishController = FindObjectOfType<FishController>();
                if (fishController != null)
                {
                    fishStatus = fishController.GetComponent<FishStatus>();
                }
            }
        }

        private void UpdateLevelProgressBar()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Game)
            {
                // ゲーム中でない場合は非表示または0にリセット
                if (levelProgressFillBarMask != null)
                {
                    Vector2 anchorMin = levelProgressFillBarMask.anchorMin;
                    Vector2 anchorMax = levelProgressFillBarMask.anchorMax;
                    if (anchorMax.x > 0f)
                    {
                        anchorMax.x = 0f;
                        levelProgressFillBarMask.anchorMax = anchorMax;
                    }
                }
                if (levelProgressText != null)
                {
                    levelProgressText.text = "";
                }
                return;
            }

            // FishControllerが見つかっていない場合は検索
            if (fishController == null || fishStatus == null)
            {
                FindFishController();
            }

            if (fishStatus != null)
            {
                // Fill Barの更新（anchorMax.xを進捗に合わせて更新）
                if (levelProgressFillBarMask != null)
                {
                    float progress = fishStatus.GetProgress01();
                    Vector2 anchorMin = levelProgressFillBarMask.anchorMin;
                    Vector2 anchorMax = levelProgressFillBarMask.anchorMax;
                    anchorMax.x = progress; // Rightを進捗の割合に設定
                    levelProgressFillBarMask.anchorMax = anchorMax;
                }

                // 残り匹数のテキスト更新
                if (levelProgressText != null)
                {
                    int remaining = fishStatus.GetRemainingFoodCount();
                    if (remaining == -1)
                    {
                        levelProgressText.text = "MAX";
                    }
                    else if (remaining == 0)
                    {
                        levelProgressText.text = "0";
                    }
                    else
                    {
                        levelProgressText.text = $"{remaining}";
                    }
                }
            }
            else
            {
                // FishStatusが見つからない場合は0にリセット
                if (levelProgressFillBarMask != null)
                {
                    Vector2 anchorMin = levelProgressFillBarMask.anchorMin;
                    Vector2 anchorMax = levelProgressFillBarMask.anchorMax;
                    if (anchorMax.x > 0f)
                    {
                        anchorMax.x = 0f;
                        levelProgressFillBarMask.anchorMax = anchorMax;
                    }
                }
                if (levelProgressText != null)
                {
                    levelProgressText.text = "";
                }
            }
        }
    }
}



