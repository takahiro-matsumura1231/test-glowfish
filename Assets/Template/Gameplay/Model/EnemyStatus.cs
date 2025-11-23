using UnityEngine;

namespace Template.Gameplay.Model
{
    public class EnemyStatus : MonoBehaviour
    {
        [SerializeField] private int level = 1; // 1..3

        public int Level => level;

        public void SetLevel(int lvl)
        {
            level = Mathf.Clamp(lvl, 1, 3);
        }

        private void OnValidate()
        {
            level = Mathf.Clamp(level, 1, 3);
        }
    }
}


