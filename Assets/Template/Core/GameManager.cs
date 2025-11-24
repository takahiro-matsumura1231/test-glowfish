using UnityEngine;
using UnityEngine.SceneManagement;
using Template.Gameplay.Controller;
using Template.Core;
using UnityEngine.UI;

namespace Template.Core
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public GameState CurrentState { get; private set; } = GameState.Menu;
        public bool IsClearing { get; private set; } = false;
        [Header("Score")]
        [SerializeField] private int pointsLevel1 = 5;
        [SerializeField] private int pointsLevel2 = 20;
        [SerializeField] private int pointsLevel3 = 50;
        public int CurrentScore { get; private set; } = 0;
        [Header("Game Timer")]
        [SerializeField] private float gameDurationSeconds = 90f;
        private float remainingTimeSeconds = 0f;
        private bool timerRunning = false;
        [Header("Win Visual Snapshot")]
        public Sprite FinalFishSprite { get; private set; }
        public Vector2 FinalFishSize { get; private set; }

        protected override void SingletonStarted()
        {
            SetState(GameState.Menu);
        }

        public void StartGame()
        {
            GameplayInitializer.Instance.InitializeForGameStart();
            ResetScore();
            IsClearing = false;
            FinalFishSprite = null;
            FinalFishSize = Vector2.zero;
            remainingTimeSeconds = Mathf.Max(0f, gameDurationSeconds);
            timerRunning = true;
            EventBus.OnTimeChanged?.Invoke(remainingTimeSeconds);
            SetState(GameState.Game);
        }

        public void WinGame()
        {
            CaptureFinalFishVisual();
            timerRunning = false;
            EnemySpawner.Instance.StopSpawning();
            SetState(GameState.Win);
        }

        public void LoseGame()
        {
            timerRunning = false;
            EnemySpawner.Instance.StopSpawning();
            SetState(GameState.Lose);
        }

        public void GoToMenu()
        {
            timerRunning = false;
            EnemySpawner.Instance.StopSpawning();
            IsClearing = false;
            SetState(GameState.Menu);
        }

        public void ResetGame()
        {
            timerRunning = false;
            EnemySpawner.Instance.StopSpawning();
            IsClearing = false;
            SetState(GameState.Menu);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void SetState(GameState next)
        {
            if (CurrentState == next) return;
            CurrentState = next;
            EventBus.OnGameStateChanged?.Invoke(next);
        }

        public void StopGame()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (CurrentState != GameState.Game || !timerRunning) return;

            remainingTimeSeconds -= Time.deltaTime;
            float clamped = Mathf.Max(0f, remainingTimeSeconds);
            EventBus.OnTimeChanged?.Invoke(clamped);

            if (remainingTimeSeconds <= 0f)
            {
                timerRunning = false;
                // Stop new spawns and notify time up; overlay animation will run on top of Game, then transition to Win.
                EnemySpawner.Instance.StopSpawning();
                IsClearing = true;
                EventBus.OnGameTimeExpired?.Invoke();
            }
        }

        public void AddScoreForEnemyLevel(int enemyLevel)
        {
            int add = GetPointsForLevel(enemyLevel);
            if (add <= 0) return;
            CurrentScore += add;
            if (CurrentScore < 0) CurrentScore = 0;
            EventBus.OnScoreChanged?.Invoke(CurrentScore);
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            EventBus.OnScoreChanged?.Invoke(CurrentScore);
        }

        private int GetPointsForLevel(int level)
        {
            if (level <= 1) return pointsLevel1;
            if (level == 2) return pointsLevel2;
            return pointsLevel3; // 3 or higher
        }

        private void CaptureFinalFishVisual()
        {
            // Try to get the player's current fish sprite/size
            var fish = Object.FindObjectOfType<FishController>();
            if (fish != null && fish.TryGetCurrentVisual(out Sprite sprite, out Vector2 size))
            {
                FinalFishSprite = sprite;
                FinalFishSize = size;
            }
        }
    }
}


