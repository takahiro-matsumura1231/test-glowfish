using UnityEngine;
using System;

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

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            var manager = GameManager.Instance;
            if (manager != null)
            {
                HandleStateChanged(manager.CurrentState);
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
        }
        
        private void UpdateComponents(GameState state)
        {
            if (ReStartButton != null)
            {
                bool shouldBeActive = (state != GameState.Menu);
                SetActiveSafe(ReStartButton, shouldBeActive);
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
    }
}



