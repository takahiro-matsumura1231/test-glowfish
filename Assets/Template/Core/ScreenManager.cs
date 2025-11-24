using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

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

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleStateChanged;
            EventBus.OnTimeChanged += HandleTimeChanged;
            EventBus.OnScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleStateChanged;
            EventBus.OnTimeChanged -= HandleTimeChanged;
            EventBus.OnScoreChanged -= HandleScoreChanged;
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
    }
}



