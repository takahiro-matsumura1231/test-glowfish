using UnityEngine;
using UnityEngine.SceneManagement;
using Template.Gameplay.Controller;

namespace Template.Core
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public GameState CurrentState { get; private set; } = GameState.Menu;

        protected override void SingletonStarted()
        {
            SetState(GameState.Menu);
        }

        public void StartGame()
        {
            GameplayInitializer.Instance.InitializeForGameStart();
            SetState(GameState.Game);
        }

        public void WinGame()
        {
            EnemySpawner.Instance.StopSpawning();
            SetState(GameState.Win);
        }

        public void LoseGame()
        {
            EnemySpawner.Instance.StopSpawning();
            SetState(GameState.Lose);
        }

        public void GoToMenu()
        {
            EnemySpawner.Instance.StopSpawning();
            SetState(GameState.Menu);
        }

        public void ResetGame()
        {
            EnemySpawner.Instance.StopSpawning();
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
    }
}


