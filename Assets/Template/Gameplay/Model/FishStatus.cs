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

        /// <summary>
        /// 現在のレベルアップ進捗を0.0～1.0で返す
        /// </summary>
        public float GetProgress01()
        {
            if (level == 1)
            {
                return Mathf.Clamp01((float)foodEaten / foodToLevel2);
            }
            else if (level == 2)
            {
                int progress = foodEaten - foodToLevel2;
                int required = foodToLevel3 - foodToLevel2;
                return Mathf.Clamp01((float)progress / required);
            }
            else // level == 3
            {
                return 1f; // 最大レベル
            }
        }

        /// <summary>
        /// 次のレベルアップまでに必要な残りの食べ物の数を返す。最大レベルの場合は-1を返す。
        /// </summary>
        public int GetRemainingFoodCount()
        {
            if (level == 1)
            {
                return Mathf.Max(0, foodToLevel2 - foodEaten);
            }
            else if (level == 2)
            {
                return Mathf.Max(0, foodToLevel3 - foodEaten);
            }
            else // level == 3
            {
                return -1; // 最大レベル
            }
        }

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


