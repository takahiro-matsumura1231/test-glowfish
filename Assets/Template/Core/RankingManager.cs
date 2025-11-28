using System;
using System.Collections.Generic;
using UnityEngine;

namespace Template.Core
{
	[Serializable]
	public class ScoreEntry
	{
		public int score;
		public string name;
		public long unixTime;
	}

	[Serializable]
	public class ScoreData
	{
		public List<ScoreEntry> entries = new List<ScoreEntry>();
	}

	public class RankingManager : MonoBehaviourSingleton<RankingManager>
	{
		[SerializeField] private string playerName = "Player";
		[SerializeField] private int maxEntriesToKeep = 100;
		[SerializeField] private string prefsKey = "RANKING_DATA";

		private ScoreData cached;

		public string PlayerName
		{
			get => playerName;
			set => playerName = string.IsNullOrEmpty(value) ? "Player" : value;
		}

		protected override void SingletonAwakened()
		{
			_ = Load();
		}

		public void RecordWinScore(int score)
		{
			AddScore(score, playerName);
		}

		public void AddScore(int score, string name)
		{
			if (score < 0) score = 0;
			if (string.IsNullOrEmpty(name)) name = "Player";
			var data = Load();
			if (data == null) data = new ScoreData();
			var entry = new ScoreEntry
			{
				score = score,
				name = name,
				unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};
			data.entries.Add(entry);
			data.entries.Sort((a, b) =>
			{
				int cmp = b.score.CompareTo(a.score);
				if (cmp != 0) return cmp;
				return a.unixTime.CompareTo(b.unixTime); // older first if tie
			});
			if (maxEntriesToKeep > 0 && data.entries.Count > maxEntriesToKeep)
			{
				data.entries.RemoveRange(maxEntriesToKeep, data.entries.Count - maxEntriesToKeep);
			}
			Save(data);
		}

		public IReadOnlyList<ScoreEntry> GetAllScores()
		{
			return Load()?.entries ?? new List<ScoreEntry>();
		}

		public IReadOnlyList<ScoreEntry> GetTopScores(int count)
		{
			var list = Load()?.entries ?? new List<ScoreEntry>();
			int take = Mathf.Clamp(count, 0, list.Count);
			return list.GetRange(0, take);
		}

		private ScoreData Load()
		{
			if (cached != null) return cached;
			string json = PlayerPrefs.GetString(prefsKey, string.Empty);
			if (string.IsNullOrEmpty(json))
			{
				cached = new ScoreData();
			}
			else
			{
				try
				{
					cached = JsonUtility.FromJson<ScoreData>(json);
					if (cached == null) cached = new ScoreData();
				}
				catch
				{
					cached = new ScoreData();
				}
			}
			return cached;
		}

		private void Save(ScoreData data)
		{
			cached = data ?? new ScoreData();
			string json = JsonUtility.ToJson(cached);
			PlayerPrefs.SetString(prefsKey, json);
			PlayerPrefs.Save();
		}
	}
}


