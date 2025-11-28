using System.Collections.Generic;
using UnityEngine;

namespace Template.Core
{
	public class RankingScreen : MonoBehaviour
	{
		[SerializeField] private Transform contentParent;
		[SerializeField] private RankingEntryView entryPrefab;
		[SerializeField] private int maxDisplay = 50;
		[SerializeField] private bool clearOnRefresh = true;

		private readonly List<GameObject> spawned = new List<GameObject>();

		private void OnEnable()
		{
			Refresh();
		}

		public void Refresh()
		{
			if (entryPrefab == null || contentParent == null) return;
			if (clearOnRefresh) ClearSpawned();

			var list = RankingManager.Instance?.GetTopScores(maxDisplay);
			if (list == null) return;
			for (int i = 0; i < list.Count; i++)
			{
				var go = Instantiate(entryPrefab.gameObject, contentParent);
				spawned.Add(go);
				var view = go.GetComponent<RankingEntryView>();
				if (view != null)
				{
					view.Bind(i + 1, list[i].score, list[i].name);
				}
			}
		}

		private void ClearSpawned()
		{
			for (int i = 0; i < spawned.Count; i++)
			{
				if (spawned[i] != null) Destroy(spawned[i]);
			}
			spawned.Clear();
		}
	}
}


