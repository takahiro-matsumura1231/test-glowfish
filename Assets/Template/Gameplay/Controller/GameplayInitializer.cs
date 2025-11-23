using System.Linq;
using UnityEngine;
using Template.Core;
using Template.Gameplay.Model;

namespace Template.Gameplay.Controller
{
	public class GameplayInitializer : MonoBehaviourSingleton<GameplayInitializer>
	{
		[SerializeField] private RectTransform fishStartParent;
		[SerializeField] private Vector2 fishStartAnchoredPosition = Vector2.zero;
		[SerializeField] private int playerStartLevel = 1;
		[SerializeField] private RectTransform enemiesParent;
        [SerializeField] private FishController fish;

		public void InitializeForGameStart()
		{
			ResetPlayer();
			DestroyAllEnemies();
            EnemySpawner.Instance.StartSpawning();
		}

		private void ResetPlayer()
		{
			if (fish == null) return;

			// Position
			Vector2 startPos = fishStartAnchoredPosition;
			if (fishStartParent == null)
			{
				var rect = fish.GetComponent<RectTransform>();
				if (rect != null && rect.parent is RectTransform parentRect)
				{
					// default to center if unspecified
					startPos = Vector2.zero;
				}
			}
			fish.ResetState(startPos, Mathf.Clamp(playerStartLevel, 1, 3));
		}

		private void DestroyAllEnemies()
		{
			if (enemiesParent != null)
			{
				for (int i = enemiesParent.childCount - 1; i >= 0; i--)
				{
					var child = enemiesParent.GetChild(i);
					if (child != null)
					{
						Destroy(child.gameObject);
					}
				}
			}
			else
			{
				var enemies = FindObjectsOfType<EnemyController>();
				for (int i = 0; i < enemies.Length; i++)
				{
					if (enemies[i] != null)
					{
						Destroy(enemies[i].gameObject);
					}
				}
			}
		}
	}
}


