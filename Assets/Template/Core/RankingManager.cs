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
		// Player name max length requirement (UI input + ranking data)
		public const int PlayerNameMaxLength = 10;

		[SerializeField] private string playerName = "Guest";
		[SerializeField] private int maxEntriesToKeep = 100;
		[SerializeField] private string prefsKey = "RANKING_DATA";
		[SerializeField] private bool enableDebugLogs = false;

		private ScoreData cached;

		public string PlayerName
		{
			get => playerName;
			set => playerName = SanitizePlayerName(value);
		}

		public static string SanitizePlayerName(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return "Guest";

			// Single-line, trim, and remove control characters/newlines to prevent UI overflow issues.
			string s = raw.Trim();
			s = s.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", " ");

			// Collapse double spaces a little (cheap)
			while (s.Contains("  ")) s = s.Replace("  ", " ");

			// Clamp length (UTF-16 code units; good enough for TMP display constraints)
			if (s.Length > PlayerNameMaxLength) s = s.Substring(0, PlayerNameMaxLength);

			return string.IsNullOrWhiteSpace(s) ? "Guest" : s;
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
			name = SanitizePlayerName(name);
			var data = Load();
			if (data == null) data = new ScoreData();
			// Keep only one entry per name: update if higher, otherwise keep existing
			int existingIndex = data.entries.FindIndex(e => string.Equals(e.name, name, StringComparison.Ordinal));
			if (existingIndex >= 0)
			{
				if (score > data.entries[existingIndex].score)
				{
					data.entries[existingIndex].score = score;
					data.entries[existingIndex].unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
					if (enableDebugLogs) Debug.Log($"[Ranking] Updated score for {name} -> {score}");
				}
				else
				{
					if (enableDebugLogs) Debug.Log($"[Ranking] Kept existing higher/equal score for {name} ({data.entries[existingIndex].score} >= {score})");
				}
			}
			else
			{
				var entry = new ScoreEntry
				{
					score = score,
					name = name,
					unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
				};
				data.entries.Add(entry);
				if (enableDebugLogs) Debug.Log($"[Ranking] Added score={score} name={name}. Total(before sort)={data.entries.Count}");
			}
			data.entries.Sort((a, b) =>
			{
				int cmp = b.score.CompareTo(a.score);
				if (cmp != 0) return cmp;
				return a.unixTime.CompareTo(b.unixTime); // older first if tie
			});
			if (maxEntriesToKeep > 0 && data.entries.Count > maxEntriesToKeep)
			{
				data.entries.RemoveRange(maxEntriesToKeep, data.entries.Count - maxEntriesToKeep);
				if (enableDebugLogs) Debug.Log($"[Ranking] Trimmed to {maxEntriesToKeep}");
			}
			Save(data);
		}

		public IReadOnlyList<ScoreEntry> GetAllScores()
		{
			var d = Load();
			if (enableDebugLogs) Debug.Log($"[Ranking] GetAllScores count={(d?.entries?.Count ?? 0)}");
			return d?.entries ?? new List<ScoreEntry>();
		}

		public IReadOnlyList<ScoreEntry> GetTopScores(int count)
		{
			var list = Load()?.entries ?? new List<ScoreEntry>();
			if (enableDebugLogs) Debug.Log($"[Ranking] GetTopScores({count}) returning {Mathf.Clamp(count,0,list.Count)} of {list.Count}");
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
				if (enableDebugLogs) Debug.Log("[Ranking] No save found. Starting fresh.");
			}
			else
			{
				try
				{
					cached = JsonUtility.FromJson<ScoreData>(json);
					if (cached == null) cached = new ScoreData();
					if (enableDebugLogs) Debug.Log($"[Ranking] Loaded {cached.entries.Count} entries.");
				}
				catch
				{
					cached = new ScoreData();
					if (enableDebugLogs) Debug.LogWarning("[Ranking] Load failed. Resetting.");
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
			if (enableDebugLogs) Debug.Log($"[Ranking] Saved entries={cached.entries.Count}");
		}
	}
}


