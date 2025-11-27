using System;
using UnityEngine;

namespace Template.Core
{
    public static class EventBus
    {
        public static Action<int> OnScoreChanged;
        public static Action OnFishGrow;
        public static Action<GameState> OnGameStateChanged;
        public static Action<float> OnTimeChanged;
        public static Action OnGameTimeExpired;
		public static Action<Vector3, int> OnEnemyEaten;
    }
}


