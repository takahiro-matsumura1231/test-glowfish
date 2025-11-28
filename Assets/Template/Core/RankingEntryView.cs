using TMPro;
using UnityEngine;

namespace Template.Core
{
	public class RankingEntryView : MonoBehaviour
	{
		[SerializeField] private TMP_Text rankText;
		[SerializeField] private TMP_Text scoreText;
		[SerializeField] private TMP_Text nameText;

		public void Bind(int rank, int score, string playerName)
		{
			if (rankText != null) rankText.text = rank.ToString();
			if (scoreText != null) scoreText.text = score.ToString();
			if (nameText != null) nameText.text = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
		}
	}
}


