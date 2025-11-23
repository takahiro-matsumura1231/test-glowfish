using UnityEngine;

namespace Template.Gameplay.Model
{
    public class FishStatus : MonoBehaviour
    {
        [SerializeField] private int level = 1; // 1..3
        [SerializeField] private int foodToLevel2 = 5;
        [SerializeField] private int foodToLevel3 = 12;

        private int foodEaten = 0;

        public int Level => level;
        public int FoodEaten => foodEaten;
        public event System.Action<int> LevelChanged;

        private void OnValidate()
        {
            level = Mathf.Clamp(level, 1, 3);
            foodToLevel2 = Mathf.Max(1, foodToLevel2);
            foodToLevel3 = Mathf.Max(foodToLevel2 + 1, foodToLevel3);
        }

        public void ResetProgress(int startLevel = 1, bool emitEvent = true)
        {
            int newLevel = Mathf.Clamp(startLevel, 1, 3);
            level = newLevel;
            foodEaten = 0;
            if (emitEvent)
            {
                LevelChanged?.Invoke(level);
            }
        }

        public void AddFood(int amount = 1)
        {
            if (amount <= 0) return;
            foodEaten += amount;
            TryLevelUp();
        }

        private void TryLevelUp()
        {
            int before = level;
            if (level == 1 && foodEaten >= foodToLevel2)
            {
                level = 2;
            }
            if (level == 2 && foodEaten >= foodToLevel3)
            {
                level = 3;
            }
            if (level != before)
            {
                LevelChanged?.Invoke(level);
            }
        }
    }
}


