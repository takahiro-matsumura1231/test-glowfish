using System.Collections;
using UnityEngine;
using Template.Gameplay.Model;

namespace Template.Gameplay.Controller
{
	public class EnemySpawner : MonoBehaviourSingleton<EnemySpawner>
	{
		[System.Serializable]
		public struct EnemyVariant
		{
			public EnemyController prefab;
			public Vector2 speedRange;
			public float weight;
			[Header("Level Override (optional)")]
			public bool overrideLevel;
			[Range(1, 3)] public int levelOverride;
		}

		[SerializeField] private RectTransform spawnParent;
		[SerializeField] private EnemyVariant[] variants = new EnemyVariant[3];
		[SerializeField] private bool autoStart = true;
		[SerializeField] private float spawnInterval = 1.5f;
		[SerializeField] private float spawnMarginPixels = 20f;
		[SerializeField] private Vector2 verticalPadding = new Vector2(10f, 10f);

		private Coroutine spawnRoutine;

		private void Awake()
		{
			// Provide sensible defaults if empty (user can override in Inspector)
			if (variants == null || variants.Length == 0)
			{
				variants = new EnemyVariant[3];
			}
			for (int i = 0; i < variants.Length; i++)
			{
				if (variants[i].weight <= 0f) variants[i].weight = 1f;
				if (variants[i].speedRange == Vector2.zero)
				{
					// Slightly different defaults per slot
					if (i == 0) variants[i].speedRange = new Vector2(150f, 250f);
					else if (i == 1) variants[i].speedRange = new Vector2(200f, 320f);
					else variants[i].speedRange = new Vector2(260f, 380f);
				}
				if (variants[i].levelOverride < 1) variants[i].levelOverride = 1;
			}

			if (spawnParent == null)
			{
				spawnParent = GetComponent<RectTransform>();
				if (spawnParent == null && transform.parent != null)
					spawnParent = transform.parent as RectTransform;
			}
		}

		private void OnEnable()
		{
			if (autoStart) StartSpawning();
		}

		private void OnDisable()
		{
			StopSpawning();
		}

		public void StartSpawning()
		{
			if (spawnRoutine != null || !HasAnyPrefab() || spawnParent == null) return;
			spawnRoutine = StartCoroutine(SpawnLoop());
		}

		public void StopSpawning()
		{
			if (spawnRoutine == null) return;
			StopCoroutine(spawnRoutine);
			spawnRoutine = null;
		}

		public EnemyController SpawnImmediate(EnemyController.HorizontalDirection direction, float speed)
		{
			if (spawnParent == null) return null;
			int variantIndex = GetRandomVariantIndex();
			if (variantIndex < 0) return null;
			return SpawnOne(variantIndex, direction, speed);
		}

		public EnemyController SpawnImmediateOfType(int variantIndex, EnemyController.HorizontalDirection direction, float speed)
		{
			if (spawnParent == null) return null;
			if (!IsValidVariantIndex(variantIndex)) return null;
			return SpawnOne(variantIndex, direction, speed);
		}

		private IEnumerator SpawnLoop()
		{
			var wait = new WaitForSeconds(spawnInterval);
			while (true)
			{
				int variantIndex = GetRandomVariantIndex();
				if (variantIndex < 0) yield return wait;
				var dir = (Random.value < 0.5f)
					? EnemyController.HorizontalDirection.LeftToRight
					: EnemyController.HorizontalDirection.RightToLeft;
				Vector2 sr = variants[variantIndex].speedRange;
				float speed = Random.Range(sr.x, sr.y);
				SpawnOne(variantIndex, dir, speed);
				yield return wait;
			}
		}

		private EnemyController SpawnOne(int variantIndex, EnemyController.HorizontalDirection direction, float speed)
		{
			var variant = variants[variantIndex];
			if (variant.prefab == null) return null;
			EnemyController enemy = Instantiate(variant.prefab, spawnParent);
			RectTransform enemyRect = enemy.GetComponent<RectTransform>();
			RectTransform parentRect = spawnParent;

			Vector2 parentHalf = parentRect.rect.size * 0.5f;
			Vector2 enemyHalf = (enemyRect != null) ? enemyRect.rect.size * 0.5f : new Vector2(32f, 32f);

			float startX = (direction == EnemyController.HorizontalDirection.LeftToRight)
				? -parentHalf.x - enemyHalf.x - spawnMarginPixels
				: parentHalf.x + enemyHalf.x + spawnMarginPixels;

			float minY = -parentHalf.y + enemyHalf.y + verticalPadding.x;
			float maxY = parentHalf.y - enemyHalf.y - verticalPadding.y;
			float startY = Mathf.Clamp(Random.Range(minY, maxY), minY, maxY);

			if (enemyRect != null)
			{
				enemyRect.anchoredPosition = new Vector2(startX, startY);
				// EnemyStatus の決定（Prefabのレベルを使用 or Variantで上書き）
				EnemyStatus status = enemy.GetComponent<EnemyStatus>();
				int levelForScale = 1;
				if (status != null)
				{
					if (variant.overrideLevel)
					{
						int lvl = Mathf.Clamp(variant.levelOverride, 1, 3);
						status.SetLevel(lvl);
						levelForScale = lvl;
					}
					else
					{
						// Prefabに設定されたレベルを使用
						levelForScale = Mathf.Clamp(status.Level, 1, 3);
					}
				}
				// レベルに応じたスケール適用（1/2/3）
				float scale = (levelForScale == 1) ? 0.9f : (levelForScale == 2) ? 1.0f : 1.25f;
				Vector3 s = enemyRect.localScale;
				float signX = Mathf.Sign(s.x);
				enemyRect.localScale = new Vector3(Mathf.Abs(s.x) * scale * signX, s.y * scale, s.z);
			}

			enemy.Setup(direction, speed);
			return enemy;
		}

		private bool HasAnyPrefab()
		{
			if (variants == null || variants.Length == 0) return false;
			for (int i = 0; i < variants.Length; i++)
			{
				if (variants[i].prefab != null) return true;
			}
			return false;
		}

		private bool IsValidVariantIndex(int index)
		{
			return variants != null && index >= 0 && index < variants.Length && variants[index].prefab != null;
		}

		private int GetRandomVariantIndex()
		{
			if (variants == null || variants.Length == 0) return -1;
			float total = 0f;
			for (int i = 0; i < variants.Length; i++)
			{
				if (variants[i].prefab == null) continue;
				total += Mathf.Max(0f, variants[i].weight);
			}
			if (total <= 0f) return -1;
			float r = Random.value * total;
			for (int i = 0; i < variants.Length; i++)
			{
				if (variants[i].prefab == null) continue;
				float w = Mathf.Max(0f, variants[i].weight);
				if (r < w) return i;
				r -= w;
			}
			// Fallback
			for (int i = 0; i < variants.Length; i++)
			{
				if (variants[i].prefab != null) return i;
			}
			return -1;
		}
	}
}


